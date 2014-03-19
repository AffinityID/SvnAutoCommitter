using System.Configuration;

namespace SvnAutoCommitter.Service.Config {
    public class FolderElement : ConfigurationElement {
        [ConfigurationProperty("path", IsRequired=true, IsKey = true)]
        public string Path {
            get { return (string) this["path"]; }
            set { this["path"] = value; }
        }

        [ConfigurationProperty("relativeUrl", IsRequired=false)]
        public string RelativeUrl {
            get { return (string)this["relativeUrl"]; }
            set { this["relativeUrl"] = value; }
        }
    }
}