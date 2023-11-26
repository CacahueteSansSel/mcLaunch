using System.Diagnostics;
using System.Text.Json;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models.Forge;
using Cacahuete.MinecraftLib.Models.NeoForge;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class NeoForgeModLoaderSupport : ModLoaderSupport
{
    public const string MavenQueryUrl 
        = "https://maven.neoforged.net/api/maven/latest/version/releases/net/neoforged/forge?filter={0}";
    public override string Id { get; } = "neoforge";
    public override string Name { get; set; } = "NeoForge";
    public override string Type { get; set; } = "modded";
    public override ModLoaderVersion LatestVersion { get; set; }

    public string JvmExecutablePath { get; }
    public string SystemFolderPath { get; }

    public NeoForgeModLoaderSupport(string jvmExecutablePath, string systemFolderPath)
    {
        JvmExecutablePath = jvmExecutablePath;
        SystemFolderPath = systemFolderPath;
    }
    
    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        NeoForgeMavenQuery? query = await Api.GetAsync<NeoForgeMavenQuery>
            (string.Format(MavenQueryUrl, minecraftVersion));

        if (query == null) return null;
        if (!query.Version.StartsWith($"{minecraftVersion}-")) return null;

        return new[]
        {
            new NeoForgeModLoaderVersion 
            {
                MinecraftVersion = minecraftVersion,
                Name = query.Version.Split('-')[1],
                JvmExecutablePath = JvmExecutablePath,
                SystemFolderPath = SystemFolderPath
            }
        };
    }
}