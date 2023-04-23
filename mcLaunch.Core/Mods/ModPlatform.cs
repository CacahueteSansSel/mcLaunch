using Cacahuete.MinecraftLib.Core.ModLoaders;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Core.Mods;

public abstract class ModPlatform
{
    public abstract string Name { get; }

    public abstract Task<Modification[]> GetModsAsync(int page, Box box, string searchQuery);

    public abstract Task<Modification> GetModAsync(string id);
    public abstract Task<string[]> GetVersionsForMinecraftVersionAsync(string modId, string modLoaderId, string minecraftVersionId);

    public abstract Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId);

    public abstract Task<Modification> DownloadAdditionalInfosAsync(Modification mod);
    public abstract ModPlatform GetModPlatform(string id);
}