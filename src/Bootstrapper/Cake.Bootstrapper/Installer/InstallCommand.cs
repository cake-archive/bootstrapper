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

        public bool AppVeyor { get; set; }
        public bool GitIgnore { get; set; }
        public bool Empty { get; set; }
        public bool InstallNuGet { get; set; }

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

            // Create packages.config.
            ReportProgress("Generating NuGet package configuration...", 25);
            var packagesPath = toolsPath.MakeAbsolute(_environment).CombineWithFilePath("packages.config");
            if (!_fileSystem.Exist(packagesPath))
            {
                _packageConfigCreator.Generate(toolsPath);
                _log.Information(" -> Generated NuGet package configuration.");
            }

            // Copy bootstrapper script.
            ReportProgress("Copying bootstrapper script...", 50);
            var bootstrapperPath = new FilePath("build.ps1").MakeAbsolute(_environment);
            if (!_fileSystem.Exist(bootstrapperPath))
            {
                _fileCopier.CopyBootstrapperScript();
                _log.Information(" -> Copied bootstrapper script.");
            }

            // Copy build script.
            ReportProgress("Preparing build script...", 75);
            var buildScriptPath = new FilePath("build.cake").MakeAbsolute(_environment);
            if (!_fileSystem.Exist(buildScriptPath))
            {
                if (Empty)
                {
                    _fileCopier.CopyEmptyCakeScript();
                    _log.Information(" -> Copied empty build script.");
                }
                else
                {
                    _fileCopier.CopyConventionBasedCakeScript();
                    _log.Information(" -> Copied build script.");
                }
            }

            // Download NuGet.exe.
            if (InstallNuGet)
            {
                ReportProgress("Downloading NuGet executable...", 85);
                var nugetFilePath = toolsPath.MakeAbsolute(_environment).CombineWithFilePath("nuget.exe");
                if (!_fileSystem.Exist(nugetFilePath))
                {
                    _downloader.Download(new Uri("http://nuget.org/nuget.exe"), nugetFilePath);
                    _log.Information(" -> Downloaded NuGet executable.");
                }
            }

            // Copy appveyor file.
            if (AppVeyor)
            {
                ReportProgress("Copying AppVeyor configuration file...", 95);
                var appVeyorPath = new FilePath("appveyor.yml").MakeAbsolute(_environment);
                if (!_fileSystem.Exist(appVeyorPath))
                {
                    _fileCopier.CopyAppVeyorConfiguration();
                    _log.Information(" -> Copied AppVeyor configuration file.");
                }
            }

            // Patch .gitignore file.
            if (GitIgnore)
            {
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

            ReportProgress("Done!", 100);
        }

        private void ReportProgress(string description, int percentage)
        {
            _runtime.ReportProgress("Cake Bootstrapper", description, percentage);
        }
    }
}
