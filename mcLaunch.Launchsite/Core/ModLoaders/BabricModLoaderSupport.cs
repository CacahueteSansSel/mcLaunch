using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models.Fabric;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class BabricModLoaderSupport : ModLoaderSupport
{
    public const string Url = "https://meta.babric.glass-launcher.net";

    public override string Id { get; } = "babric";
    public override string Name { get; set; } = "Babric";
    public override string Type { get; set; } = "modded";
    public override bool IsAdvanced => true;
    public override ModLoaderVersion LatestVersion { get; set; }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        FabricLoaderManifest[]? versions = await Api.GetAsync<FabricLoaderManifest[]>(
            $"{Url}/v2/versions/loader/{minecraftVersion}",
            true);

        if (versions == null) return null;

        return versions.Select(ver => (ModLoaderVersion)new BabricModLoaderVersion
        {
            Name = ver.Loader.Version,
            MinecraftVersion = minecraftVersion
        }).ToArray();
    }
}