﻿using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public abstract class ModLoaderSupport
{
    public abstract string Id { get; }
    public abstract string Name { get; set; }
    public abstract string Type { get; set; }
    public abstract ModLoaderVersion LatestVersion { get; set; }
    public virtual bool NeedsMerging => true;
    public virtual bool SupportsLauncherExposure => true;

    public abstract Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion);

    public virtual async Task<Result> FinalizeMinecraftInstallationAsync(string jarFilename, string[] additionalFiles)
    {
        return new Result();
    }

    public async Task<ModLoaderVersion?> FetchLatestVersion(string minecraftVersion)
    {
        ModLoaderVersion[]? versions = await GetVersionsAsync(minecraftVersion);

        if (versions == null || versions.Length == 0) return null;

        LatestVersion = versions[0];
        return LatestVersion;
    }

    public virtual async Task<MinecraftVersion> PostProcessMinecraftVersionAsync(MinecraftVersion minecraftVersion)
    {
        return minecraftVersion;
    }
}

public class ModLoaderVersion
{
    public string Name { get; set; }
    public string MinecraftVersion { get; set; }

    public virtual async Task<Result<MinecraftVersion>> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        return null;
    }
}