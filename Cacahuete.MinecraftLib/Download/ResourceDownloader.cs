namespace Cacahuete.MinecraftLib.Download;

public abstract class ResourceDownloader
{
    public string CurrentTargetSource { get; protected set; }
    public string CurrentTargetFilename { get; protected set; }
    public float Progress { get; protected set; }
    public bool IsDone { get; protected set; }

    public abstract Task<bool> DownloadAsync(string url, string target, string? expectedHash);
    public abstract Task<bool> ExtractAsync(string sourceArchive, string targetDir);
    public abstract Task<bool> ChmodAsync(string target, string perms);
    public abstract Task BeginSectionAsync(string sectionName, bool immediate);
    public abstract Task EndSectionAsync(bool immediate);
    public abstract Task SetSectionProgressAsync(string itemName, float progressPercent);
    public abstract Task FlushAsync();
}