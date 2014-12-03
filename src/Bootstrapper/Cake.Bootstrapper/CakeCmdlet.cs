using System.Management.Automation;
using Autofac;
using Cake.Bootstrapper.Diagnostics;
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
                // Get the session.
                var session = container.Resolve<ISessionState>();

                // Get the environment and set the working directory.
                var environment = container.Resolve<ICakeEnvironment>();
                environment.WorkingDirectory = session.FileSystemLocation;

                // Resolve the command.
                var command = container.Resolve<TCommand>();

                // Set parameters.
                SetCommandParameters(command);

                // Execute the command.
                command.Execute();
            }
        }

        public abstract void SetCommandParameters(TCommand command);

        public virtual void RegisterDependencies(ContainerBuilder builder)
        {            
        }

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TCommand>().SingleInstance();
            builder.RegisterInstance(this).As<Cmdlet>().As<PSCmdlet>().SingleInstance();
            builder.RegisterType<PowerShellRuntime>().As<IRuntime>().SingleInstance();
            builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<CakeEnvironment>().As<ICakeEnvironment>().SingleInstance();            
            builder.RegisterType<PowerShellLog>().As<ICakeLog>().SingleInstance();
            builder.RegisterType<PowerShellSessionState>().As<ISessionState>();

            RegisterDependencies(builder);

            return builder.Build();
        }
    }
}
