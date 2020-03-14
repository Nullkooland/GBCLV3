using GBCLV3.Models.Download;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;

namespace GBCLV3.Services.Download
{
    public class DownloadService : IDisposable
    {
        #region Events

        public event Action<DownloadResult> Completed;

        public event Action<DownloadProgress> ProgressChanged;

        #endregion

        #region Private Fields

        // private const int MAX_DEGREE_OF_PARALLELISM = 16;
        private const int MAX_RETRY_COUNT = 3;
        private const int BUFFER_SIZE = 4096; // byte
        private const double UPDATE_INTERVAL = 1.0; // second

        private readonly HttpClient _client;
        private readonly ArrayPool<byte> _bufferPool;
        private readonly ExecutionDataflowBlockOptions _parallelOptions;
        private readonly DispatcherTimer _timer;
        private readonly CancellationTokenSource _userCts;
        private readonly AutoResetEvent _sync;

        private List<DownloadItem> _downloadItems;

        private int _totalBytes;
        private int _downloadedBytes;
        private int _previousDownloadedBytes;

        private int _totalCount;
        private int _completedCount;
        private int _failledCount;

        #endregion

        #region Constructor

        public DownloadService(IEnumerable<DownloadItem> downloadItems)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Connection.Add("keep-alive");

            _bufferPool = ArrayPool<byte>.Create(BUFFER_SIZE, Environment.ProcessorCount * 2);

            _userCts = new CancellationTokenSource();
            _sync = new AutoResetEvent(true);

            _parallelOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                CancellationToken = _userCts.Token,
            };

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(UPDATE_INTERVAL)
            };

            // Update download progress and raise events
            _timer.Tick += (sender, e) => UpdateDownloadProgress();

            // Initialize states
            _downloadItems = downloadItems.ToList();
            _totalBytes = _downloadItems.Sum(item => item.Size);
            _downloadedBytes = 0;
            _previousDownloadedBytes = 0;

            _totalCount = _downloadItems.Count();
            _completedCount = 0;
            _failledCount = 0;
        }

        #endregion

        #region Public Methods

        public async ValueTask<bool> StartAsync()
        {
            for (;;)
            {
                _timer.Start();

                try
                {
                    var downloader = new ActionBlock<DownloadItem>(async item =>
                    {
                        for (int i = 0; i < MAX_RETRY_COUNT; i++)
                        {
                            if (await DownloadTask(item, i + 1)) break;
                        }
                    }, _parallelOptions);

                    foreach (var item in _downloadItems)
                    {
                        downloader.Post(item);
                    }

                    downloader.Complete();
                    await downloader.Completion;
                }
                catch (OperationCanceledException)
                {
                    // You are the one who canceled me :D
                    Debug.WriteLine("Download canceled!");
                }

                _timer.Stop();
                // Ensure the last progress report is fired
                UpdateDownloadProgress();

                // Succeeded
                if (_completedCount == _totalCount)
                {
                    Completed?.Invoke(DownloadResult.Succeeded);
                    return true;
                }

                // Clean incomplete files
                foreach (var item in _downloadItems)
                {
                    if (!item.IsCompleted && File.Exists(item.Path))
                    {
                        File.Delete(item.Path);
                    }
                }

                if (_failledCount > 0 && !_userCts.IsCancellationRequested)
                {
                    Completed?.Invoke(DownloadResult.Incomplete);
                }

                // Wait for retry or cancel
                _sync.WaitOne();

                // Canceled
                if (_userCts.IsCancellationRequested)
                {
                    Completed?.Invoke(DownloadResult.Canceled);
                    return false;
                }
            }
        }

        /// <summary>
        /// If previous download task is not fully completed (error occurred on some items)
        /// </summary>
        public void Retry()
        {
            _downloadItems = _downloadItems.Where(item => !item.IsCompleted).ToList();
            _failledCount = 0;

            _sync.Set();
        }

        /// <summary>
        /// Cancel ongoing download task
        /// </summary>
        public void Cancel()
        {
            _userCts.Cancel();
            _timer.Stop();
            _sync.Set();
        }

        public void Dispose()
        {
            _client.Dispose();
            _userCts.Dispose();
            _sync.Dispose();
        }

        #endregion

        #region Private Methods

        private async ValueTask<bool> DownloadTask(DownloadItem item, int retryTimes)
        {
            if (_userCts.IsCancellationRequested) return true;

            // Make sure directory exists
            if (Path.IsPathRooted(item.Path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item.Path));
            }

            var buffer = _bufferPool.Rent(BUFFER_SIZE);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, item.Url);
                var response =
                    await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _userCts.Token);

                if (response.StatusCode == HttpStatusCode.Found)
                {
                    // Handle redirection
                    request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
                    response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                        _userCts.Token);
                }

                if (item.Size == 0)
                {
                    item.Size = (int) (response.Content.Headers.ContentLength ?? 0);
                    Interlocked.Add(ref _totalBytes, item.Size);
                }

                item.IsPartialContentSupported = response.Headers.AcceptRanges.Contains("bytes");

                await using var httpStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.OpenWrite(item.Path);

                var timeout = TimeSpan.FromSeconds(Math.Max(item.Size / 16384.0, 30.0));
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var readCts = CancellationTokenSource.CreateLinkedTokenSource(_userCts.Token, timeoutCts.Token);

                int bytesReceived;
                while ((bytesReceived = await httpStream.ReadAsync(buffer, readCts.Token)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesReceived);
                    item.DownloadedBytes += bytesReceived;
                    Interlocked.Add(ref _downloadedBytes, bytesReceived);
                }

                // Download successful
                item.IsCompleted = true;
                Interlocked.Increment(ref _completedCount);

                request.Dispose();
                response.Dispose();
                return true;
            }
            catch (OperationCanceledException)
            {
                if (_userCts.IsCancellationRequested)
                {
                    Debug.WriteLine("Download canceled by user");
                }
                else
                {
                    Debug.WriteLine("Download read chunk timeout");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                _bufferPool.Return(buffer);
            }

            // If is not caused by cancellation, mark as failure
            if (!_userCts.IsCancellationRequested)
            {
                Interlocked.Increment(ref _failledCount);

                // 全  部  木  大
                Interlocked.Add(ref _downloadedBytes, -item.DownloadedBytes);
                item.DownloadedBytes = 0;
                Interlocked.Exchange(ref _previousDownloadedBytes, _downloadedBytes);
            }

            return false; // We're not done yet, prepare for retry
        }

        private void UpdateDownloadProgress()
        {
            // Calculate speed
            int diffBytes = _downloadedBytes - _previousDownloadedBytes;
            _previousDownloadedBytes = _downloadedBytes;

            var progress = new DownloadProgress
            {
                TotalCount = _totalCount,
                CompletedCount = _completedCount,
                FailedCount = _failledCount,
                TotalBytes = _totalBytes,
                DownloadedBytes = _downloadedBytes,
                Speed = diffBytes / UPDATE_INTERVAL,
            };

            ProgressChanged?.Invoke(progress);
        }

        #endregion
    }
}