using System.Text.Json.Serialization;

namespace mcLaunch.Launchsite.Core.Azul.Models;

public class AzulJvmEntry
{
    [JsonPropertyName("availability_type")]
    public string? AvailabilityType { get; set; }
    
    [JsonPropertyName("distro_version")]
    public int[] DistroVersion { get; set; }
    
    [JsonPropertyName("java_version")]
    public int[] JavaVersion { get; set; }
    
    [JsonPropertyName("download_url")]
    public string? DownloadUrl { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("product")]
    public string? Product { get; set; }
}