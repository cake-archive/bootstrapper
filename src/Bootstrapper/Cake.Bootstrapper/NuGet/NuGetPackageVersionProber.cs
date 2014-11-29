using System.Linq;
using NuGet;

namespace Cake.Bootstrapper.NuGet
{
    internal sealed class NuGetPackageVersionProber : INugetPackageVersionProber
    {
        public string GetVersion(string packageName)
        {
            var repository = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            var package = repository.FindPackagesById(packageName).FirstOrDefault(x => x.IsLatestVersion);
            return package != null ? package.Version.ToString() : null;
        }
    }
}
