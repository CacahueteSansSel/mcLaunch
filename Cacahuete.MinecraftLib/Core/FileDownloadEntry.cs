namespace Cacahuete.MinecraftLib.Core;

public class FileDownloadEntry
{
    public string SourceUrl { get; }
    /// <summary>
    /// Target filename relative to the minecraft folder
    /// </summary>
    public string TargetFilename { get; }

    public FileDownloadEntry(string sourceUrl, string targetFilename)
    {
        SourceUrl = sourceUrl;
        TargetFilename = targetFilename;
    }
}