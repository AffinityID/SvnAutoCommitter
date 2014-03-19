namespace SvnAutoCommitter.Core {
    public interface IFileSystemEventWorker {
        void Work(FileSystemEvent e);
    }
}