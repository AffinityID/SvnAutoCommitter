using System.Configuration;

namespace SvnAutoCommitter.Service.Config {
    public class SvnAutoCommitterConfigurationSection : ConfigurationSection {
        [ConfigurationProperty("basePath", IsRequired = false)]
        public string BasePath {
            get { return (string)this["basePath"]; }
            set { this["path"] = value; }
        }

        [ConfigurationProperty("repositoryUrl", IsRequired = false)]
        public string RepositoryUrl {
            get { return (string)this["repositoryUrl"]; }
            set { this["repositoryURL"] = value; }
        }

        [ConfigurationProperty("repositoryUsername", IsRequired = true)]
        public string RepositoryUsername {
            get { return (string)this["repositoryUsername"]; }
            set { this["repositoryUsername"] = value; }
        }

        [ConfigurationProperty("repositoryPassword", IsRequired = true)]
        public string RepositoryPassword {
            get { return (string)this["repositoryPassword"]; }
            set { this["repositoryPassword"] = value; }
        }

        [ConfigurationProperty("folders", IsRequired = true)]
        public FolderCollection Folders {
            get { return (FolderCollection)this["folders"]; }
            set { this["folders"] = value; }
        }
    }
}