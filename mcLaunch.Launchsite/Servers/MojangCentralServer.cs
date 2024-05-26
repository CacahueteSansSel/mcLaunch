using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Servers;

public class MojangCentralServer : CentralServer
{
    public const string Url = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

    public override Task<VersionManifest?> GetVersionManifestAsync() => Api.GetAsync<VersionManifest>(Url);
}