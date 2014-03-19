using System;
using System.IO;
using SharpSvn;

namespace SvnAutoCommitter.Tests.Integration.Helpers {
    public static class TestRepositoryHelper {
        public static string RepositoryPath {
            get { return Path.Combine(TestFileSystemHelper.TestRootPath, "repository"); }
        }

        public static Uri RepositoryUrl {
            get { return new Uri(RepositoryPath); }
        }

        public static void SetupRepository() {
            TestFileSystemHelper.RecreateDirectory(RepositoryPath);
            using (var client = new SvnRepositoryClient()) {
                client.CreateRepository(RepositoryPath);
            }
        }

        public static void CreateDirectoryInRepository(Uri url) {
            using (var client = new SvnClient()) {
                client.RemoteCreateDirectory(url, new SvnCreateDirectoryArgs {
                    LogMessage = "Test create " + url
                });
            }
        }
    }
}
