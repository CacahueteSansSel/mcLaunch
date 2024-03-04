using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models.Fabric;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class FabricModLoaderSupport : ModLoaderSupport
{
    public const string Url = "https://meta.fabricmc.net";
    public const string MavenUrl = "https://maven.fabricmc.net/";

    public override string Id { get; } = "fabric";
    public override string Name { get; set; } = "Fabric";
    public override string Type { get; set; } = "modded";
    public override ModLoaderVersion LatestVersion { get; set; }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        FabricLoaderManifest[]? versions = await Api.GetAsync<FabricLoaderManifest[]>(
            $"{Url}/v2/versions/loader/{minecraftVersion}",
            true);

        if (versions == null) return null;

        return versions.Select(ver => (ModLoaderVersion) new FabricModLoaderVersion
        {
            Name = ver.Loader.Version,
            MinecraftVersion = minecraftVersion
        }).ToArray();
    }
}