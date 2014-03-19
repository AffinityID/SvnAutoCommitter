using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using SvnAutoCommitter.Core;
using SvnAutoCommitter.Service.Config;

namespace SvnAutoCommitter.Service {
    public partial class SvnAutoCommitterService : ServiceBase {
        private readonly ILog _logger;
        private FileMonitorManager _fileMonitorManager;

        public SvnAutoCommitterService() {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            InitializeComponent();
        }

        public void Start() {
            _logger.Info("SvnAutoCommitService is starting.");

            try {
                var configurationSection = (SvnAutoCommitterConfigurationSection) ConfigurationManager.GetSection("svnAutoCommitter");
                var configuration = new SvnAutoCommitterConfiguration(configurationSection);
                var svnCredential = new NetworkCredential(configuration.RepositoryUsername, configuration.RepositoryPassword);

                var poller = new FileSystemEventPoller(new FileSystemEventWorker(new SharpSvnClient(svnCredential, _logger), _logger), _logger);

                _fileMonitorManager = new FileMonitorManager(poller, _logger);

                _fileMonitorManager.Monitor(configuration.Folders);
                _fileMonitorManager.StartProcessing();
            }
            catch (Exception ex) {
                _logger.FatalFormat("SvnAutoCommitService failed to start: {0}.", ex);
                throw;
            }
        }

        protected override void OnStart(string[] args) {
            Start();
        }

        protected override void OnStop() {
            _fileMonitorManager.Dispose();
            _logger.Warn("SvnAutoCommitService has stopped");
        }
    }
}