using System;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Installer.Net
{
    public interface IHttpDownloader
    {
        void Download(Uri uri, FilePath path);
    }
}