using GBCLV3.Models.Download;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private readonly AutoResetEvent _autoResetEvent;
        private readonly LogService _logService;

        private CancellationTokenSource _userCts;
        private ImmutableList<DownloadItem> _downloadItems;

        private int _totalBytes;
        private int _downloadedBytes;
        private int _previousDownloadedBytes;

        private int _totalCount;
        private int _completedCount;
        private int _failedCount;

        #endregion

        #region Constructor

        public DownloadService(LogService logService)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Connection.Add("keep-alive");

            _bufferPool = ArrayPool<byte>.Create(BUFFER_SIZE, Environment.ProcessorCount * 2);
            _autoResetEvent = new AutoResetEvent(true);

            _parallelOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            };

            _userCts = new CancellationTokenSource();
            _parallelOptions.CancellationToken = _userCts.Token;

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(UPDATE_INTERVAL)
            };

            // Update download progress and raise events
            _timer.Tick += (sender, e) => UpdateDownloadProgress();

            _logService = logService;
        }

        #endregion

        #region Public Methods

        public void Setup(IEnumerable<DownloadItem> downloadItems)
        {
            // Initialize states
            _downloadItems = downloadItems.ToImmutableList();
            _totalBytes = _downloadItems.Sum(item => item.Size);
            _downloadedBytes = 0;
            _previousDownloadedBytes = 0;

            _totalCount = _downloadItems.Count();
            _completedCount = 0;
            _failedCount = 0;

            if (_userCts.IsCancellationRequested)
            {
                _userCts.Dispose();
                _userCts = new CancellationTokenSource();
                _parallelOptions.CancellationToken = _userCts.Token;
            }

            _autoResetEvent.Reset();

            _logService.Info(nameof(DownloadService), $"New downloads added. Count: {_totalCount} Size: {_totalBytes} bytes");
        }

        public async ValueTask<bool> StartAsync()
        {
            while (true)
            {
                _logService.Info(nameof(DownloadService), "Starting all downloads");
                _timer.Start();

                try
                {
                    var downloader = new ActionBlock<DownloadItem>(async item =>
                    {
                        for (int i = 0; i < MAX_RETRY_COUNT && !_userCts.IsCancellationRequested; i++)
                        {
                            if (await DownloadItemAsync(item, i)) break;
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
                    //_logService.Info(nameof(DownloadService), "Download canceled");
                }

                _timer.Stop();
                // Ensure the last progress report is fired
                UpdateDownloadProgress();

                // Succeeded
                if (_completedCount == _totalCount)
                {
                    _logService.Info(nameof(DownloadService), "All downloads successful");

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

                if (_failedCount > 0 && !_userCts.IsCancellationRequested)
                {
                    _logService.Info(nameof(DownloadService), "Downloads incomplete");

                    Completed?.Invoke(DownloadResult.Incomplete);
                }

                // Wait for retry or cancel
                _autoResetEvent.WaitOne();

                // Canceled
                if (_userCts.IsCancellationRequested)
                {
                    _logService.Info(nameof(DownloadService), "Downloads canceled");

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
            _logService.Info(nameof(DownloadService), "Retrying incomplete downloads");

            _downloadItems = _downloadItems.Where(item => !item.IsCompleted).ToImmutableList();
            _failedCount = 0;

#if DEBUG
            int remainingCount = _totalCount - _completedCount;
            int remainingBytes = _downloadItems.Sum(item => item.Size);
            _logService.Debug(nameof(DownloadService), $"Remaining items count: {remainingCount}.");
            _logService.Debug(nameof(DownloadService), $"Remaining items size (Bytes): {remainingBytes}.");
#endif
            _autoResetEvent.Set();
        }

        /// <summary>
        /// Cancel ongoing download task
        /// </summary>
        public void Cancel()
        {
            _logService.Info(nameof(DownloadService), "Canceling downloads");

            _userCts.Cancel();
            _timer.Stop();
            _autoResetEvent.Set();
        }

        public void Dispose()
        {
#if DEBUG
            _logService.Debug(nameof(DownloadService), "Disposed");
#endif
            _client.Dispose();
            _userCts.Dispose();
            _autoResetEvent.Dispose();
        }

        #endregion

        #region Private Methods

        private async ValueTask<bool> DownloadItemAsync(DownloadItem item, int retryTimes)
        {
            if (_userCts.IsCancellationRequested) return true;

            if (retryTimes > 0)
            {
                _logService.Warn(nameof(DownloadService), $"{item.Url}: Retrying {retryTimes} times");
            }

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
                    item.Size = (int)(response.Content.Headers.ContentLength ?? 0);
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
                if (!_userCts.IsCancellationRequested)
                {
                    _logService.Error(nameof(DownloadService), $"{item.Url}: Timeout");
                }
            }
            catch (HttpRequestException ex)
            {
                _logService.Error(nameof(DownloadService), $"{item.Url}: HTTP error occurred.\n{ex.Message}");
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(DownloadService), $"{item.Url}: Unkown error occurred.\n{ex.Message}");
            }
            finally
            {
                _bufferPool.Return(buffer);
            }

            // If is not caused by cancellation, mark as failure
            if (!_userCts.IsCancellationRequested)
            {
                Interlocked.Increment(ref _failedCount);

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
                FailedCount = _failedCount,
                TotalBytes = _totalBytes,
                DownloadedBytes = _downloadedBytes,
                Speed = diffBytes / UPDATE_INTERVAL,
            };

            ProgressChanged?.Invoke(progress);
        }

        #endregion
    }
}