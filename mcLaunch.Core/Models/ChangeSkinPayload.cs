using System.Text.Json.Serialization;

namespace mcLaunch.Core.Models;

public class ChangeSkinPayload
{
    [JsonPropertyName("variant")]
    public string Variant { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
}