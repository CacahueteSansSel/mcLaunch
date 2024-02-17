using System.Text.Json.Serialization;

namespace mcLaunch.GitHub.Models;

public class GitHubRelease
{
    [JsonPropertyName("body")] public string MarkdownBody { get; set; }
    [JsonPropertyName("url")] public string Url { get; set; }
    [JsonPropertyName("assets_url")] public string AssetsUrl { get; set; }
    [JsonPropertyName("upload_url")] public string UploadUrl { get; set; }
    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; }
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("node_id")] public string NodeId { get; set; }
    [JsonPropertyName("tag_name")] public string TagName { get; set; }
    [JsonPropertyName("target_commitish")] public string TargetCommitish { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("draft")] public bool Draft { get; set; }
    [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }
    [JsonPropertyName("assets")] public GitHubReleaseAsset[] Assets { get; set; }
}