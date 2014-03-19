using System;

namespace SvnAutoCommitter.Core.Models {
    public class Folder {
        public Folder() {}

        public Folder(string path, Uri repositoryUrl) {
            Path = path;
            RepositoryUrl = repositoryUrl;
        }

        public string Path { get; set; }
        public Uri RepositoryUrl { get; set; }
    }
}