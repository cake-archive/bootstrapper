using System;
using System.Management.Automation;
using Cake.Core.Diagnostics;

namespace Cake.Bootstrapper.Diagnostics
{
    internal sealed class PowerShellLog : ICakeLog
    {
        private readonly PSCmdlet _cmdlet;

        public PowerShellLog(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    _cmdlet.WriteWarning(string.Format(format, args));
                    return;
                case LogLevel.Warning:
                    _cmdlet.WriteWarning(string.Format(format, args));
                    return;
                case LogLevel.Information:
                    _cmdlet.WriteObject(string.Format(format, args));
                    return;
                case LogLevel.Verbose:
                    _cmdlet.WriteVerbose(string.Format(format, args));
                    return;
                case LogLevel.Debug:
                    _cmdlet.WriteDebug(string.Format(format, args));
                    return;
                default:
                    throw new InvalidOperationException("Invalid log level.");
            }
        }
    }
}
