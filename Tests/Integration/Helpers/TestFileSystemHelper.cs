using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SvnAutoCommitter.Core.Models;

namespace SvnAutoCommitter.Tests.Integration.Helpers {
    public static class TestFileSystemHelper {
        private static string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string TestRootPath {
            get { return Path.Combine(AssemblyPath, "Test"); }
        }

        public static string WatchRootPath {
            get { return Path.Combine(TestRootPath, "watched"); }
        }
        
        public static void CreateDummyFiles(string directoryPath, int numOfFiles) {
            if (numOfFiles == 0) return;
            for (var i = 1; i <= numOfFiles; i++) {
                CreateFile(String.Format("\\dummyFile{0}.txt", i), directoryPath);
            }
        }

        public static void CreateFile(string fileName, string directoryPath) {
            var filePath = String.Format("{0}\\{1}", directoryPath, fileName);
            using (File.Create(filePath)) { }
        }

        public static string MakeUniqueFileName(string ext) {
            return "main_" + Guid.NewGuid().ToString("N") + "." + ext;
        }

        public static IList<Folder> CreateAndGetTestFolders(Uri rootRepositoryUrl = null) {
            rootRepositoryUrl = rootRepositoryUrl ?? new Uri("svn://dummy");
            var folders = new List<Folder> {
                new Folder(WatchRootPath + "\\css", new Uri(rootRepositoryUrl + "/css")),
                new Folder(WatchRootPath + "\\xslt", new Uri(rootRepositoryUrl + "/xslt")),
                new Folder(WatchRootPath + "\\scripts", new Uri(rootRepositoryUrl + "/usync")),
                new Folder(WatchRootPath + "\\masterpages", new Uri(rootRepositoryUrl + "/masterpages"))
            };

            foreach (var folder in folders) {
                RecreateDirectory(folder.Path);
            }

            return folders;
        }

        public static void RecreateDirectory(string path) {
            if (Directory.Exists(path)) {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }
    }
}