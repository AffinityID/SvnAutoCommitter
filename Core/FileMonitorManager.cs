using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using log4net;
using SvnAutoCommitter.Core.Models;

namespace SvnAutoCommitter.Core {
    public class FileMonitorManager : IDisposable, IAsyncDisposable {
        private volatile bool _disposed = false;
        private readonly BlockingCollection<FileSystemEvent> _eventCollection;
        private readonly IFileSystemEventPoller _fileSystemEventPoller;
        private readonly ILog _logger;
        private readonly List<FileSystemWatcher> _watcherList;

        public FileMonitorManager(IFileSystemEventPoller fileSystemEventPoller, ILog logger) {
            _fileSystemEventPoller = fileSystemEventPoller;
            _logger = logger;
            _eventCollection = new BlockingCollection<FileSystemEvent>();
            _watcherList = new List<FileSystemWatcher>();
        }

        public BlockingCollection<FileSystemEvent> Queue {
            get { return _eventCollection; }
        }

        public void Monitor(IEnumerable<Folder> folders) {
            EnsureNotDisposed();
            WatchForChanges(folders);
        }

        public void StartProcessing() {
            EnsureNotDisposed();
            _fileSystemEventPoller.StartOnNewThread(_eventCollection);
        }

        private void WatchForChanges(IEnumerable<Folder> folders) {
            foreach (var folder in folders) {
                if (!Directory.Exists(folder.Path))
                    throw new Exception(string.Format("The directory {0} does not exist.", folder.Path));

                var watcher = new FileSystemWatcher();
                watcher.Path = folder.Path;
                watcher.Filter = "*";
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName |
                                        NotifyFilters.DirectoryName;
                watcher.InternalBufferSize = 65536;
                watcher.Created += SystemWatcherOnCreated;
                watcher.Changed += SystemWatcherOnChanged;
                watcher.Deleted += SystemWatcherOnDeleted;
                watcher.Renamed += SystemWatcherOnRenamed;
                watcher.Error += SystemWatcherOnError;
                watcher.EnableRaisingEvents = true;

                _watcherList.Add(watcher);
            }
        }

        private void SystemWatcherOnCreated(object sender, FileSystemEventArgs e) {
            _eventCollection.Add(new FileSystemEvent(e, DateTime.Now));
        }

        private void SystemWatcherOnChanged(object sender, FileSystemEventArgs e) {
            _eventCollection.Add(new FileSystemEvent(e, DateTime.Now));
        }

        private void SystemWatcherOnDeleted(object sender, FileSystemEventArgs e) {
            _eventCollection.Add(new FileSystemEvent(e, DateTime.Now));
        }

        private void SystemWatcherOnRenamed(object sender, RenamedEventArgs e) {
            _eventCollection.Add(new FileSystemEvent(e, DateTime.Now));
        }

        private void SystemWatcherOnError(object sender, ErrorEventArgs e) {
            _logger.Error("FileSystemWatcher error:" + e.GetException().Message, e.GetException());
        }

        private void EnsureNotDisposed() {
            if (_disposed)
                throw new ObjectDisposedException("FileMonitorManager");
        }

        public void Dispose() {
            DisposeAsync().Wait();
        }
        
        public Task DisposeAsync() {
            // at the moment, dispose is not thread-safe by itself
            if (_disposed) {
                // can be replaced with Task.FromResult in 4.5
                var source = new TaskCompletionSource<object>();
                source.SetResult(null);
                return source.Task;
            }

            foreach (var watcher in _watcherList) {
                watcher.Dispose();
            }
            _eventCollection.CompleteAdding();
            return _fileSystemEventPoller.DisposeAsync().ContinueWith(_ => {
                _eventCollection.Dispose();
                _disposed = true;
            });
        }
    }
}