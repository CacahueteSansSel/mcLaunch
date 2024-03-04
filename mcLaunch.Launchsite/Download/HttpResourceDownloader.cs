using System.IO.Compression;
using System.Security.Cryptography;

namespace mcLaunch.Launchsite.Download;

public class HttpResourceDownloader : ResourceDownloader
{
    private readonly HttpClient client = new();

    public override async Task<bool> DownloadAsync(string url, string target, string? hash)
    {
        CurrentTargetSource = url;
        CurrentTargetFilename = target;
        IsDone = false;
        Progress = 0f;

        if (hash != null && File.Exists(target))
        {
            string localFileHash = Convert.ToHexString(
                SHA1.HashData(await File.ReadAllBytesAsync(target))).ToLower();

            if (localFileHash == hash.ToLower())
                return true;
        }

        if (hash == null && File.Exists(target)) return true;

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

    public override async Task<bool> ChmodAsync(string target, string perms)
    {
        return false;
    }

    public override async Task BeginSectionAsync(string sectionName, bool immediate)
    {
        // Not supported: only used by downloaders with a download queue
    }

    public override async Task EndSectionAsync(bool immediate)
    {
        // Not supported: only used by downloaders with a download queue
    }

    public override async Task SetSectionProgressAsync(string itemName, float progressPercent)
    {
        // Not supported: only used by downloaders with a download queue
    }

    public override async Task FlushAsync()
    {
        // Not supported: only used by downloaders with a download queue
    }

    public override async Task WaitForPendingProcessesAsync()
    {
        // Not supported: only used by downloaders with a download queue
    }
}