using System.Xml;
using mcLaunch.Launchsite.Http;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class ForgeModLoaderSupport : ModLoaderSupport
{
    public const string PromosUrl = "https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json";

    public ForgeModLoaderSupport(string jvmExecutablePath, string systemFolderPath)
    {
        JvmExecutablePath = jvmExecutablePath;
        SystemFolderPath = systemFolderPath;
    }

    public override string Id { get; } = "forge";
    public override string Name { get; set; } = "Forge";
    public override string Type { get; set; } = "modded";
    public override bool SupportsLauncherExposure => false;
    public override ModLoaderVersion LatestVersion { get; set; }

    public string JvmExecutablePath { get; }
    public string SystemFolderPath { get; }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        if (Version.TryParse(minecraftVersion, out Version? version) && version < new Version("1.12.2"))
            return [];

        try
        {
            XmlDocument? forgeVersionXml =
                await Api.GetAsyncXml("https://maven.minecraftforge.net/net/minecraftforge/forge/maven-metadata.xml");

            if (forgeVersionXml == null)
                return [];

            XmlNode versionsNode = forgeVersionXml!.DocumentElement!.SelectNodes("versioning/versions")![0]!;
            List<ForgeModLoaderVersion> versions = [];

            foreach (XmlNode childVersionNode in versionsNode.ChildNodes)
            {
                string currentVersion = childVersionNode.InnerText;
                if (!currentVersion.StartsWith(minecraftVersion)) continue;

                versions.Add(new ForgeModLoaderVersion
                {
                    Name = currentVersion.Replace($"{minecraftVersion}-", "").Trim(),
                    MinecraftVersion = minecraftVersion,
                    JvmExecutablePath = JvmExecutablePath,
                    SystemFolderPath = SystemFolderPath
                });
            }

            return versions.ToArray();
        }
        catch (XmlException e)
        {
            return [];
        }
    }
}