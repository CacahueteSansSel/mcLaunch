using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models.NeoForge;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class NeoForgeModLoaderSupport : ModLoaderSupport
{
    public const string OlderMavenQueryUrl
        = "https://maven.neoforged.net/api/maven/latest/version/releases/net/neoforged/forge?filter={0}";

    public const string NewerMavenQueryUrl
        = "https://maven.neoforged.net/api/maven/latest/version/releases/net/neoforged/neoforge?filter={0}";

    public NeoForgeModLoaderSupport(string JvmExecutablePath, string systemFolderPath)
    {
        JvmExecutablePath = JvmExecutablePath;
        SystemFolderPath = systemFolderPath;
    }

    public override string Id { get; } = "neoforge";
    public override string Name { get; set; } = "NeoForge";
    public override string Type { get; set; } = "modded";
    public override ModLoaderVersion LatestVersion { get; set; }

    public string JvmExecutablePath { get; }
    public string SystemFolderPath { get; }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        NeoForgeMavenQuery? query = await Api.GetAsync<NeoForgeMavenQuery>
            (string.Format(OlderMavenQueryUrl, minecraftVersion));

        string versionName;
        bool newer = false;

        if (query == null)
        {
            // Recent NeoForge version omits the "1." on the version name. Not sure if it's related to the
            // Minecraft version or not, but we will try that for now

            query = await Api.GetAsync<NeoForgeMavenQuery>
                (string.Format(NewerMavenQueryUrl, minecraftVersion[2..]));

            if (query == null) return null;
            if (!query.Version.StartsWith(minecraftVersion[2..])) return null;

            versionName = query.Version;
            newer = true;
        }
        else
        {
            if (!query.Version.StartsWith($"{minecraftVersion}-")) return null;

            versionName = query.Version.Split('-')[1];
        }

        return new[]
        {
            new NeoForgeModLoaderVersion
            {
                MinecraftVersion = minecraftVersion,
                Name = versionName,
                JvmExecutablePath = JvmExecutablePath,
                SystemFolderPath = SystemFolderPath,
                IsNewerFormat = newer
            }
        };
    }
}