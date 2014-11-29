using System.Management.Automation;

namespace Cake.Bootstrapper.Runtime
{
    internal sealed class PowerShellRuntime : IRuntime
    {
        private readonly Cmdlet _cmdlet;

        public PowerShellRuntime(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public void ReportProgress(string title, string description, int percentage)
        {
            _cmdlet.WriteProgress(new ProgressRecord(0, title, description)
            {
                PercentComplete = percentage
            });
        }
    }
}
