using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using log4net;

namespace SvnAutoCommitter.Core {
    public class FileSystemEventPoller : IFileSystemEventPoller {
        private readonly ILog _logger;
        private Task _task;
        private readonly IFileSystemEventWorker _fileSystemEventWorker;

        public FileSystemEventPoller(IFileSystemEventWorker fileSystemEventWorker, ILog logger) {
            _fileSystemEventWorker = fileSystemEventWorker;
            _logger = logger;
        }

        public void StartOnNewThread(BlockingCollection<FileSystemEvent> eventCollection) {
            if (_task != null)
                throw new InvalidOperationException("Poller has already been started.");

            _task = Task.Factory.StartNew(
                () => PollEventFromQueue(eventCollection),
                TaskCreationOptions.LongRunning
            );
        }

        private void PollEventFromQueue(BlockingCollection<FileSystemEvent> eventCollection) {
            _logger.Info("Starting to poll for events.");
            foreach (var e in eventCollection.GetConsumingEnumerable()) {
                _fileSystemEventWorker.Work(e);
            }
        }

        public void Dispose() {
            _task.Wait();
        }

        public Task DisposeAsync() {
            return _task;
        }
    }
}