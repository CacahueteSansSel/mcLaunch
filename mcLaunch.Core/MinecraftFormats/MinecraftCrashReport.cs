namespace mcLaunch.Core.MinecraftFormats;

public class MinecraftCrashReport(string path)
{
    public string Filename { get; set; } = Path.GetFileNameWithoutExtension(path);
    public string CompletePath { get; set; } = path;
    public DateTime CrashTime { get; set; } = File.GetCreationTime(path);
}