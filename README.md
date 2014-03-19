## Overview

SvnAutoCommitter is a windows service that watches a set of folders for changes, and pushes those changes to an SVN repository.  
One of the possible uses could be versioning [uSync](http://our.umbraco.org/projects/developer-tools/usync) files in Umbraco.

## Limitations

Feel free to submit pull requests for those:
* All folders must be in a single SVN repository
* Commit message is not currently configurable
* Each change is committed separately
* Having a really large amount of changes (e.g. 500 files at once) may cause some changes to go unnoticed

## Configuration

    <configuration>
        <configSections>
           <section name="svnAutoCommitter" type="SvnAutoCommitter.Service.Config.SvnAutoCommitterConfigurationSection, SvnAutoCommitter.Service" />
        </configSections>
        <appSettings>
          <add key="ServiceName" value="" />        <!-- Windows Service name (used during service installation only) -->
          <add key="ServiceDisplayName" value="" /> <!-- Windows Service display name (used during service installation only) -->
        </appSettings>
     
        <svnAutoCommitter basePath="" repositoryUrl="" repositoryUsername="" repositoryPassword="">
          <!-- 
              basePath            (optional) a path that will be used as a prefix to all folder paths
              repositoryUrl       (required) an URL of the Subversion repository
              repositoryUsername  (required) a user name that is used to commit the changes
              repositoryPassword  (required) a password that is used to commit the changes
          -->
          <folders> <!-- List of folders to watch -->
            <add path="" relativeUrl="" /> 
            <!-- 
                path        (required) a path to the folder (relative to the basePath or absolute if basePath is not set)
                relativeUrl (optional) associated URL in Subversion, relative to repositoryUrl (uses path if not specified) 
            -->
          </folders>
        </svnAutoCommitter> 
    </configuration>
