using System.Management.Automation;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Runtime
{
    internal sealed class PowerShellSessionState : ISessionState
    {
        private readonly PSCmdlet _cmdlet;

        public PowerShellSessionState(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public DirectoryPath FileSystemLocation
        {
            get { return _cmdlet.SessionState.Path.CurrentFileSystemLocation.Path; }
        }
    }
}
