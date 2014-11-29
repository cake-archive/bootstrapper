namespace Cake.Bootstrapper.NuGet
{
    public interface INugetPackageVersionProber
    {
        string GetVersion(string packageName);
    }
}
