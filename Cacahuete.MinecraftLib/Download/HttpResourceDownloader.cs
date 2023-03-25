using System.IO.Compression;

namespace Cacahuete.MinecraftLib.Download;

public class HttpResourceDownloader : ResourceDownloader
{
    HttpClient client = new();
    
    public override async Task<bool> DownloadAsync(string url, string target)
    {
        CurrentTargetSource = url;
        CurrentTargetFilename = target;
        IsDone = false;
        Progress = 0f;

        HttpResponseMessage resp = await client.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        
        await File.WriteAllBytesAsync(target, await resp.Content.ReadAsByteArrayAsync());

        return true;
    }

    public override async Task<bool> ExtractAsync(string sourceArchive, string targetDir)
    {
        ZipFile.ExtractToDirectory(sourceArchive, targetDir, true);
        return true;
    }
}