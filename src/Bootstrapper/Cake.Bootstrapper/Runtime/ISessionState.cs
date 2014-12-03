using Cake.Core.IO;

namespace Cake.Bootstrapper.Runtime
{
    public interface ISessionState
    {
        DirectoryPath FileSystemLocation { get; }
    }
}
