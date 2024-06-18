using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Threading.Tasks;
using Downloader;

namespace mcLaunch.Installer.Core;

public static class DownloadManager
{
    public static event Action<string, float> OnDownloadProgressUpdate;

    public static async Task<MemoryStream> DownloadToMemoryAsync(string url, long? expectedSize = null)
    {
        DownloadService download = new DownloadService(new DownloadConfiguration()
        {
            RequestConfiguration = new RequestConfiguration()
            {
                UserAgent = "mcLaunch.Installer/1.1.0",
                Accept = "*/*",
                AllowAutoRedirect = false,
                AuthenticationLevel = AuthenticationLevel.None,
                AutomaticDecompression = DecompressionMethods.All,
                PreAuthenticate = false
            }
        });

        download.DownloadProgressChanged += (sender, args) =>
        {
            OnDownloadProgressUpdate?.Invoke(Path.GetFileName(url), (float) (args.ProgressPercentage / 100f));
        };

        Stream stream = await download.DownloadFileTaskAsync(url);

        return (MemoryStream)stream;
    }
}