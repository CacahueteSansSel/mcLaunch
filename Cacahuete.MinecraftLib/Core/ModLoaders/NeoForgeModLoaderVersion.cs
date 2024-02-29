using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class NeoForgeModLoaderVersion : ForgeModLoaderVersion
{
    public bool IsNewerFormat { get; set; }

    public override async Task<Result<MinecraftVersion>> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string[] installerUrls =
        {
            IsNewerFormat
                ? $"https://maven.neoforged.net/releases/net/neoforged/neoforge/{Name}/neoforge-{Name}-installer.jar"
                : $"https://maven.neoforged.net/releases/net/neoforged/forge/{FullName}/forge-{FullName}-installer.jar"
        };
        
        return await GetForgeMinecraftVersionAsync(minecraftVersionId, installerUrls, "NeoForge");
    }
}