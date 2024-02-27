using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.QuickPlay;

public class QuickPlayProfile
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("lastPlayedTime")] public DateTime LastPlayedTime { get; set; }

    [JsonPropertyName("gamemode")] public string Gamemode { get; set; }
}