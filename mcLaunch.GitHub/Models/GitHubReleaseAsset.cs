using System.Text.Json.Serialization;

namespace mcLaunch.GitHub.Models;

public class GitHubReleaseAsset
{
    [JsonPropertyName("url")] public string Url { get; set; }
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("node_id")] public string NodeId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("label")] public string Label { get; set; }
    [JsonPropertyName("content_type")] public string ContentType { get; set; }
    [JsonPropertyName("state")] public string State { get; set; }
    [JsonPropertyName("size")] public ulong Size { get; set; }
    [JsonPropertyName("download_count")] public uint DownloadCount { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; }
}