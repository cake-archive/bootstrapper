using Cake.Core.IO;

namespace Cake.Bootstrapper.NuGet
{
    public interface INuGetPackageConfigurationCreator
    {
        bool Generate(DirectoryPath path);
    }
}
