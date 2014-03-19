using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace SvnAutoCommitter.Core {
    public class FileSystemItem {
        private readonly string _fullPath = "";
        private FileSystemInfo _entry;

        public FileSystemItem(string fullPath) {
            _fullPath = fullPath;
            Refresh();
        }

        public void Refresh() {
            var directory = new DirectoryInfo(_fullPath);
            if (directory.Exists) {
                _entry = directory;
            }

            _entry = new FileInfo(_fullPath);
        }

        public string FullPath {
            get { return _fullPath; }
        }

        public bool IsFile {
            get { return (_entry is FileInfo) && _entry.Exists; }
        }

        public bool IsFolder {
            get { return _entry is DirectoryInfo; }
        }

        public bool Exists {
            get { return _entry.Exists; }
        }

        public bool IsTemp {
            get { return _entry.Attributes != (FileAttributes)(-1) && _entry.Attributes.HasFlag(FileAttributes.Temporary); }
        }

        public bool IsHidden {
            get { return _entry.Attributes != (FileAttributes)(-1) && _entry.Attributes.HasFlag(FileAttributes.Hidden); }
        }

        //Ignore umbraco temp files, svn files, hidden files and folders and temp file and folders
        public bool ShouldBeIgnored {
            get { return Regex.IsMatch(FullPath, "\\\\\\d{18}_") || FullPath.Contains(".svn") || IsHidden || IsTemp; }
        }

        public bool IsFileReadyToRead(int maxTry) {
            var attempt = 0;
            var waitBeforeTry = 1000;
            while (attempt < maxTry) {
                attempt++;
                try {
                    using (new StreamReader(FullPath)) {
                        return true;
                    }
                }
                catch {
                    Thread.Sleep(waitBeforeTry);
                }
            }

            return false;
        }
    }
}