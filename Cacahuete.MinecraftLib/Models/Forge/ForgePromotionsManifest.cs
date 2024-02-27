using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.Forge;

public class ForgePromotionsManifest
{
    [JsonPropertyName("homepage")] public string Homepage { get; set; }

    [JsonPropertyName("promos")] public JsonElement Promos { get; set; }
}