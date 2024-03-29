﻿using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core.ModLoaders;

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