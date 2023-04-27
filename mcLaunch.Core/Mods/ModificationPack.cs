using mcLaunch.Core.Boxes;

namespace mcLaunch.Core.Mods;

public abstract class ModificationPack
{
    public abstract string Name { get; }
    public abstract string Author { get; }
    public abstract string Version { get; }
    public abstract string? Id { get; }
    public abstract string? Description { get; }
    
    public abstract string MinecraftVersion { get; }
    public abstract string ModloaderId { get; }
    public abstract string ModloaderVersion { get; }
    
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