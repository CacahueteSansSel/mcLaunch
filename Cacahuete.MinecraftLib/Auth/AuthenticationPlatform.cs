namespace Cacahuete.MinecraftLib.Auth;

public abstract class AuthenticationPlatform
{
    public delegate void ProgressCallback(string stepName, int stepIndex, int maxStepCount);
    
    public Action<BrowserLoginCallbackParameters> BrowserLoginCallback { get; private set; }
    public abstract string UserType { get; }
    public abstract string ClientId { get; }
    public abstract bool IsLoggedIn { get; }
    public abstract Task<MinecraftAuthenticationResult?> TryLoginAsync();
    public abstract Task<MinecraftAuthenticationResult?> AuthenticateAsync(ProgressCallback? callback);
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
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string Message { get; set; }

    public string Uuid { get; set; }
    public string Xuid { get; set; }
    public string AccessToken { get; set; }
    public string Username { get; set; }

    public MinecraftAuthenticationResult()
    {
        
    }

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