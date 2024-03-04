using System.Text.Json.Serialization;

namespace mcLaunch.Launchsite.Models.NeoForge;

public class NeoForgeMavenQuery
{
    [JsonPropertyName("isSnapshot")] public bool IsSnapshot { get; set; }

    [JsonPropertyName("version")] public string Version { get; set; }
}