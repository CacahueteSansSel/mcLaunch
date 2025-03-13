namespace mcLaunch.Core.Core;

public class FallbackDownloader : IDisposable
{
    private readonly HttpClient client;

    public FallbackDownloader(string userAgent)
    {
        client = new HttpClient();

        client.DefaultRequestHeaders.Add("User-Agent", $"{userAgent} (FallbackDownloader/1.0.0)");
    }

    public void Dispose()
    {
        client.Dispose();
    }

    public event Action<float> ProgressUpdated;
    public event Action<bool, Exception> Finished;

    public async Task<bool> DownloadAsync(string sourceUrl, string targetFilename)
    {
        HttpResponseMessage? response = await client.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            Finished?.Invoke(false,
                new Exception($"unsuccessful status code ({response.StatusCode} {response.ReasonPhrase})"));

            return false;
        }

        string? directory = Path.GetDirectoryName(targetFilename);
        if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

        await using Stream stream = await response.Content.ReadAsStreamAsync();
        await using FileStream fileStream = new(targetFilename, FileMode.Create);

        byte[] buffer = new byte[1024];
        long position = 0;
        float lastProgress = 0;
        long size = response.Content.Headers.ContentLength ?? 0;

        ProgressUpdated?.Invoke(0);

        while (stream.CanRead)
        {
            int readSize = await stream.ReadAsync(buffer);
            if (readSize <= 0) break;
            if (readSize != buffer.Length) Array.Resize(ref buffer, readSize);

            position += readSize;
            float progress = (float)position / size;

            if (size != 0 && Math.Abs(lastProgress - progress) > 0.05f) ProgressUpdated?.Invoke(progress);

            lastProgress = progress;

            await fileStream.WriteAsync(buffer, 0, readSize);
        }

        ProgressUpdated?.Invoke(1);

        if (!fileStream.SafeFileHandle.IsClosed)
            fileStream.SafeFileHandle.Close();

        return true;
    }
}