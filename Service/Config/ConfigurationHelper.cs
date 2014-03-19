using System.Configuration;
using System.Reflection;

namespace SvnAutoCommitter.Service.Config {
    public static class ConfigurationHelper {
        public static string GetAppConfigValue(string serviceName) {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetAssembly(typeof(SvnAutoCommitterServiceInstaller)).Location);
            return config.AppSettings.Settings[serviceName].Value;
        }
    }
}