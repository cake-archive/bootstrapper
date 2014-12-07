using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer.GitIgnore
{
    public interface IGitIgnorePatcher
    {
        void Patch(FilePath path);
    }
}