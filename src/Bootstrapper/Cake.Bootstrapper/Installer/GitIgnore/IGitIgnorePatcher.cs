using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer.GitIgnore
{
    public interface IGitIgnorePatcher
    {
        bool Patch(FilePath path);
    }
}