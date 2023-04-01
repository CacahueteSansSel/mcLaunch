using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public abstract class ModLoaderSupport
{
    public abstract string Id { get; }
    public abstract string Name { get; set; }
    public abstract string Type { get; set; }
    public abstract ModLoaderVersion LatestVersion { get; set; }
    public virtual bool NeedsMerging => true;

    public abstract Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion);
}

public class ModLoaderVersion
{
    public string Name { get; set; }
    public string MinecraftVersion { get; set; }
    
    public virtual async Task<MinecraftVersion?> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        return null;
    }
}