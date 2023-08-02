using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models;

public class AssetIndex
{
    [JsonPropertyName("map_to_resources")] public bool MapToResources { get; set; }
    [JsonPropertyName("objects")] public JsonElement Objects { get; set; }

    public Asset[] ParseAll()
    {
        List<Asset> assets = new();

        foreach (JsonProperty property in Objects.EnumerateObject())
        {
            JsonElement entry = property.Value;

            assets.Add(new Asset
            {
                Name = property.Name,
                Hash = entry.GetProperty("hash").GetString()!,
                Size = entry.GetProperty("size").GetUInt64()
            }.DeducePrefix());
        }

        return assets.ToArray();
    }
}

public class Asset
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public string Prefix { get; set; }
    public ulong Size { get; set; }
    public string Url => $"https://resources.download.minecraft.net/{Prefix}/{Hash}";

    public Asset DeducePrefix()
    {
        Prefix = Hash[..2];

        return this;
    }
}