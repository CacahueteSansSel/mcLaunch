using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.Auth;

public class MinecraftLoginResponse
{
    [JsonPropertyName("username")] public string Username { get; set; }

    [JsonPropertyName("roles")] public string[] Roles { get; set; }

    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("token_type")] public string TokenType { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}