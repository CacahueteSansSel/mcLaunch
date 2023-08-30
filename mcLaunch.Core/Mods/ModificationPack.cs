using mcLaunch.Core.Boxes;

namespace mcLaunch.Core.Mods;

public abstract class ModificationPack
{
    public abstract string Name { get; init; }
    public abstract string Author { get; init; }
    public abstract string Version { get; init; }
    public abstract string? Id { get; init; }
    public abstract string? Description { get; init; }
    
    public abstract string MinecraftVersion { get; init; }
    public abstract string ModloaderId { get; init; }
    public abstract string ModloaderVersion { get; init; }
    
    public abstract SerializedModification[] Modifications { get; set; }
    
    public abstract AdditionalFile[] AdditionalFiles { get; set; }

    public ModificationPack()
    {
        
    }

    public ModificationPack(string filename)
    {
        
    }

    public abstract Task InstallModificationAsync(Box targetBox, SerializedModification mod);

    public abstract Task ExportAsync(Box box, string filename);

    public class SerializedModification
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