using System.IO;
using log4net;

namespace SvnAutoCommitter.Core {
    public class FileSystemEventWorker : IFileSystemEventWorker {
        private const int MaxFileAttempt = 10;
        private readonly ILog _logger;
        private readonly ISvnClient _svnClient;

        public FileSystemEventWorker(ISvnClient svnClient, ILog logger) {
            _svnClient = svnClient;
            _logger = logger;
        }

        public void Work(FileSystemEvent e) {
            switch (e.EventArgs.ChangeType) {
                case WatcherChangeTypes.Created:
                    WorkOnCreateEvent(e);
                    break;
                case WatcherChangeTypes.Changed:
                    WorkOnChangeEvent(e);
                    break;
                case WatcherChangeTypes.Deleted:
                    WorkOnDeleteEvent(e);
                    break;
                case WatcherChangeTypes.Renamed:
                    WorkOnRenameEvent(e);
                    break;
            }
        }

        private void WorkOnCreateEvent(FileSystemEvent fileSystemEvent) {
            var fileSystemObject = new FileSystemItem(fileSystemEvent.EventArgs.FullPath);
            if (!fileSystemObject.Exists || fileSystemObject.ShouldBeIgnored) return;
            _logger.InfoFormat("New {0} is created on {1}", fileSystemObject.FullPath,
                fileSystemEvent.OccurredTime.ToString("dd-MM-yyyy hh:mm:ss"));


            if (fileSystemObject.IsFile && !fileSystemObject.IsFileReadyToRead(MaxFileAttempt)) {
                _logger.ErrorFormat(
                    "It has exceeded the max number of attempts. File {0} is in use and cannot be read",
                    fileSystemObject.FullPath);
                return;
            }

            if (_svnClient.SvnAdd(fileSystemObject.FullPath)) {
                _svnClient.SvnCommit(fileSystemObject.FullPath);
            }
        }

        private void WorkOnChangeEvent(FileSystemEvent fileSystemEvent) {
            var fileSystemObject = new FileSystemItem(fileSystemEvent.EventArgs.FullPath);
            if (!fileSystemObject.Exists || fileSystemObject.ShouldBeIgnored || fileSystemObject.IsFolder) return;

            _logger.InfoFormat("{0} is changed on {1}", fileSystemObject.FullPath,
                fileSystemEvent.OccurredTime.ToString("dd-MM-yyyy hh:mm:ss"));

            if (fileSystemObject.IsFile && !fileSystemObject.IsFileReadyToRead(MaxFileAttempt)) {
                _logger.ErrorFormat(
                    "It has exceeded the max number of attemptes. File {0} is in use and cannot be read",
                    fileSystemObject.FullPath);
                return;
            }
            _svnClient.SvnCommit(fileSystemObject.FullPath);
        }

        private void WorkOnDeleteEvent(FileSystemEvent fileSystemEvent) {
            var fileSystemObject = new FileSystemItem(fileSystemEvent.EventArgs.FullPath);
            if (fileSystemObject.Exists || fileSystemObject.ShouldBeIgnored) return;

            _logger.InfoFormat("{0} is deleted on {1}", fileSystemObject.FullPath,
                fileSystemEvent.OccurredTime.ToString("dd-MM-yyyy hh:mm:ss"));

            _svnClient.SvnDelete(fileSystemObject.FullPath);
            _svnClient.SvnCommit(fileSystemObject.FullPath);
        }

        private void WorkOnRenameEvent(FileSystemEvent fileSystemEvent) {
            var fileSystemObject = new FileSystemItem(fileSystemEvent.EventArgs.FullPath);
            var fileSystemObjectOld = new FileSystemItem(((RenamedEventArgs) fileSystemEvent.EventArgs).OldFullPath);


            if (fileSystemObjectOld.ShouldBeIgnored || fileSystemObject.ShouldBeIgnored) return;

            _logger.InfoFormat("{0} is renamed to {1} on {2}", fileSystemObjectOld.FullPath,
                fileSystemObject.FullPath,
                fileSystemEvent.OccurredTime.ToString("dd-MM-yyyy hh:mm:ss"));
            if (fileSystemObjectOld.Exists) return;
            _svnClient.SvnDelete(fileSystemObjectOld.FullPath);
            _svnClient.SvnCommit(fileSystemObjectOld.FullPath);

            if (fileSystemObject.IsFile && !fileSystemObject.IsFileReadyToRead(MaxFileAttempt)) {
                _logger.ErrorFormat(
                    "It has exceeded the max number of attemptes. File {0} is in use and cannot be read",
                    fileSystemObject.FullPath);
                return;
            }
            if (_svnClient.SvnAdd(fileSystemObject.FullPath)) {
                _svnClient.SvnCommit(fileSystemObject.FullPath);
            }
        }
    }
}