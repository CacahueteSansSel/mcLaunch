using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;

namespace mcLaunch.Core.Mods;

public abstract class ModPlatform
{
    public abstract string Name { get; }
    public Bitmap Icon { get; private set; }

    public abstract Task<PaginatedResponse<Modification>> GetModsAsync(int page, Box box, string searchQuery);
    public abstract Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery, string minecraftVersion);
    public abstract Task<PaginatedResponse<ModDependency>> GetModDependenciesAsync(string id, string modLoaderId, string versionId, string minecraftVersionId);

    public abstract Task<Modification> GetModAsync(string id);
    public abstract Task<ModVersion[]> GetModVersionsAsync(Modification mod, string modLoaderId, string minecraftVersionId);
    public abstract Task<PlatformModpack> GetModpackAsync(string id);
    public abstract Task<string[]> GetModVersionList(string modId, string modLoaderId, string minecraftVersionId);

    public abstract Task<bool> InstallModAsync(Box targetBox, Modification mod, string versionId, bool installOptional);
    public abstract Task<ModificationPack> LoadModpackFileAsync(string filename);

    public abstract Task<Modification> DownloadModInfosAsync(Modification mod);
    public abstract ModPlatform GetModPlatform(string id);
    public abstract Task<ModVersion?> GetModVersionFromData(Stream stream);

    public ModPlatform WithIcon(string name)
    {
        Icon = new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{name}.png")));

        return this;
    }

    public class ModDependency
    {
        public Modification Mod { get; init; }
        public string VersionId { get; init; }
        public DependencyRelationType Type { get; init; }
    }

    public enum DependencyRelationType
    {
        Required,
        Optional,
        Incompatible,
        Unknown
    }
}