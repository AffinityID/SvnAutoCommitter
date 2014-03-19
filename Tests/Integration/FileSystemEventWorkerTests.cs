using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using log4net;
using log4net.Core;
using Moq;
using NUnit.Framework;
using SharpSvn;
using SvnAutoCommitter.Core;
using SvnAutoCommitter.Core.Models;
using SvnAutoCommitter.Tests.Integration.Helpers;

namespace SvnAutoCommitter.Tests.Integration {
    [TestFixture]
    public class FileSystemEventWorkerTests {
        private IList<Folder> _testFolders;
        private FileMonitorManager _manager;
        private TestEventWorkerWrapper _workerWrapper;

        [SetUp]
        public void BeforeEachTest() {
            var sharpSvnClient = new SharpSvnClient(new NetworkCredential(), LogFor<SharpSvnClient>());
            _workerWrapper = new TestEventWorkerWrapper(new FileSystemEventWorker(sharpSvnClient, LogFor<FileSystemEventWorker>()), LogFor<TestEventWorkerWrapper>());
            _manager = new FileMonitorManager(new FileSystemEventPoller(_workerWrapper, LogFor<FileSystemEventPoller>()), LogFor<FileMonitorManager>());

            var repositoryUrl = TestRepositoryHelper.RepositoryUrl;
            TestRepositoryHelper.SetupRepository();

            //Create folders
            _testFolders = TestFileSystemHelper.CreateAndGetTestFolders(repositoryUrl);
            foreach (var folder in _testFolders) {
                TestRepositoryHelper.CreateDirectoryInRepository(folder.RepositoryUrl);
                if (!sharpSvnClient.SvnCheckOut(folder.RepositoryUrl, folder.Path))
                    throw new InvalidOperationException("Failed to checkout " + folder.Path + ".");
            }
        }

        [TearDown]
        public void AfterEachTest() {
            if (_manager == null)
                return;

            var disposed = _manager.DisposeAsync().Wait(TimeSpan.FromMinutes(1));
            Assert.IsTrue(disposed, "Failed to dispose FileMonitorManager.");
        }

        [Test]
        public void EventWorker_CommitsChangeToSvn_WhenFileIsChanged() {
            _manager.Monitor(_testFolders);
            _manager.StartProcessing();

            var folder = _testFolders[0];
            var fileName = TestFileSystemHelper.MakeUniqueFileName("css");
            DoAndWaitForWorker(() => TestFileSystemHelper.CreateFile(fileName, folder.Path));

            var filePath = folder.Path + "\\" + fileName;

            using (var client = new SvnClient()) {
                SvnInfoEventArgs info;
                client.GetInfo(folder.RepositoryUrl, out info);
                var revisionBeforeChange = info.Revision;

                DoAndWaitForWorker(() => File.AppendAllText(filePath, "Hello World"));

                client.GetInfo(folder.RepositoryUrl, out info);
                var revisionAfterChange = info.Revision;
                Assert.That(revisionAfterChange, Is.EqualTo(revisionBeforeChange + 1));
            }
        }

        [Test]
        public void EventWorker_CommittsNewFileToSvn_WhenFileIsAdded() {
            _manager.Monitor(_testFolders);
            _manager.StartProcessing();

            var folder = _testFolders[0];
            var fileName = TestFileSystemHelper.MakeUniqueFileName("css");

            using (var client = new SvnClient()) {
                SvnInfoEventArgs info;
                client.GetInfo(folder.RepositoryUrl, out info);
                var revisionBeforeAdd = info.Revision;

                DoAndWaitForWorker(() => TestFileSystemHelper.CreateFile(fileName, folder.Path));

                client.GetInfo(folder.RepositoryUrl, out info);
                var revisionAfterAdd = info.Revision;

                Assert.That(revisionAfterAdd, Is.EqualTo(revisionBeforeAdd + 1));
            }
        }

        [Test]
        public void EventWorker_DeletesFileFromSvn_WhenFileIsDeletedFromDirectory() {
            _manager.Monitor(_testFolders);
            _manager.StartProcessing();

            var folder = _testFolders[0];
            var fileName = TestFileSystemHelper.MakeUniqueFileName("css");
            DoAndWaitForWorker(() => TestFileSystemHelper.CreateFile(fileName, folder.Path));

            var filePath = folder.Path + "\\" + fileName;
            DoAndWaitForWorker(() => File.Delete(filePath));

            using (var client = new SvnClient()) {
                var status = GetSvnStatus(client, filePath);
                Assert.That(status, Is.EqualTo(SvnStatus.None));
            }
        }

        private void DoAndWaitForWorker(Action action) {
            _workerWrapper.BeforeNextWork();
            action();
            var completed = _workerWrapper.NextWorkCompleted().Wait(TimeSpan.FromMinutes(2));
            Assert.IsTrue(completed, "Worker did not complete on time.");
        }

        private SvnStatus GetSvnStatus(SvnClient client, string path) {
            var sa = new SvnStatusArgs { Depth = SvnDepth.Empty };
            sa.AddExpectedError(SvnErrorCode.SVN_ERR_WC_PATH_NOT_FOUND);
            Collection<SvnStatusEventArgs> results;

            if (!client.GetStatus(path, sa, out results)
                || results.Count != 1) {
                return SvnStatus.None;
            }

            return results[0].LocalNodeStatus;
        }

        private ILog LogFor<T>() {
            return LogManager.GetLogger(typeof(T));
        }
    }
}