using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;
using Cacahuete.MinecraftLib.Models.Fabric;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class QuiltModLoaderSupport : ModLoaderSupport
{
    public const string Url = "https://meta.quiltmc.org/";
    public const string MavenUrl = "https://maven.quiltmc.org/";

    public override string Id { get; } = "quilt";
    public override string Name { get; set; } = "Quilt";
    public override string Type { get; set; } = "modded";
    public override ModLoaderVersion LatestVersion { get; set; }

    public QuiltModLoaderSupport()
    {
        
    }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        QuiltLoaderManifest[]? versions = await Api.GetAsync<QuiltLoaderManifest[]>(
            $"{Url}/v3/versions/loader/{minecraftVersion}",
            patchDateTimes: true);

        if (versions == null) return null;

        return versions.Select(ver => (ModLoaderVersion) new QuiltModLoaderVersion
        {
            Name = ver.Loader.Version,
            MinecraftVersion = minecraftVersion
        }).ToArray();
    }
}