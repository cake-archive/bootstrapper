using System.IO;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer.Resources
{
    internal sealed class FileCopier : IFileCopier
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;
        private readonly ICakeLog _log;

        public FileCopier(IFileSystem fileSystem, ICakeEnvironment environment, ICakeLog log)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            _log = log;
        }

        public bool Copy(string scriptName)
        {
            var applicationRoot = _environment.GetApplicationRoot();

            // Get the source file.
            var sourcePath = applicationRoot.CombineWithFilePath(scriptName);
            var sourceFile = _fileSystem.GetFile(sourcePath);

            // Get the destination file.
            var destinationPath = new FilePath(scriptName).MakeAbsolute(_environment);
            var destinationFile = _fileSystem.GetFile(destinationPath);

            // Copy the script.
            return Copy(sourceFile, destinationFile);            
        }

        private bool Copy(IFile source, IFile destination)
        {
            if (!source.Exists)
            {
                _log.Error("Could not find file {0}.", source.Path.FullPath);
                return false;
            }
            if (destination.Exists)
            {
                _log.Error("The file {0} already exist.", destination.Path.FullPath);
                return false;
            }

            using (var input = source.OpenRead())
            using (var output = destination.OpenWrite())
            {
                using (var reader = new StreamReader(input))
                using (var writer = new StreamWriter(output))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        writer.WriteLine(line);
                    }
                }
            }

            return true;
        }
    }
}
