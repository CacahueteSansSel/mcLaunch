using System.Collections.Generic;
using Cacahuete.MinecraftLib.Models;
using Cacahuete.MinecraftLib.Servers;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Core.Managers;

public static class MinecraftManager
{
    static VersionManifest? manifest;
    public static CentralServer CentralServer { get; private set; }
    public static ManifestMinecraftVersion[] ManifestVersions => manifest.Releases;
    public static VersionManifest? Manifest => manifest;

    public static async Task InitAsync()
    {
        if (CentralServer == null) CentralServer = new MojangCentralServer();
        if (manifest == null) manifest = await CentralServer.GetVersionManifestAsync();
    }

    public static async Task<ManifestMinecraftVersion> GetManifestAsync(string versionId)
    {
        if (manifest == null) await InitAsync();
        
        return manifest.Versions.FirstOrDefault(v => v.Id == versionId);
    }

    public static async Task<MinecraftVersion?> GetAsync(string versionId)
    {
        ManifestMinecraftVersion manifest = await GetManifestAsync(versionId);
        return await manifest.DownloadOrGetLocally(BoxManager.SystemFolder);
    }
}