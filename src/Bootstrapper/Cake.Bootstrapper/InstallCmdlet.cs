using System.Management.Automation;
using Autofac;
using Cake.Bootstrapper.Installer;
using Cake.Bootstrapper.Installer.GitIgnore;
using Cake.Bootstrapper.Installer.NuGet;
using Cake.Bootstrapper.Installer.Resources;

namespace Cake.Bootstrapper
{
    [Cmdlet(VerbsLifecycle.Install, "Cake")]
    public sealed class InstallCmdlet : CakeCmdlet<InstallCommand>
    {
        [Parameter]
        public string Source { get; set; }
        [Parameter]
        public SwitchParameter AppVeyor { get; set; }
        [Parameter]
        public SwitchParameter GitIgnore { get; set; }
        [Parameter]
        public SwitchParameter Empty { get; set; }

        public override void RegisterDependencies(ContainerBuilder builder)
        {
            // Register NuGet package configuration creator.
            builder.RegisterType<NuGetPackageConfigurationCreator>()
                .As<INuGetPackageConfigurationCreator>()
                .SingleInstance();
            
            // Register NuGet version prober.
            builder.RegisterType<NuGetPackageVersionProber>()
                .As<INugetPackageVersionProber>()
                .SingleInstance();

            // Register file copier.
            builder.RegisterType<FileCopier>().As<IFileCopier>().SingleInstance();

            // Register gitignore patcher.
            builder.RegisterType<GitIgnorePatcher>().As<IGitIgnorePatcher>().SingleInstance();
        }

        public override void SetCommandParameters(InstallCommand command)
        {
            // Source
            if (!string.IsNullOrWhiteSpace(Source))
            {
                command.Source = Source;
            }
            // AppVeyor
            if (AppVeyor.IsPresent)
            {
                command.AppVeyor = true;
            }
            // GitIgnore
            if (GitIgnore.IsPresent)
            {
                command.GitIgnore = true;
            }
            // Empty script
            if (Empty.IsPresent)
            {
                command.Empty = true;
            }
        }
    }
}
