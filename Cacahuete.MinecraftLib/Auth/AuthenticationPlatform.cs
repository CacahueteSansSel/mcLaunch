namespace Cacahuete.MinecraftLib.Auth;

public abstract class AuthenticationPlatform
{
    public Action<BrowserLoginCallbackParameters> BrowserLoginCallback { get; private set; }
    public abstract string UserType { get; }
    public abstract string ClientId { get; }
    public abstract bool IsLoggedIn { get; }
    public abstract Task<MinecraftAuthenticationResult?> TryLoginAsync();
    public abstract Task<MinecraftAuthenticationResult?> AuthenticateAsync();
    public abstract Task<bool> DisconnectAsync();
    public abstract Task<bool> HasMinecraftAsync(MinecraftAuthenticationResult result);

    public AuthenticationPlatform WithBrowserLoginCallback(Action<BrowserLoginCallbackParameters> callback)
    {
        BrowserLoginCallback = callback;

        return this;
    }
}

public class BrowserLoginCallbackParameters
{
    public string Message { get; set; }
    public string Code { get; set; }
    public string Url { get; set; }
    public string ExpiresAt { get; set; }
}

public class MinecraftAuthenticationResult
{
    public bool IsSuccess { get; }
    public string ErrorCode { get; }
    public string Message { get; }

    public string Uuid { get; }
    public string Xuid { get; }
    public string AccessToken { get; }
    public string Username { get; }

    public MinecraftAuthenticationResult(string errorMessage, string code = null)
    {
        IsSuccess = false;
        Message = errorMessage;
        ErrorCode = code;
    }

    public MinecraftAuthenticationResult(string accessToken, string uuid, string username)
    {
        JwtToken<MinecraftSessionJWTBody> jwt = Jwt.Decode<MinecraftSessionJWTBody>(accessToken);

        Xuid = jwt.Body.Xuid;
        Uuid = uuid;
        Username = username;
        AccessToken = accessToken;

        IsSuccess = true;
    }
}