using System.Net;

namespace mcLaunch.Core.Managers.DownloaderBackends;

[Obsolete]
public class ExternalDownloaderBackend : DownloaderBackend
{
    public override async Task<bool> Download(string sourceUrl, string filename, Action<string, float>? updateCallback)
    {
        string? dirName = Path.GetDirectoryName(filename);
        if (string.IsNullOrWhiteSpace(dirName)) return false;
        
        //IDownload? download = DownloadBuilder.New()
        //    .WithConfiguration(new DownloadConfiguration
        //    {
        //        RequestConfiguration = new RequestConfiguration
        //        {
        //            UserAgent = UserAgent,
        //            Accept = "*/*",
        //            AllowAutoRedirect = false,
        //            AutomaticDecompression = DecompressionMethods.All,
        //            PreAuthenticate = false
        //        }
        //    })
        //    .WithUrl(sourceUrl)
        //    .WithFolder(new DirectoryInfo(dirName))
        //    .WithFileName(Path.GetFileName(filename))
        //    .Build();
//
        //download.DownloadProgressChanged += (sender, args) =>
        //{
        //    updateCallback?.Invoke(sourceUrl, (float)args.ProgressPercentage / 100f);
        //};
//
        //Stream? stream = await download.StartAsync();
//
        //return download.Package.Status != DownloadStatus.Failed;

        return false;
    }
}