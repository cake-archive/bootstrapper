using System.Management.Automation;
using Autofac;
using Cake.Bootstrapper.Installer;
using Cake.Bootstrapper.Installer.IO;
using Cake.Bootstrapper.Installer.Net;
using Cake.Bootstrapper.Installer.NuGet;

namespace Cake.Bootstrapper
{
    [Cmdlet(VerbsLifecycle.Install, "Cake")]
    public sealed class InstallCmdlet : CakeCmdlet<InstallCommand>
    {
        [Parameter]
        public string Source { get; set; }

        public override void RegisterDependencies(ContainerBuilder builder)
        {
            // Register NuGet package configuration creator.
            builder.RegisterType<NuGetPackageConfigurationCreator>()
                .As<INuGetPackageConfigurationCreator>()
                .SingleInstance();

            // Register NuGet package version prober.
            builder.RegisterType<NuGetPackageVersionProber>()
                .As<INugetPackageVersionProber>()
                .SingleInstance();

            // Misc registrations.
            builder.RegisterType<ScriptCopier>().As<IScriptCopier>().SingleInstance();
            builder.RegisterType<HttpDownloader>().As<IHttpDownloader>().SingleInstance();
        }

        public override void SetCommandParameters(InstallCommand command)
        {
            if (!string.IsNullOrWhiteSpace(Source))
            {
                // Set the source.
                command.Source = Source;
            }
        }
    }
}
