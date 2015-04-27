namespace Cake.Bootstrapper.Installer.Resources
{
    public interface IFileCopier
    {
        bool CopyConventionBasedCakeScript();
        bool CopyEmptyCakeScript();
        bool CopyAppVeyorConfiguration();
        bool CopyBootstrapperScript();
    }
}