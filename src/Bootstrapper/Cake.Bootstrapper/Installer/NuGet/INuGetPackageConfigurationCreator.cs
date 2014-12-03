using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer.NuGet
{
    public interface INuGetPackageConfigurationCreator
    {
        bool Generate(DirectoryPath path);
    }
}
