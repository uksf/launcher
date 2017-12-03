using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;

namespace Patching {
    internal class DownloadManager {
        private const string USERNAME = "launcher";
        private const string PASSWORD = "sneakysnek";
        private const int MAX_CONCURRENT_DOWNLOADS = 5;
        private static DateTime _lastReport = DateTime.Now;
        private long _bytesDone, _bytesTotal, _bytesRate;

        private ConcurrentQueue<DownloadTask> _downloadQueue;
        private Task _downloadTask;

        public DownloadManager() => _downloadQueue = new ConcurrentQueue<DownloadTask>();

        public static event EventHandler<string> LogEvent;
        public static event EventHandler<Tuple<float, string>> ProgressEvent;

        public void AddDownloadTask(string localPath, string remotePath, CancellationToken downloadCancellationToken, Action callbackAction) {
            DownloadTask task = new DownloadTask(localPath, remotePath, downloadCancellationToken, callbackAction);
            _downloadQueue.Enqueue(task);
            task.DownloadProgressEvent += (sender, change) => ProgressAction(change);
            _bytesTotal += GetRemoteFileSize(remotePath);
        }

        public void ProcessDownloadQueue(CancellationToken downloadCancellationToken) {
            if (_downloadTask != null) return;
            // ReSharper disable once MethodSupportsCancellation
            Task.Run(() => {
                try {
                    _downloadTask = Task.Run(() => {
                        while (_downloadQueue.Count > 0 && !downloadCancellationToken.IsCancellationRequested) {
                            if (_downloadQueue.Count(downloadTask => downloadTask.Running()) >= MAX_CONCURRENT_DOWNLOADS) continue;
                            if (_downloadQueue.TryDequeue(out DownloadTask task)) task.StartDownloadTask();
                        }
                    }, downloadCancellationToken);
                    _downloadTask.Wait(downloadCancellationToken);
                } catch {
                    // ignored
                } finally {
                    _downloadTask = null;
                }
            });
        }

        public bool IsDownloadQueueEmpty() => _downloadQueue.Count == 0;

        public void ResetDownloadQueue() {
            foreach (DownloadTask downloadTask in _downloadQueue) {
                downloadTask.StopDownloadTask();
            }
            _downloadQueue = new ConcurrentQueue<DownloadTask>();
            _bytesDone = 0;
            _bytesTotal = 0;
            _bytesRate = 0;
        }

        private static void LogAction(string message) {
            LogEvent?.Invoke(null, message);
        }

        private void ProgressAction(long change) {
            _bytesDone += change;
            _bytesRate += change;
            if (DateTime.Now < _lastReport) return;
            _lastReport = DateTime.Now.AddMilliseconds(100);
            ProgressEvent?.Invoke(null,
                                  new Tuple<float, string>((float) _bytesDone / _bytesTotal,
                                                           $"Downloaded \n{ByteSize.FromBytes(_bytesDone)} / {ByteSize.FromBytes(_bytesTotal)} ({(int) (_bytesRate * 0.000078125)} Mbps)"));
            _bytesRate = 0L;
            // Mbps = rate * 80 / 1024 / 1000
        }

        private static long GetRemoteFileSize(string remotePath) {
            FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create($"ftp://uk-sf.com/{remotePath}");
            ftpWebRequest.UsePassive = true;
            ftpWebRequest.Credentials = new NetworkCredential(USERNAME, PASSWORD);
            ftpWebRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            return ftpWebRequest.GetResponse().ContentLength;
        }

        public static void UploadFile(string localPath, string remotePath, CancellationToken downloadCancellationToken, Action callBack) {
            Task.Run(() => {
                try {
                    LogAction($"File '{ByteSize.FromBytes(new FileInfo(localPath).Length)}'");
                    using (WebClient webClient = new WebClient()) {
                        downloadCancellationToken.Register(webClient.CancelAsync);
                        webClient.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                        webClient.UploadFile(new Uri(remotePath), "STOR", localPath);
                    }
                    LogAction($"Uploaded '{localPath}'");
                    callBack.Invoke();
                } catch (Exception exception) {
                    LogAction($"An error occured uploading '{localPath}'\n{exception}");
                } finally {
                    File.Delete(localPath);
                }
            }, downloadCancellationToken);
        }

        private class DownloadTask {
            private readonly Action _callbackAction;
            private readonly CancellationToken _downloadCancellationToken;
            private readonly string _localPath;
            private readonly string _remotePath;
            private Task _downloadTask;

            public DownloadTask(string localPath, string remotePath, CancellationToken downloadCancellationToken, Action callbackAction) {
                _localPath = localPath;
                _remotePath = remotePath;
                _downloadCancellationToken = downloadCancellationToken;
                _callbackAction = callbackAction;
                if (!Directory.Exists(Path.GetDirectoryName(_localPath))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(_localPath));
                }
            }

            public event EventHandler<long> DownloadProgressEvent;

            public void StartDownloadTask() {
                _downloadTask = Task.Run(() => {
                    while (!_downloadCancellationToken.IsCancellationRequested) {
                        if (!Download()) continue;
                        if (_downloadCancellationToken.IsCancellationRequested) break;
                        _callbackAction.Invoke();
                        break;
                    }
                }, _downloadCancellationToken);
            }

            public void StopDownloadTask() {
                if (_downloadTask == null) return;
                _downloadTask.Dispose();
                _downloadTask = null;
            }

            private bool Download() {
                try {
                    FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create($"ftp://uk-sf.com/{_remotePath}");
                    ftpWebRequest.KeepAlive = true;
                    ftpWebRequest.UsePassive = true;
                    ftpWebRequest.UseBinary = true;
                    ftpWebRequest.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                    ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    ftpWebRequest.Timeout = 60000;
                    byte[] buffer = new byte[4096];
                    using (FtpWebResponse response = (FtpWebResponse) ftpWebRequest.GetResponse()) {
                        using (FileStream fileStream = File.OpenWrite(_localPath)) {
                            using (Stream responseStream = response.GetResponseStream()) {
                                int count = -1;
                                while ((uint) count > 0U && !_downloadCancellationToken.IsCancellationRequested) {
                                    if (responseStream != null) count = responseStream.Read(buffer, 0, buffer.Length);
                                    DownloadProgressEvent.Invoke(this, count);
                                    fileStream.Write(buffer, 0, count);
                                }
                            }
                        }
                    }
                    LogAction($"Downloaded '{_localPath}'");
                    return true;
                } catch (OperationCanceledException) {
                    return true;
                } catch (WebException webException) {
                    if (webException.Status != WebExceptionStatus.Timeout) {
                        LogAction($"An error occured downloading '{_remotePath}'\n{webException}");
                    }
                    return false;
                } catch (Exception exception) {
                    LogAction($"An error occured downloading '{_remotePath}'\n{exception}");
                    return false;
                }
            }

            public bool Running() => !_downloadTask?.IsCompleted == true;
        }
    }
}