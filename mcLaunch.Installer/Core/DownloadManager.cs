using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;

namespace mcLaunch.Installer.Core;

public static class DownloadManager
{
    public static event Action<string, float> OnDownloadProgressUpdate;

    public static async Task<MemoryStream> DownloadToMemoryAsync(string url, long? expectedSize = null)
    {
        HttpClient client = new();

        HttpResponseMessage resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();
        
        await using Stream sourceStream = await resp.Content.ReadAsStreamAsync();
        MemoryStream targetStream = new MemoryStream();
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
            
            OnDownloadProgressUpdate?.Invoke(Path.GetFileName(url), (float)(length != 0 ? offset / (double)length : 1));
        }
        
        return targetStream;
    }
}