using Cake.Core.IO;

namespace Cake.Bootstrapper.Extensions
{
    internal static class FileExtensions
    {
        public static bool Create(this IFile file)
        {
            if (!file.Exists)
            {
                using (var stream = file.OpenWrite())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
