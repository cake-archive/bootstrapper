using System.Management.Automation;
using Autofac;
using Cake.Bootstrapper.Commands;
using Cake.Bootstrapper.Diagnostics;
using Cake.Bootstrapper.NuGet;
using Cake.Bootstrapper.Runtime;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Bootstrapper
{
    public abstract class CakeCmdlet<TCommand> : PSCmdlet
        where TCommand : ICommand
    {
        protected override void ProcessRecord()
        {
            using (var container = BuildContainer())
            {
                // Get the environment and set the working directory.
                var environment = container.Resolve<ICakeEnvironment>();
                environment.WorkingDirectory = SessionState.Path.CurrentFileSystemLocation.Path;

                // Resolve the command.
                var command = container.Resolve<TCommand>();

                // Set parameters.
                SetCommandParameters(command);

                // Execute the command.
                command.Execute();
            }
        }

        public abstract void SetCommandParameters(TCommand command);

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            // Register the command.
            builder.RegisterType<TCommand>().SingleInstance();

            // Runtime specific registrations.
            builder.RegisterInstance(this).As<Cmdlet>().As<PSCmdlet>().SingleInstance();
            builder.RegisterType<PowerShellRuntime>().As<IRuntime>().SingleInstance();

            // File system and environment specific registrations.
            builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<CakeEnvironment>().As<ICakeEnvironment>().SingleInstance();

            // Diagnostic specific registrations.
            builder.RegisterType<PowerShellLog>().As<ICakeLog>().SingleInstance();
            
            // NuGet specific registrations.
            builder.RegisterType<NuGetPackageVersionProber>()
                .As<INugetPackageVersionProber>()
                .SingleInstance();
            builder.RegisterType<NuGetPackageConfigurationCreator>()
                .As<INuGetPackageConfigurationCreator>()
                .SingleInstance();

            return builder.Build();
        }
    }
}
