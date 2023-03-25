using System.Text.Json.Serialization;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Http;

namespace Cacahuete.MinecraftLib.Models;

public class ManifestMinecraftVersion
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("url")] public string ManifestUrl { get; set; }

    [JsonPropertyName("time")] public DateTime Time { get; set; }

    [JsonPropertyName("releaseTime")] public DateTime ReleaseTime { get; set; }

    public Task<MinecraftVersion?> GetAsync() => Api.GetAsync<MinecraftVersion>(ManifestUrl);

    public async Task<MinecraftVersion?> DownloadOrGetLocally(MinecraftFolder folder)
    {
        if (folder.HasVersion(Id)) return folder.GetVersion(Id);

        return await GetAsync();
    }
}