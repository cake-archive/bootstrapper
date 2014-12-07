using System;
using System.Net;
using Cake.Core.IO;

namespace Cake.Bootstrapper.Net
{
    internal sealed class HttpDownloader : IHttpDownloader
    {
        public void Download(Uri uri, FilePath path)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(uri, path.FullPath);
            }
        }
    }
}
