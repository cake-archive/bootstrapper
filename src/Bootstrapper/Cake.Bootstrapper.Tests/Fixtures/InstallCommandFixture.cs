using Cake.Bootstrapper.Installer;
using Cake.Bootstrapper.Installer.IO;
using Cake.Bootstrapper.Installer.Net;
using Cake.Bootstrapper.Installer.NuGet;
using Cake.Bootstrapper.Runtime;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;

namespace Cake.Bootstrapper.Tests.Fixtures
{
    public sealed class InstallCommandFixture
    {
        public IRuntime Runtime { get; set; }
        public IFileSystem FileSystem { get; set; }
        public ICakeEnvironment Environment { get; set; }
        public ICakeLog Log { get; set; }
        public INuGetPackageConfigurationCreator PackageConfigurationCreator { get; set; }
        public IScriptCopier ScriptCopier { get; set; }
        public IHttpDownloader Downloader { get; set; }

        public IDirectory ToolsDirectory { get; set; }
        public IFile NuGetExecutable { get; set; }
        public IFile PackagesConfiguration { get; set; }
        public IFile BootstrapperScript { get; set; }
        public IFile BuildScript { get; set; }

        public InstallCommandFixture()
        {
            // Initialize services.
            Runtime = Substitute.For<IRuntime>();
            FileSystem = Substitute.For<IFileSystem>();
            Log = Substitute.For<ICakeLog>();
            PackageConfigurationCreator = Substitute.For<INuGetPackageConfigurationCreator>();
            ScriptCopier = Substitute.For<IScriptCopier>();
            Downloader = Substitute.For<IHttpDownloader>();

            // Initialize environment.
            Environment = Substitute.For<ICakeEnvironment>();
            Environment.WorkingDirectory.Returns(x => "/Working");

            // Initialize the file system.
            InitializeFileSystem();
        }

        private void InitializeFileSystem()
        {
            ToolsDirectory = Substitute.For<IDirectory>();
            ToolsDirectory.Exists.Returns(false);
            FileSystem.GetDirectory(Arg.Is<DirectoryPath>(x => x.FullPath == "/Working/tools")).Returns(ToolsDirectory);

            NuGetExecutable = Substitute.For<IFile>();
            NuGetExecutable.Exists.Returns(false);
            FileSystem.GetFile(Arg.Is<FilePath>(p => p.FullPath == "/Working/tools/nuget.exe")).Returns(NuGetExecutable);

            PackagesConfiguration = Substitute.For<IFile>();
            PackagesConfiguration.Exists.Returns(false);
            FileSystem.GetFile(Arg.Is<FilePath>(p => p.FullPath == "/Working/tools/packages.config")).Returns(PackagesConfiguration);

            BootstrapperScript = Substitute.For<IFile>();
            BootstrapperScript.Exists.Returns(false);
            FileSystem.GetFile(Arg.Is<FilePath>(p => p.FullPath == "/Working/build.ps1")).Returns(BootstrapperScript);

            BuildScript = Substitute.For<IFile>();
            BuildScript.Exists.Returns(false);
            FileSystem.GetFile(Arg.Is<FilePath>(p => p.FullPath == "/Working/build.cake")).Returns(BuildScript);
        }

        public InstallCommand CreateCommand()
        {
            return new InstallCommand(Runtime, FileSystem, Environment,
                Log, PackageConfigurationCreator, ScriptCopier, Downloader);
        }
    }
}
