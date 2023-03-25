using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Servers;

public abstract class CentralServer
{
    public abstract Task<VersionManifest?> GetVersionManifestAsync();
}