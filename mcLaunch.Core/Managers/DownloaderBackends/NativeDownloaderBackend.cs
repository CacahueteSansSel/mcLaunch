using System.Net.Http.Headers;

namespace mcLaunch.Core.Managers.DownloaderBackends;

public class NativeDownloaderBackend : DownloaderBackend
{
    public override async Task<bool> Download(string sourceUrl, string filename, Action<string, float>? updateCallback)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

        await using Stream sourceStream = await client.GetStreamAsync(sourceUrl);
        await using FileStream targetStream = new FileStream(filename, FileMode.Create);
        int readBytes = 0;

        //todo
        
        return false;
    }
}