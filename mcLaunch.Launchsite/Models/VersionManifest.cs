using System.Text.Json.Serialization;

namespace mcLaunch.Launchsite.Models;

public class VersionManifest
{
    [JsonPropertyName("latest")] public LatestVersion Latest { get; set; }

    [JsonPropertyName("versions")] public ManifestMinecraftVersion[] Versions { get; set; }

    [JsonIgnore] public ManifestMinecraftVersion LatestRelease => Versions.First(ver => ver.Id == Latest.Release);
    [JsonIgnore] public ManifestMinecraftVersion LatestSnapshot => Versions.First(ver => ver.Id == Latest.Snapshot);

    [JsonIgnore] public ManifestMinecraftVersion[] Releases => Versions.Where(ver => ver.Type == "release").ToArray();
    [JsonIgnore] public ManifestMinecraftVersion[] Snapshots => Versions.Where(ver => ver.Type == "snapshot").ToArray();

    public ManifestMinecraftVersion? Get(string versionId)
    {
        return Versions.FirstOrDefault(ver => ver.Id == versionId);
    }
}

public class LatestVersion
{
    [JsonPropertyName("release")] public string Release { get; set; }

    [JsonPropertyName("snapshot")] public string Snapshot { get; set; }
}