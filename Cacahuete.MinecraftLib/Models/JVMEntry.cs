using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models;

public class JVMEntry
{
    [JsonPropertyName("availability")] public ModelAvailability Availability { get; set; }

    [JsonPropertyName("manifest")] public FileArtifact Manifest { get; set; }

    [JsonPropertyName("version")] public ModelVersion Version { get; set; }

    public class ModelVersion
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("released")] public string ReleasedAt { get; set; }
    }

    public class ModelAvailability
    {
        [JsonPropertyName("group")] public int Group { get; set; }

        [JsonPropertyName("progress")] public int Progress { get; set; }
    }
}