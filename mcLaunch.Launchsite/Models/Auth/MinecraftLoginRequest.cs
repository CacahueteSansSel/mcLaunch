using System.Text.Json.Serialization;

namespace mcLaunch.Launchsite.Models.Auth;

public class MinecraftLoginRequest
{
    public MinecraftLoginRequest(string identityToken)
    {
        IdentityToken = identityToken;
    }

    [JsonPropertyName("identityToken")] public string IdentityToken { get; set; }

    public static MinecraftLoginRequest CreateXbox(string userHash, string xstsToken)
    {
        return new MinecraftLoginRequest($"XBL3.0 x={userHash};{xstsToken}");
    }
}