namespace ddLaunch.Core.Mods;

public abstract class ModificationPack
{
    public abstract string Name { get; }
    public abstract string Author { get; }
    public abstract string Version { get; }
    
    public abstract string MinecraftVersion { get; }
    public abstract string ModloaderId { get; }
    public abstract string ModloaderVersion { get; }
    
    public abstract SerializedModification[] Modifications { get; }
    
    public abstract AdditionalFile[] AdditionalFiles { get; }

    public ModificationPack(string filename)
    {
        
    }
    
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