using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Auth;

public static class Jwt
{
    static string DecodeBase64(string input)
    {
        string fixedBase64 = input.Replace('-', '+').Replace('_', '/');
        int fraction = fixedBase64.Length % 4;

        if (fraction == 2) fixedBase64 += "==";
        else if (fraction == 3) fixedBase64 += "=";
        else if (fraction != 0) throw new Exception("Invalid base64 input !");

        return Encoding.UTF8.GetString(Convert.FromBase64String(fixedBase64));
    }

    public static JwtToken<T> Decode<T>(string token) where T : JwtBody
    {
        string[] tokenParts = token.Split('.');

        JwtHeader header = JsonSerializer.Deserialize<JwtHeader>(DecodeBase64(tokenParts[0]))!;
        T body = JsonSerializer.Deserialize<T>(DecodeBase64(tokenParts[1]))!;

        return new JwtToken<T>
        {
            Header = header,
            Body = body
        };
    }
}

public class JwtToken<T> where T : JwtBody
{
    public JwtHeader Header { get; init; }
    public T Body { get; init; }
}

public class JwtHeader
{
    [JsonPropertyName("typ")] public string Type { get; set; }
    [JsonPropertyName("alg")] public string Algorythm { get; set; }
    [JsonPropertyName("kid")] public string Kid { get; set; }
}

public class JwtBody
{
    [JsonPropertyName("ver")] public string Version { get; set; }
    [JsonPropertyName("iss")] public string Issuer { get; set; }
    [JsonPropertyName("sub")] public string Sub { get; set; }
    [JsonPropertyName("aud")] public string Aud { get; set; }
    [JsonPropertyName("exp")] public int Expiration { get; set; }
    [JsonPropertyName("iat")] public int IssuedAt { get; set; }
    [JsonPropertyName("nbf")] public int Nbf { get; set; }

    [JsonIgnore] public DateTime ExpirationTime => DateTimeOffset.FromUnixTimeSeconds(Expiration).DateTime;
}

public class MinecraftSessionJWTBody : JwtBody
{
    [JsonPropertyName("xuid")] public string Xuid { get; set; }
    [JsonPropertyName("yuid")] public string Yuid { get; set; }
    [JsonPropertyName("auth")] public string Auth { get; set; }
    [JsonPropertyName("agg")] public string Age { get; set; }
}