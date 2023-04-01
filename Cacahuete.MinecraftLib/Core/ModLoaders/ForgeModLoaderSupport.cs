using System.Diagnostics;
using System.Text.Json;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models.Forge;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class ForgeModLoaderSupport : ModLoaderSupport
{
    public const string PromosUrl = "https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json";
    public override string Id { get; } = "forge";
    public override string Name { get; set; } = "Forge";
    public override string Type { get; set; } = "modded";
    public override ModLoaderVersion LatestVersion { get; set; }

    public string JvmExecutablePath { get; }
    public string SystemFolderPath { get; }

    public ForgeModLoaderSupport(string jvmExecutablePath, string systemFolderPath)
    {
        JvmExecutablePath = jvmExecutablePath;
        SystemFolderPath = systemFolderPath;
    }
    
    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        ForgePromotionsManifest promos = await Api.GetAsync<ForgePromotionsManifest>(PromosUrl);
        string keyRecommended = $"{minecraftVersion}-recommended";
        string keyLatest = $"{minecraftVersion}-latest";
        string key = keyRecommended;

        if (!promos.Promos.TryGetProperty(key, out _))
        {
            key = keyLatest;
            
            if (!promos.Promos.TryGetProperty(key, out _))
            {
                Debug.WriteLine($"Cannot find any Forge version for {minecraftVersion}");
                return null;
            }
        }

        string forgeVersion = promos.Promos.GetProperty(key).GetString();
        
        return new []
        {
            new ForgeModLoaderVersion
            {
                MinecraftVersion = minecraftVersion,
                Name = forgeVersion,
                JvmExecutablePath = JvmExecutablePath,
                SystemFolderPath = SystemFolderPath
            }
        };
    }
}