using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace mcLaunch.Installer.Core;

public static class Downloader
{
    public static event Action<string, float> OnDownloadProgressUpdate;

    public static async Task<MemoryStream> DownloadToMemoryAsync(string url, long? expectedSize = null)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mcLaunch.Installer", "1.0.0"));

        HttpResponseMessage resp = await client.GetAsync(url,
            HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();

        MemoryStream stream = new();

        Stream downloadStream = await resp.Content.ReadAsStreamAsync();
        long size = resp.Content.Headers.ContentLength ?? expectedSize ?? 0;
        long b = 0;

        while (true)
        {
            double byteProgress = (double) b / size;

            byte[] buffer = new byte[25];
            int input = await downloadStream.ReadAsync(buffer);
            if (input == 0) break;

            await stream.WriteAsync(buffer, 0, input);

            OnDownloadProgressUpdate?.Invoke(url,
                (float) byteProgress);

            b += 25;
        }

        return stream;
    }
}