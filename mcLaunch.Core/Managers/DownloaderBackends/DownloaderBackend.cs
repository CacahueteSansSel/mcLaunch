namespace mcLaunch.Core.Managers.DownloaderBackends;

public abstract class DownloaderBackend
{
    public string UserAgent { get; set; }
    
    public abstract Task<bool> Download(string sourceUrl, string filename, Action<string, float>? updateCallback);
}