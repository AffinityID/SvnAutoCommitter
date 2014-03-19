using System;
using System.Net;
using log4net;
using SharpSvn;

namespace SvnAutoCommitter.Core {
    public class SharpSvnClient : ISvnClient {
        private readonly ILog _logger;
        private readonly ICredentials _svnCredentials;

        public SharpSvnClient(ICredentials svnCredentials, ILog logger) {
            _svnCredentials = svnCredentials;
            _logger = logger;
        }

        public bool IsWorkingCopy(string path) {
            using (var svnClient = new SvnClient()) {
                svnClient.Authentication.DefaultCredentials = _svnCredentials;
                var uri = svnClient.GetUriFromWorkingCopy(path);
                return uri != null;
            }
        }

        public bool SvnCheckOut(Uri uri, string path) {
            return DoWithSvn(path, svnClient => {
                svnClient.Authentication.DefaultCredentials = _svnCredentials;
                var url = new SvnUriTarget(uri);
                var args = new SvnCheckOutArgs { Depth = SvnDepth.Infinity, IgnoreExternals = true };
                var result = svnClient.CheckOut(url, path, args);
                if (result)
                    _logger.InfoFormat("SharpSvn has checked out {0}", path);

                return result;
            });
        }

        public bool SvnAdd(string path) {
            return DoWithSvn(path, svnClient => {
                svnClient.Authentication.DefaultCredentials = _svnCredentials;
                var args = new SvnAddArgs { Depth = SvnDepth.Infinity, AddParents = true };
                var result = svnClient.Add(path, args);
                if (result)
                    _logger.InfoFormat("SharpSvn has added {0}", path);

                return result;
            });
        }

        public bool SvnCommit(string path) {
            return DoWithSvn(path, svnClient => {
                svnClient.Authentication.DefaultCredentials = _svnCredentials;
                var args = new SvnCommitArgs {
                    LogMessage = string.Format("Commited by SvnAutoCommitter service via SharpSvn. {0}", path),
                    Depth = SvnDepth.Infinity
                };

                SvnCommitResult svnCommitResult = null;
                var result = svnClient.Commit(path, args, out svnCommitResult);
                if (result) {
                    _logger.InfoFormat(
                        svnCommitResult != null
                            ? "SharpSvn has committed {0}"
                            : "SharpSvn tried to commit {0}, but no modification was detected", path
                    );
                }

                return result;
            });
        }

        public bool SvnDelete(string path) {
            return DoWithSvn(path, svnClient => {
                svnClient.Authentication.DefaultCredentials = _svnCredentials;
                var args = new SvnDeleteArgs {
                    Force = true,
                    KeepLocal = false,
                    LogMessage = string.Format("Deleted by SvnAutoCommitter service via SharpSvn. {0}", path)
                };
                var result = svnClient.Delete(path, args);
                if (result)
                    _logger.InfoFormat("SharpSvn has deleted {0}", path);

                return result;
            });
        }

        private bool DoWithSvn(string path, Func<SvnClient, bool> action) {
            using (var svnClient = new SvnClient()) {
                try {
                    return action(svnClient);
                }
                catch (SvnException se) {
                    _logger.ErrorFormat("Error: {0}, SharpSvn error on: {1}, {2}. {3}.",
                        se.SvnErrorCode, path, se.RootCause.Message, se);
                }
            }
            return false;
        }
    }
}