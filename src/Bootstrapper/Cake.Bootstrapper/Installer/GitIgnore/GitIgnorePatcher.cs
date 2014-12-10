using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer.GitIgnore
{
    internal sealed class GitIgnorePatcher : IGitIgnorePatcher
    {
        private readonly IFileSystem _fileSystem;

        private readonly string[] _content = { "[Tt]ools/[Cc]ake/" };

        public GitIgnorePatcher(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool Patch(FilePath path)
        {
            var file = _fileSystem.GetFile(path);
            if (!file.Exists)
            {
                return false;
            }

            // Read all lines and remove the Cake related stuff.            
            bool needsPatching;
            var lines = ReadLines(file, out needsPatching);
            if (!needsPatching)
            {
                return false;
            }

            // Add the content we want to add the end of the file.
            lines.Add(string.Empty);
            lines.Add("#Cake (generated)");
            foreach (var line in _content)
            {
                lines.Add(line);
            }

            // Save the content back to the file.
            using (var stream = file.OpenWrite())
            using (var writer = new StreamWriter(stream))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }

            return true;
        }

        private List<string> ReadLines(IFile file, out bool needsPatching)
        {
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var stream = file.OpenRead())
            using (var reader = new StreamReader(stream, true))
            {
                var readLines = new List<string>();
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (!_content.Contains(line, StringComparer.Ordinal))
                    {
                        readLines.Add(line);
                    }
                    else
                    {
                        existing.Add(line);
                    }
                }

                needsPatching = existing.Count != _content.Length;
                return RemoveTrailingEmptyLines(readLines);
            }
        }

        private static List<string> RemoveTrailingEmptyLines(IReadOnlyList<string> lines)
        {
            var end = lines.Count - 1;
            while (end >= 0 && string.IsNullOrWhiteSpace(lines[end]))
            {
                end--;
            }
            return lines.Take(end + 1).ToList();            
        }
    }
}
