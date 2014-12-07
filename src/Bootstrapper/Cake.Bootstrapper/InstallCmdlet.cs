using System.Management.Automation;
using Autofac;
using Cake.Bootstrapper.Installer;
using Cake.Bootstrapper.Installer.GitIgnore;
using Cake.Bootstrapper.Installer.NuGet;
using Cake.Bootstrapper.Installer.Scripts;
using Cake.Bootstrapper.Net;

namespace Cake.Bootstrapper
{
    [Cmdlet(VerbsLifecycle.Install, "Cake")]
    public sealed class InstallCmdlet : CakeCmdlet<InstallCommand>
    {
        [Parameter]
        public string Source { get; set; }

        public override void RegisterDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<NuGetPackageConfigurationCreator>()
                .As<INuGetPackageConfigurationCreator>()
                .SingleInstance();
            
            builder.RegisterType<NuGetPackageVersionProber>()
                .As<INugetPackageVersionProber>()
                .SingleInstance();

            builder.RegisterType<ScriptCopier>().As<IScriptCopier>().SingleInstance();

            builder.RegisterType<GitIgnorePatcher>().As<IGitIgnorePatcher>().SingleInstance();
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
