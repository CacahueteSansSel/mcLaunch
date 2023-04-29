using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.Auth;

public class MinecraftLoginRequest
{
    public static MinecraftLoginRequest CreateXbox(string userHash, string xstsToken)
        => new($"XBL3.0 x={userHash};{xstsToken}");
    
    [JsonPropertyName("identityToken")]
    public string IdentityToken { get; set; }

    public MinecraftLoginRequest(string identityToken)
    {
        IdentityToken = identityToken;
    }
}