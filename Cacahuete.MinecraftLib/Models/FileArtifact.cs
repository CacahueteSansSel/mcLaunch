using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models;

public class FileArtifact
{
    [JsonPropertyName("path")] public string Path { get; set; }

    [JsonPropertyName("sha1")] public string Hash { get; set; }

    [JsonPropertyName("size")] public ulong Size { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }
}