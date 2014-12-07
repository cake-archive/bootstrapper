using System;
using Cake.Bootstrapper.Installer.GitIgnore;
using Cake.Bootstrapper.Installer.NuGet;
using Cake.Bootstrapper.Installer.Resources;
using Cake.Bootstrapper.Net;
using Cake.Bootstrapper.Runtime;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer
{
    public sealed class InstallCommand : ICommand
    {
        private readonly IRuntime _runtime;
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;
        private readonly ICakeLog _log;
        private readonly INuGetPackageConfigurationCreator _packageConfigCreator;
        private readonly IFileCopier _fileCopier;
        private readonly IHttpDownloader _downloader;
        private readonly IGitIgnorePatcher _gitIgnorePatcher;

        public string Source { get; set; }
        public bool AppVeyor { get; set; }

        public InstallCommand(IRuntime runtime, IFileSystem fileSystem, ICakeEnvironment environment,
            ICakeLog log, INuGetPackageConfigurationCreator packageConfigCreator,
            IFileCopier fileCopier, IHttpDownloader downloader, IGitIgnorePatcher gitIgnorePatcher)
        {
            _runtime = runtime;
            _fileSystem = fileSystem;
            _environment = environment;
            _log = log;
            _packageConfigCreator = packageConfigCreator;
            _fileCopier = fileCopier;
            _downloader = downloader;
            _gitIgnorePatcher = gitIgnorePatcher;

            // Set default parameter values.
            Source = "https://raw.githubusercontent.com/cake-build/bootstrapper/master/res/";
        }

        public void Execute()
        {
            // Create tools directory.            
            ReportProgress("Creating tools directory...", 0);
            var toolsPath = new DirectoryPath("./tools").MakeAbsolute(_environment);
            var toolsDirectory = _fileSystem.GetDirectory(toolsPath);
            if (!toolsDirectory.Exists)
            {
                toolsDirectory.Create();
            }

            // Download NuGet.exe.
            ReportProgress("Downloading NuGet executable...", 25);
            var nugetFilePath = toolsPath.MakeAbsolute(_environment).CombineWithFilePath("nuget.exe");
            if (!_fileSystem.Exist(nugetFilePath))
            {
                _downloader.Download(new Uri("http://nuget.org/nuget.exe"), nugetFilePath);
                _log.Information(" -> Downloaded NuGet executable.");
            }

            // Create packages.config.
            ReportProgress("Generating NuGet package configuration...", 50);
            var packagesPath = toolsPath.MakeAbsolute(_environment).CombineWithFilePath("packages.config");
            if (!_fileSystem.Exist(packagesPath))
            {
                _packageConfigCreator.Generate(toolsPath);
                _log.Information(" -> Generated NuGet package configuration.");
            }

            // Copy bootstrapper script.
            ReportProgress("Copying bootstrapper script...", 75);
            var bootstrapperPath = new FilePath("build.ps1").MakeAbsolute(_environment);
            if (!_fileSystem.Exist(bootstrapperPath))
            {
                _fileCopier.Copy("build.ps1");
                _log.Information(" -> Copied bootstrapper script.");
            }

            // Copy build script.
            ReportProgress("Preparing build script...", 85);
            var buildScriptPath = new FilePath("build.cake").MakeAbsolute(_environment);
            if (!_fileSystem.Exist(buildScriptPath))
            {
                _fileCopier.Copy("build.cake");
                _log.Information(" -> Copied build script.");
            }

            // Copy appveyor file.
            if (AppVeyor)
            {
                ReportProgress("Copying AppVeyor configuration file...", 95);
                var appVeyorPath = new FilePath("appveyor.yml").MakeAbsolute(_environment);
                if (!_fileSystem.Exist(appVeyorPath))
                {
                    _fileCopier.Copy("appveyor.yml");
                    _log.Information(" -> Copied AppVeyor configuration file.");
                }
            }

            // Patch .gitignore file.
            ReportProgress("Patching .gitignore...", 100);
            var gitIgnorePath = new FilePath(".gitignore").MakeAbsolute(_environment);
            if (_fileSystem.Exist(gitIgnorePath))
            {
                if (_gitIgnorePatcher.Patch(gitIgnorePath))
                {
                    _log.Information(" -> Patched .gitignore.");   
                }                
            }            
        }

        private void ReportProgress(string description, int percentage)
        {
            _runtime.ReportProgress("Cake Bootstrapper", description, percentage);
        }
    }
}
