namespace mcLaunch.Core.Core;

public class FallbackDownloader : IDisposable
{
    private HttpClient client;

    public event Action<float> ProgressUpdated;
    public event Action<bool, Exception> Finished;

    public FallbackDownloader(string userAgent)
    {
        client = new HttpClient();
        
        client.DefaultRequestHeaders.Add("User-Agent", $"{userAgent} (FallbackDownloader/1.0.0)");
    }

    public async Task<bool> DownloadAsync(string sourceUrl, string targetFilename)
    {
        var response = await client.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            Finished?.Invoke(false, 
                new Exception($"unsuccessful status code ({response.StatusCode} {response.ReasonPhrase})"));
            
            return false;
        }
        
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        await using MemoryStream ramStream = new();

        byte[] buffer = new byte[1024];
        long position = 0;
        float lastProgress = 0;
        long size = response.Content.Headers.ContentLength ?? 0;

        while (stream.CanRead)
        {
            int readSize = await stream.ReadAsync(buffer);
            if (readSize <= 0) break;
            if (readSize != buffer.Length) Array.Resize(ref buffer, readSize);

            position += readSize;
            float progress = (float) position / size;
            
            if (size != 0 && Math.Abs(lastProgress - progress) > 0.05f) ProgressUpdated?.Invoke(progress);
            
            lastProgress = progress;

            await ramStream.WriteAsync(buffer, 0, readSize);
        }

        byte[] data = ramStream.GetBuffer();
        await File.WriteAllBytesAsync(targetFilename, data);

        return true;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}