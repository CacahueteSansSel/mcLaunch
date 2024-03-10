using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models.Forge;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class ForgeModLoaderSupport : ModLoaderSupport
{
    public const string PromosUrl = "https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json";

    public ForgeModLoaderSupport(string jvmExecutablePath, string systemFolderPath)
    {
        JvmExecutablePath = jvmExecutablePath;
        SystemFolderPath = systemFolderPath;
    }

    public override string Id { get; } = "forge";
    public override string Name { get; set; } = "Forge";
    public override string Type { get; set; } = "modded";
    public override bool SupportsLauncherExposure => false;
    public override ModLoaderVersion LatestVersion { get; set; }

    public string JvmExecutablePath { get; }
    public string SystemFolderPath { get; }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        if (Version.TryParse(minecraftVersion, out Version? version) && version < new Version("1.12.2"))
            return [];
        
        ForgePromotionsManifest promos = await Api.GetAsync<ForgePromotionsManifest>(PromosUrl);
        string keyRecommended = $"{minecraftVersion}-recommended";
        string keyLatest = $"{minecraftVersion}-latest";

        string? forgeRecommendedVersion = null;
        string? forgeLatestVersion = null;

        try
        {
            forgeRecommendedVersion = promos.Promos.GetProperty(keyRecommended).GetString();
        }
        catch
        {
        }

        try
        {
            forgeLatestVersion = promos.Promos.GetProperty(keyLatest).GetString();
        }
        catch
        {
        }

        List<ForgeModLoaderVersion> versions = new();

        if (forgeLatestVersion != null)
            versions.Add(new ForgeModLoaderVersion
            {
                MinecraftVersion = minecraftVersion,
                Name = forgeLatestVersion,
                JvmExecutablePath = JvmExecutablePath,
                SystemFolderPath = SystemFolderPath
            });
        if (forgeRecommendedVersion != null)
            versions.Add(new ForgeModLoaderVersion
            {
                MinecraftVersion = minecraftVersion,
                Name = forgeRecommendedVersion,
                JvmExecutablePath = JvmExecutablePath,
                SystemFolderPath = SystemFolderPath
            });

        return versions.ToArray();
    }
}