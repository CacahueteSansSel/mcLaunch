using mcLaunch.Core.Boxes;

namespace mcLaunch.Core.Contents;

public abstract class ModificationPack
{
    public ModificationPack()
    {
    }

    public ModificationPack(string filename)
    {
    }

    public abstract string Name { get; init; }
    public abstract string Author { get; init; }
    public abstract string Version { get; init; }
    public abstract string? Id { get; init; }
    public abstract string? Description { get; init; }

    public abstract string MinecraftVersion { get; init; }
    public abstract string ModloaderId { get; init; }
    public abstract string ModloaderVersion { get; init; }

    public abstract SerializedMinecraftContent[] Modifications { get; set; }

    public abstract AdditionalFile[] AdditionalFiles { get; set; }

    public abstract Task InstallModificationAsync(Box targetBox, SerializedMinecraftContent mod);

    public abstract Task ExportAsync(Box box, string filename, string[]? includedFiles);

    public class SerializedMinecraftContent
    {
        public string ModId { get; init; }
        public string VersionId { get; init; }
        public string PlatformId { get; init; }
        public bool IsRequired { get; init; }
    }

    public class AdditionalFile
    {
        public string Path { get; init; }
        public byte[] Data { get; init; }
    }
}