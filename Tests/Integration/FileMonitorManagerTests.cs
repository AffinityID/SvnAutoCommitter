using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Moq;
using NUnit.Framework;
using SvnAutoCommitter.Core;
using SvnAutoCommitter.Core.Models;
using SvnAutoCommitter.Tests.Integration.Helpers;

namespace SvnAutoCommitter.Tests.Integration {
    [TestFixture]
    public class FileMonitorManagerTests {
        private IList<Folder> _folders;
        private FileMonitorManager _manager;
            
        [SetUp]
        public void BeforeEachTest() {
            _manager = new FileMonitorManager(
                Mock.Of<IFileSystemEventPoller>(p => p.DisposeAsync() == Task.FromResult<object>(null)),
                LogManager.GetLogger(typeof(FileMonitorManager))
            );
            _folders = TestFileSystemHelper.CreateAndGetTestFolders();
        }

        [TearDown]
        public void AfterEachTest() {
            var disposed = _manager.DisposeAsync().Wait(TimeSpan.FromMinutes(2));
            Assert.IsTrue(disposed, "Manager dispose operation did not complete in time.");
        }
        
        [Test]
        [TestCase(1, 1)]
        [TestCase(5, 5)]
        public void ChangeEvents_AreQueued_WhenFilesAreChanged(int numOfFiles, int expectedValue) {
            _manager.Monitor(_folders);

            var folder = _folders[0];
            TestFileSystemHelper.CreateDummyFiles(folder.Path, numOfFiles);
            Parallel.ForEach(Directory.GetFiles(folder.Path), file => File.WriteAllText(file, "Hello World"));

            AssertQueueItemCount(expectedValue, item => item.EventArgs.ChangeType == WatcherChangeTypes.Changed);
        }

        [Test]
        [TestCase(1, 1, 1)]
        [TestCase(10, 2, 20)]
        [TestCase(50, 3, 150)]
        [TestCase(100, 4, 400)]
        public void CreateEvents_AreQueued_WhenFilesAreCreatedInDifferntFolders(int numOfFiles, int numOfFolders, int expectedValue) {
            _manager.Monitor(_folders);

            Parallel.For(0, numOfFolders, i => TestFileSystemHelper.CreateDummyFiles(_folders[i].Path, numOfFiles));

            AssertQueueItemCount(expectedValue, item => item.EventArgs.ChangeType == WatcherChangeTypes.Created);
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(10, 10)]
        [TestCase(50, 50)]
        public void CreateEvents_AreQueued_WhenFilesAreCreatedInOneFolder(int numOfFiles, int expectedValue) {
            _manager.Monitor(_folders);
            TestFileSystemHelper.CreateDummyFiles(_folders[0].Path, numOfFiles);
            
            AssertQueueItemCount(expectedValue, item => item.EventArgs.ChangeType == WatcherChangeTypes.Created);
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(10, 10)]
        public void DeleteEvents_AreQueued_WhenFilesAreDeleted(int numOfFiles, int expectedValue) {
            _manager.Monitor(_folders);

            var folder = _folders[0];
            TestFileSystemHelper.CreateDummyFiles(folder.Path, numOfFiles);
            foreach (var file in Directory.GetFiles(folder.Path)) {
                File.Delete(file);
            }

            AssertQueueItemCount(expectedValue, item => item.EventArgs.ChangeType == WatcherChangeTypes.Deleted);
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(10, 10)]
        public void RenameEvents_AreQueued_WhenFilesAreRenamed(int numOfFiles, int expectedValue) {
            _manager.Monitor(_folders);

            var folder = _folders[0];
            TestFileSystemHelper.CreateDummyFiles(folder.Path, numOfFiles);

            foreach (var file in Directory.GetFiles(folder.Path)) {
                File.Move(file, file.Replace("dummy", "notdummy"));
            }

            AssertQueueItemCount(expectedValue, item => item.EventArgs.ChangeType == WatcherChangeTypes.Renamed);
        }

        private void AssertQueueItemCount(int expectedCount, Func<FileSystemEvent, bool> predicate) {
            var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var actualCount = 0;
            try {
                foreach (var item in _manager.Queue.GetConsumingEnumerable(cancellationSource.Token)) {
                    if (!predicate(item))
                        continue;

                    actualCount += 1;
                    if (actualCount == expectedCount)
                        return;
                }
            }
            catch (OperationCanceledException) {
                Assert.AreEqual(expectedCount, actualCount);
            }
        }
    }
}