using System.Text.Json;
using mcLaunch.Launchsite.Models;
using mcLaunch.Launchsite.Utils;

namespace mcLaunch.Launchsite.Core;

public class MinecraftFolder
{
    public MinecraftFolder(string path)
    {
        Path = path;

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    public string Path { get; }
    public string CompletePath => System.IO.Path.GetFullPath(Path);

    public bool HasVersion(string id) =>
        Directory.Exists($"{Path}/versions/{id}") && File.Exists($"{Path}/versions/{id}/{id}.jar");

    public string GetVersionPath(string id) => $"{Path}/versions/{id}".FixPath();

    public MinecraftVersion? GetVersion(string id) =>
        JsonSerializer.Deserialize<MinecraftVersion>(File.ReadAllText($"{GetVersionPath(id)}/{id}.json"));

    public MinecraftVersion[] GetLocalVersions()
    {
        List<MinecraftVersion> versions = new();

        foreach (string versionDirectory in Directory.GetDirectories($"{Path}/versions"))
        {
            string jsonPath = $"{versionDirectory}/{System.IO.Path.GetFileName(versionDirectory)}.json";
            if (!File.Exists(jsonPath)) continue;
            
            versions.Add(JsonSerializer.Deserialize<MinecraftVersion>(
                File.ReadAllText(jsonPath))!);
        }

        return versions.ToArray();
    }

    public string GetJvm(string jvmName)
    {
        string platform = $"{Utilities.GetPlatformIdentifier()}-{Utilities.GetArchitecture()}";

        return ($"{Path}/runtime/{jvmName}/{platform}/{jvmName}/bin/javaw" +
               (platform.StartsWith("windows") ? ".exe" : "")).FixPath();
    }

    public async Task InstallVersionAsync(MinecraftVersion version, bool force = false)
    {
        string path = $"{Path}/versions/{version.Id}".FixPath();

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        await version.InstallToAsync(path, force);
    }
}