using System.Text.Json.Serialization;

namespace mcLaunch.Launchsite.Models;

public class JvmFileEntry
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("executable")] public bool Executable { get; set; }

    [JsonPropertyName("downloads")] public ModelDownloads Downloads { get; set; }

    public class ModelDownloads
    {
        [JsonPropertyName("lzma")] public FileArtifact LZMACompressed { get; set; }

        [JsonPropertyName("raw")] public FileArtifact Raw { get; set; }
    }
}