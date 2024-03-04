using System.Text.Json.Serialization;

namespace mcLaunch.Launchsite.Models;

public class MinecraftStore
{
    [JsonPropertyName("items")] public ModelItem[] Items { get; set; }

    [JsonPropertyName("signature")] public string Signature { get; set; }

    [JsonPropertyName("keyId")] public string KeyId { get; set; }

    public class ModelItem
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("signature")] public string Signature { get; set; }
    }
}