using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.Auth;

public class XblAuthResponse
{
    public DateTime IssueInstant { get; set; }
    public DateTime NotAfter { get; set; }
    public string Token { get; set; }
    public Dictionary<string, ModelClaim[]> DisplayClaims { get; set; }
    
    public string Identity { get; set; }
    public int XErr { get; set; }
    public string Message { get; set; }
    public string Redirect { get; set; }

    public bool IsError => XErr != 0;

    public class ModelClaim
    {
        [JsonPropertyName("uhs")]
        public string Uhs { get; set; }
    }
}