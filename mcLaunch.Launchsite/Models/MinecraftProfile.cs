using System.Text.Json.Serialization;

namespace mcLaunch.Launchsite.Models;

public class MinecraftProfile
{
    [JsonPropertyName("id")] public string Uuid { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("skins")] public ModelSkin[] Skins { get; set; }

    public class ModelSkin
    {
        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("state")] public string State { get; set; }

        [JsonPropertyName("url")] public string Url { get; set; }

        [JsonPropertyName("variant")] public string Variant { get; set; }

        [JsonPropertyName("textureKey")] public string TextureKey { get; set; }

        [JsonPropertyName("alias")] public string Alias { get; set; }
    }
}