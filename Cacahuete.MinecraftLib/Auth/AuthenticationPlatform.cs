﻿using CmlLib.Core.Auth;

namespace Cacahuete.MinecraftLib.Auth;

public abstract class AuthenticationPlatform
{
    public abstract string UserType { get; }
    public abstract string ClientId { get; }
    public abstract bool IsLoggedIn { get; }
    public abstract Task<AuthenticationResult?> TryLogin();
    public abstract Task<AuthenticationResult?> AuthenticateAsync();
    public abstract Task<bool> DisconnectAsync();
}

public class AuthenticationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    
    public string Uuid { get; }
    public string Xuid { get; }
    public string AccessToken { get; }
    public string Username { get; }

    public AuthenticationResult(string errorMessage)
    {
        IsSuccess = false;
        Message = errorMessage;
    }

    public AuthenticationResult(MSession session)
    {
        JwtToken<MinecraftSessionJWTBody> jwt = Jwt.Decode<MinecraftSessionJWTBody>(session.AccessToken);
        
        Xuid = jwt.Body.Xuid;
        Uuid = session.UUID;
        Username = session.Username;
        AccessToken = session.AccessToken;

        IsSuccess = true;
    }
}