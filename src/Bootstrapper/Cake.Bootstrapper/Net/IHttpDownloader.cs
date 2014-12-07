using System;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Net
{
    public interface IHttpDownloader
    {
        void Download(Uri uri, FilePath path);
    }
}