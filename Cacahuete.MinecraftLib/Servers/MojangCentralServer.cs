using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Servers;

public class MojangCentralServer : CentralServer
{
    public const string Url = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

    public override Task<VersionManifest?> GetVersionManifestAsync()
    {
        return Api.GetAsync<VersionManifest>(Url);
    }
}