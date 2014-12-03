namespace Cake.Bootstrapper.Installer.NuGet
{
    public interface INugetPackageVersionProber
    {
        string GetVersion(string packageName);
    }
}
