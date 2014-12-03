using System;
using Cake.Bootstrapper.Installer.IO;
using Cake.Bootstrapper.Installer.Net;
using Cake.Bootstrapper.Installer.NuGet;
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
        private readonly IScriptCopier _scriptCopier;
        private readonly IHttpDownloader _downloader;

        public string Source { get; set; }

        public InstallCommand(IRuntime runtime, IFileSystem fileSystem, ICakeEnvironment environment,
            ICakeLog log, INuGetPackageConfigurationCreator packageConfigCreator,
            IScriptCopier scriptCopier, IHttpDownloader downloader)
        {
            _runtime = runtime;
            _fileSystem = fileSystem;
            _environment = environment;
            _log = log;
            _packageConfigCreator = packageConfigCreator;
            _scriptCopier = scriptCopier;
            _downloader = downloader;

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

            // Download NuGet.exe              
            ReportProgress("Downloading NuGet executable...", 25);
            var nugetFilePath = toolsPath.MakeAbsolute(_environment).CombineWithFilePath("nuget.exe");
            if (!_fileSystem.Exist(nugetFilePath))
            {
                _downloader.Download(new Uri("http://nuget.org/nuget.exe"), nugetFilePath);
                _log.Information(" -> Downloaded NuGet executable.");
            }
            else
            {
                _log.Warning("Did not download NuGet executable since it already exist.");
            }

            // Create packages.config
            ReportProgress("Generating NuGet package configuration...", 50);
            var packagesPath = toolsPath.MakeAbsolute(_environment).CombineWithFilePath("packages.config");
            if (!_fileSystem.Exist(packagesPath))
            {
                _packageConfigCreator.Generate(toolsPath);
                _log.Information(" -> Generated NuGet package configuration.");
            }
            else
            {
                _log.Warning("Did not generate NuGet package configuration since it already exist.");
            }

            // Copy bootstrapper script
            ReportProgress("Copying bootstrapper script...", 75);
            var bootstrapperPath = new FilePath("build.ps1").MakeAbsolute(_environment);
            if (!_fileSystem.Exist(bootstrapperPath))
            {
                _scriptCopier.Copy("build.ps1");
                _log.Information(" -> Copied bootstrapper script.");
            }
            else
            {
                _log.Warning("Did not copy bootstrapper script since it already exist.");
            }

            // Copy build script
            ReportProgress("Preparing build script...", 100);
            var buildScriptPath = new FilePath("build.cake").MakeAbsolute(_environment);
            if (!_fileSystem.Exist(buildScriptPath))
            {
                _scriptCopier.Copy("build.cake");
                _log.Information(" -> Copied build script.");
            }
            else
            {
                _log.Warning("Did not add build script since it already exist.");
            }
        }

        private void ReportProgress(string description, int percentage)
        {
            _runtime.ReportProgress("Cake Bootstrapper", description, percentage);
        }
    }
}
