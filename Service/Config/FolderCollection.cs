using System.Configuration;

namespace SvnAutoCommitter.Service.Config {
    public class FolderCollection : ConfigurationElementCollection {
        protected override ConfigurationElement CreateNewElement() {
            return new FolderElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((FolderElement) element).Path;
        }
    }
}