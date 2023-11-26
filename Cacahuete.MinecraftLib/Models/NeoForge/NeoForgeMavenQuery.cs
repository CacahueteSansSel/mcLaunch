using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.NeoForge;

public class NeoForgeMavenQuery
{
    [JsonPropertyName("isSnapshot")]
    public bool IsSnapshot { get; set; }
    
    [JsonPropertyName("version")]
    public string Version { get; set; }
}