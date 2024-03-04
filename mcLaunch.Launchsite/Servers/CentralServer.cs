using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Servers;

public abstract class CentralServer
{
    public abstract Task<VersionManifest?> GetVersionManifestAsync();
}