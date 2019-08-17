using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;
using GBCLV3.Models;

namespace GBCLV3.Services
{
    class DownloadService : IDisposable
    {
        #region Events

        public event Action<DownloadResult> Completed;

        public event Action<DownloadProgressEventArgs> ProgressChanged;

        #endregion

        #region Private Members

        private const int BUFFER_SIZE = 4096; // byte
        private const double INFO_UPDATE_INTERVAL = 0.5; // second

        private readonly HttpClient _client;
        private readonly DispatcherTimer _timer;
        private readonly CancellationTokenSource _cts;
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
            _client = new HttpClient() { Timeout = TimeSpan.FromMinutes(3.0) };
            _client.DefaultRequestHeaders.Connection.Add("keep-alive");

            _cts = new CancellationTokenSource();
            _sync = new AutoResetEvent(true);

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(INFO_UPDATE_INTERVAL)
            };

            // Update download progress and raise events
            _timer.Tick += (sender, e) => ProgressChanged?.Invoke(GetDownloadProgress());

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

        public async Task<bool> StartAsync()
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            for (;;)
            {
                _timer.Start();

                await Task.Factory.StartNew(() =>
                {
                    Parallel.ForEach(_downloadItems, options, (item, state) =>
                    {
                        DownloadTask(item);
                        // 所以啊...止まるんじゃねぇぞ！
                        if (_cts.IsCancellationRequested) state.Stop();
                    });
                }, TaskCreationOptions.LongRunning);

                _timer.Stop();
                // Ensure the last progress report is fired
                ProgressChanged?.Invoke(GetDownloadProgress());

                // Succeeded
                if (_completedCount == _totalCount)
                {
                    Completed?.Invoke(DownloadResult.Succeeded);
                    return true;
                }

                if (_failledCount > 0 && !_cts.IsCancellationRequested)
                {
                    Completed?.Invoke(DownloadResult.Incomplete);
                }

                // Wait for retry or cancel
                _sync.WaitOne();

                // Canceled
                if (_cts.IsCancellationRequested)
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
            _downloadItems.ForEach(item => item.DownloadedBytes = 0);
            _failledCount = 0;

            _sync.Set();
        }

        /// <summary>
        /// Cancel ongoing download task
        /// </summary>
        public void Cancel()
        {
            _cts.Cancel();
            _timer.Stop();
            _sync.Set();
        }

        public void Dispose()
        {
            _client.Dispose();
            _cts.Dispose();
            _sync.Dispose();
        }

        #endregion

        #region Private Methods

        private void DownloadTask(DownloadItem item)
        {
            string downloadDir = Path.GetDirectoryName(item.Path);
            if (!Directory.Exists(downloadDir))
            {
                Directory.CreateDirectory(downloadDir);
            }

            var fs = new FileStream(item.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, BUFFER_SIZE);

            try
            {
                var waitResponceTask = _client.GetStreamAsync(item.Url);

                waitResponceTask.Wait(_cts.Token);
                var httpStream = waitResponceTask.Result;
                _cts.Token.Register(() => httpStream.Close());

                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesReceived;

                while ((bytesReceived = httpStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                {
                    fs.Write(buffer, 0, bytesReceived);
                    item.DownloadedBytes += bytesReceived;
                    Interlocked.Add(ref _downloadedBytes, bytesReceived);
                }

                item.IsCompleted = true;
                Interlocked.Increment(ref _completedCount);
            }
            catch (OperationCanceledException ex)
            {
                // You are the one who canceled me :D
                Debug.WriteLine(ex.ToString());
            }
            catch (AggregateException ex) when (ex.InnerException is HttpRequestException)
            {
                Debug.WriteLine(ex.InnerException.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                fs.Close();
                // Handle unfinished download
                if (!item.IsCompleted)
                {
                    File.Delete(item.Path);
                    // Make sure the exception is not caused by cancellation
                    if (!_cts.IsCancellationRequested)
                    {
                        Interlocked.Increment(ref _failledCount);
                        Interlocked.Add(ref _downloadedBytes, -item.DownloadedBytes);
                    }
                }
            }
        }

        private DownloadProgressEventArgs GetDownloadProgress()
        {
            // Calculate speed
            int diffBytes = _downloadedBytes - _previousDownloadedBytes;
            _previousDownloadedBytes = _downloadedBytes;

            return new DownloadProgressEventArgs
            {
                TotalCount = _totalCount,
                CompletedCount = _completedCount,
                FailedCount = _failledCount,
                TotalBytes = _totalBytes,
                DownloadedBytes = _downloadedBytes,
                Speed = diffBytes / INFO_UPDATE_INTERVAL,
            };
        }

        #endregion
    }
}
