using System.Text.Json;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core;

public class MinecraftFolder
{
    public string Path { get; private set; }
    public string CompletePath => System.IO.Path.GetFullPath(Path);

    public MinecraftFolder(string path)
    {
        Path = path;
    }

    public bool HasVersion(string id)
        => Directory.Exists($"{Path}/versions/{id}") && File.Exists($"{Path}/versions/{id}/{id}.jar");

    public MinecraftVersion? GetVersion(string id)
        => JsonSerializer.Deserialize<MinecraftVersion>(File.ReadAllText($"{Path}/versions/{id}/{id}.json"));

    public MinecraftVersion[] GetLocalVersions()
    {
        List<MinecraftVersion> versions = new();

        foreach (string versionDirectory in Directory.GetDirectories($"{Path}/versions"))
        {
            versions.Add(JsonSerializer.Deserialize<MinecraftVersion>(
                File.ReadAllText($"{versionDirectory}/{System.IO.Path.GetFileName(versionDirectory)}.json"))!);
        }

        return versions.ToArray();
    }

    public string GetJVM(string jvmName)
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