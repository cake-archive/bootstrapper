using System.Management.Automation;
using Cake.Bootstrapper.Commands;

namespace Cake.Bootstrapper
{
    [Cmdlet(VerbsLifecycle.Install, "Cake")]
    public sealed class InstallCmdlet : CakeCmdlet<InstallCommand>
    {
        [Parameter]
        public string Source { get; set; }

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
