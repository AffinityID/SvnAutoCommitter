using System;
using System.IO;

namespace SvnAutoCommitter.Core {
    public class FileSystemEvent {
        public FileSystemEvent(FileSystemEventArgs eventArgs, DateTime occurredTime) {
            EventArgs = eventArgs;
            OccurredTime = occurredTime;
        }

        public FileSystemEventArgs EventArgs { get; private set; }
        public DateTime OccurredTime { get; private set; }
    }
}