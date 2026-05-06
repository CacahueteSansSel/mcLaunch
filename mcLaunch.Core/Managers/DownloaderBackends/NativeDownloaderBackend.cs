using System.Net.Http.Headers;
using mcLaunch.Core.Logging;

namespace mcLaunch.Core.Managers.DownloaderBackends;

public class NativeDownloaderBackend : DownloaderBackend
{
    public override async Task<bool> Download(string sourceUrl, string filename, Action<string, float>? updateCallback)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

        HttpResponseMessage resp = await client.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!resp.IsSuccessStatusCode)
        {
            Logs.Warning($"when downloading {sourceUrl} : server responded with non-200 HTTP error {(int)resp.StatusCode}");
            
            return false;
        }
        
        await using Stream sourceStream = await resp.Content.ReadAsStreamAsync();
        await using FileStream targetStream = new FileStream(filename, FileMode.Create);
        long length = 0;
        int offset = 0;
        byte[] buffer = new byte[81920];

        try
        {
            length = sourceStream.Length;
        }
        catch (Exception e)
        {
            
        }

        while (true)
        {
            int bytesRead = await sourceStream.ReadAsync(buffer.AsMemory());
            if (bytesRead <= 0) break;

            offset += bytesRead;
            
            await targetStream.WriteAsync(buffer, 0, bytesRead);
            
            updateCallback?.Invoke(sourceUrl, (float)(length != 0 ? offset / (double)length : 1));
        }
        
        return true;
    }
}