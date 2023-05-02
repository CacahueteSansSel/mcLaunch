using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Core.Mods;

public abstract class ModPlatform
{
    public abstract string Name { get; }
    public Bitmap Icon { get; private set; }

    public abstract Task<Modification[]> GetModsAsync(int page, Box box, string searchQuery);
    public abstract Task<ModDependency[]> GetModDependenciesAsync(string id, string modLoaderId, string versionId, string minecraftVersionId);

    public abstract Task<Modification> GetModAsync(string id);
    public abstract Task<string[]> GetModVersionList(string modId, string modLoaderId, string minecraftVersionId);

    public abstract Task<bool> InstallModAsync(Box targetBox, Modification mod, string versionId, bool installOptional);

    public abstract Task<Modification> DownloadModInfosAsync(Modification mod);
    public abstract ModPlatform GetModPlatform(string id);
    public abstract Task<Modification?> GetModFromSha1(string hash);

    public ModPlatform WithIcon(string name)
    {
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        Icon = new Bitmap(assets.Open(new Uri($"avares://mcLaunch/resources/icons/{name}.png")));

        return this;
    }

    public class ModDependency
    {
        public Modification Mod { get; init; }
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