using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using SvnAutoCommitter.Service.Config;

namespace SvnAutoCommitter.Service {
    [RunInstaller(true)]
    public partial class SvnAutoCommitterServiceInstaller : Installer {
        public SvnAutoCommitterServiceInstaller() {
            //InitializeComponent();
            var serviceProcessInstaller =
                new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.NetworkService;

            //# Service Information
            serviceInstaller.ServiceName = ConfigurationHelper.GetAppConfigValue("ServiceName");
            serviceInstaller.DisplayName = ConfigurationHelper.GetAppConfigValue("ServiceDisplayName");
            serviceInstaller.Description =
                "This service monitors the specified directories for changes and commit changes to SVN";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }

        private void serviceProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}