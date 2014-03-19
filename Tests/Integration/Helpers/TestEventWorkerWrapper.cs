using System;
using System.Threading.Tasks;
using log4net;
using SvnAutoCommitter.Core;

namespace SvnAutoCommitter.Tests.Integration.Helpers {
    public class TestEventWorkerWrapper : IFileSystemEventWorker {
        private readonly IFileSystemEventWorker _inner;
        private readonly ILog _logger;
        private TaskCompletionSource<FileSystemEvent> _nextTaskSource;

        public TestEventWorkerWrapper(IFileSystemEventWorker inner, ILog logger) {
            _inner = inner;
            _logger = logger;
        }

        public void Work(FileSystemEvent e) {
            _inner.Work(e);
            if (e.EventArgs.FullPath.Contains(".svn"))
                return;

            _logger.DebugFormat("Work completed for {0}.", e.EventArgs.Name);

            if (_nextTaskSource != null) {
                _nextTaskSource.SetResult(e);
                _nextTaskSource = null;
            }
        }

        public void BeforeNextWork() {
            if (_nextTaskSource != null)
                throw new InvalidOperationException("Previous work has not completed yet.");

            // obviously not thread safe, but ok for tests
            _nextTaskSource = new TaskCompletionSource<FileSystemEvent>();
        }

        public Task<FileSystemEvent> NextWorkCompleted() {
            return _nextTaskSource.Task;
        }
    }
}
