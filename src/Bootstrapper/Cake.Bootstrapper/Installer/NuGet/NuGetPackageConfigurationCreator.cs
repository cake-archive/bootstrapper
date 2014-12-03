using System.IO;
using System.Xml;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer.NuGet
{
    internal sealed class NuGetPackageConfigurationCreator : INuGetPackageConfigurationCreator
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;
        private readonly ICakeLog _log;
        private readonly INugetPackageVersionProber _prober;

        public NuGetPackageConfigurationCreator(IFileSystem fileSystem, ICakeEnvironment environment, 
            ICakeLog log, INugetPackageVersionProber prober)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            _log = log;
            _prober = prober;
        }

        public bool Generate(DirectoryPath path)
        {
            var version = _prober.GetVersion("Cake");
            if (string.IsNullOrWhiteSpace(version))
            {
                _log.Error("Could not resolve latest package version.");
                return false;
            }
            return Generate(path, version);
        }

        private bool Generate(DirectoryPath path, string version)
        {
            var packagesPath = path.MakeAbsolute(_environment).CombineWithFilePath("packages.config");
            var file = _fileSystem.GetFile(packagesPath);
            if (file.Exists)
            {
                _log.Warning("Packages file already exist.");
                return false;
            }

            using (var stream = file.OpenWrite())
            {
                using (var streamWriter = new StreamWriter(stream))
                using (var writer = new XmlTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                    writer.WriteStartDocument();
                    {
                        writer.WriteStartElement("packages");
                        {
                            writer.WriteStartElement("package");
                            {
                                writer.WriteAttributeString("id", "Cake");
                                writer.WriteAttributeString("version", version);
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndDocument();
                    writer.Flush();
                }            
            }

            return true;
        }
    }
}
