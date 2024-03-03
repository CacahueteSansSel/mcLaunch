using System.Text.Json;
using mcLaunch.Launchsite.Models;

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

    public bool HasVersion(string id)
    {
        return Directory.Exists($"{Path}/versions/{id}") && File.Exists($"{Path}/versions/{id}/{id}.jar");
    }

    public string GetVersionPath(string id)
        => $"{Path}/versions/{id}";

    public MinecraftVersion? GetVersion(string id)
    {
        return JsonSerializer.Deserialize<MinecraftVersion>(File.ReadAllText($"{GetVersionPath(id)}/{id}.json"));
    }

    public MinecraftVersion[] GetLocalVersions()
    {
        List<MinecraftVersion> versions = new();

        foreach (string versionDirectory in Directory.GetDirectories($"{Path}/versions"))
            versions.Add(JsonSerializer.Deserialize<MinecraftVersion>(
                File.ReadAllText($"{versionDirectory}/{System.IO.Path.GetFileName(versionDirectory)}.json"))!);

        return versions.ToArray();
    }

    public string GetJvm(string jvmName)
    {
        string platform = $"{Utilities.GetPlatformIdentifier()}-{Utilities.GetArchitecture()}";

        return $"{Path}/runtime/{jvmName}/{platform}/{jvmName}/bin/javaw" +
               (platform.StartsWith("windows") ? ".exe" : "");
    }

    public async Task InstallVersionAsync(MinecraftVersion version, bool force = false)
    {
        string path = $"{Path}/versions/{version.Id}";

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        await version.InstallToAsync(path, force);
    }
}