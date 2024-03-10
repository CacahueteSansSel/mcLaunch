using mcLaunch.Core.Boxes;
using mcLaunch.Launchsite.Models;
using mcLaunch.Launchsite.Servers;

namespace mcLaunch.Core.Managers;

public static class MinecraftManager
{
    public static CentralServer CentralServer { get; private set; }
    public static ManifestMinecraftVersion[] ManifestVersions => Manifest.Releases;
    public static VersionManifest? Manifest { get; private set; }

    public static async Task InitAsync()
    {
        if (CentralServer == null) CentralServer = new MojangCentralServer();
        if (Manifest == null) Manifest = await CentralServer.GetVersionManifestAsync();
    }

    public static async Task<ManifestMinecraftVersion> GetManifestAsync(string versionId)
    {
        if (Manifest == null) await InitAsync();

        return Manifest.Versions.FirstOrDefault(v => v.Id == versionId);
    }

    public static async Task<MinecraftVersion?> GetAsync(string versionId)
    {
        ManifestMinecraftVersion manifest = await GetManifestAsync(versionId);
        return await manifest.DownloadOrGetLocally(BoxManager.SystemFolder);
    }
}