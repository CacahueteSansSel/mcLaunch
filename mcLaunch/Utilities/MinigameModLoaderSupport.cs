using System.Threading.Tasks;
using mcLaunch.Launchsite.Core.ModLoaders;

namespace mcLaunch.Utilities;

public class MinigameModLoaderSupport : ModLoaderSupport
{
    public override string Id => "minigame";
    public override string Name { get; set; } = "Minigame";
    public override string Type { get; set; } = "vanilla";
    public override bool IsAdvanced => true;

    public override ModLoaderVersion LatestVersion { get; set; } = new ModLoaderVersion()
        { MinecraftVersion = "1.5.2", Name = "Minigame" };

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        if (minecraftVersion != "1.5.2") return [];

        return [LatestVersion];
    }
}