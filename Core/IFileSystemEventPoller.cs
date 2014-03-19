using System;
using System.Collections.Concurrent;

namespace SvnAutoCommitter.Core {
    public interface IFileSystemEventPoller : IDisposable, IAsyncDisposable {
        void StartOnNewThread(BlockingCollection<FileSystemEvent> eventQueue);
    }
}