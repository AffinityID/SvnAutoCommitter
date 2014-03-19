using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SvnAutoCommitter.Core.Models;
using SvnAutoCommitter.Service.Config;

namespace SvnAutoCommitter.Service {
    public class SvnAutoCommitterConfiguration {
        public string RepositoryUsername { get; set; }
        public string RepositoryPassword { get; set; }
        public ICollection<Folder> Folders { get; private set; }

        public SvnAutoCommitterConfiguration(SvnAutoCommitterConfigurationSection section) {
            RepositoryUsername = section.RepositoryUsername;
            RepositoryPassword = section.RepositoryPassword;
            Folders = (
                from FolderElement folderElement in section.Folders
                select LoadFolder(folderElement, section.BasePath, section.RepositoryUrl)
            ).ToList();
        }

        private Folder LoadFolder(FolderElement folderElement, string basePath, string repositoryUrl) {
            var path = string.IsNullOrEmpty(basePath) 
                     ? folderElement.Path 
                     : Path.Combine(basePath, folderElement.Path.TrimStart(Path.DirectorySeparatorChar));

            var relativeUrl = string.IsNullOrEmpty(folderElement.RelativeUrl) 
                            ? folderElement.Path.Replace("\\", "/")
                            : folderElement.RelativeUrl;

            var folder = new Folder {
                Path = path,
                RepositoryUrl = new Uri(new Uri(repositoryUrl), relativeUrl)
            };
            return folder;
        }
    }
}