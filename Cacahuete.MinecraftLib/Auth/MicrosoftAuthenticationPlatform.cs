using System.Net;
using Cacahuete.MinecraftLib.Cache;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;
using Cacahuete.MinecraftLib.Models.Auth;
using Microsoft.Identity.Client;

namespace Cacahuete.MinecraftLib.Auth;

public class MicrosoftAuthenticationPlatform : AuthenticationPlatform
{
    static readonly string[] scopes = new[] {"XboxLive.signin"};
    public override string UserType { get; }
    public override string ClientId { get; }
    public override bool IsLoggedIn { get; }
    private IPublicClientApplication app;
    private CredentialsCache? cache;

    public MicrosoftAuthenticationPlatform(string clientId, CredentialsCache? cache)
    {
        ClientId = clientId;
        this.cache = cache;

        app = PublicClientApplicationBuilder.Create(clientId)
            .WithTenantId("consumers")
            .WithClientName("mcLaunch")
            .WithClientVersion("1.0.0")
            .Build();
    }

    public override async Task<MinecraftAuthenticationResult?> TryLoginAsync()
    {
        var accounts = await app.GetAccountsAsync();

        try
        {
            string? token = cache?.Get<string>("msaMinecraftTokens");

            if (token == null)
            {
                AuthenticationResult result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();

                token = result.AccessToken;
                
                cache?.Set("msaMinecraftTokens", token);
            }

            return await LoginXboxLive(token, null);
        }
        catch (MsalUiRequiredException e)
        {
            return null;
        }
    }

    async Task<MinecraftAuthenticationResult?> LoginXboxLive(string accessToken, ProgressCallback? callback)
    {
        // https://wiki.vg/Microsoft_Authentication_Scheme
        
        callback?.Invoke("Logging in to Xbox Live... (step 1)", 1, 4);

        // Authenticate with Xbox Live
        XblAuthResponse? resp = await Api.PostAsync<XblAuthRequest, XblAuthResponse>(
            "https://user.auth.xboxlive.com/user/authenticate", new XblAuthRequest(accessToken));

        if (resp == null)
        {
            return new MinecraftAuthenticationResult("Failed to login to Xbox Live (step 1)");
        }
        
        callback?.Invoke("Logging in to Xbox Live... (step 2)", 2, 4);

        string xblToken = resp.Token;
        string uhs = resp.DisplayClaims["xui"][0].Uhs;

        // Obtain XSTS token for Minecraft
        XblAuthResponse? xstsResp = await Api.PostAsync<XblXstsRequest, XblAuthResponse>(
            "https://xsts.auth.xboxlive.com/xsts/authorize", new XblXstsRequest(xblToken));

        if (xstsResp == null)
        {
            return new MinecraftAuthenticationResult("Failed to login to Xbox Live (step 2: failed to get XSTS token)");
        }

        if (xstsResp.IsError)
        {
            return new MinecraftAuthenticationResult(
                $"Failed to login to Xbox Live (step 2: failed to get XSTS token) error code {xstsResp.XErr} {xstsResp.Message}");
        }
        
        callback?.Invoke("Logging in to Minecraft... (step 1)", 3, 4);

        MinecraftLoginResponse? mcResp = await Api.PostAsync<MinecraftLoginRequest, MinecraftLoginResponse>(
            "https://api.minecraftservices.com/authentication/login_with_xbox",
            MinecraftLoginRequest.CreateXbox(uhs, xstsResp.Token));

        if (mcResp == null)
        {
            return new MinecraftAuthenticationResult("Failed to login to Minecraft (step 1)");
        }

        string mcToken = mcResp.AccessToken;
        
        callback?.Invoke("Logging in to Minecraft... (step 2: fetching profile)", 4, 4);

        MinecraftProfile? profile = await Api.GetAsyncAuthBearer<MinecraftProfile>(
            "https://api.minecraftservices.com/minecraft/profile",
            mcToken);

        if (profile == null || string.IsNullOrEmpty(profile.Uuid))
        {
            return new MinecraftAuthenticationResult("Failed to login to Minecraft (step 2: fetching Minecraft profile) : Do you really own Minecraft ?");
        }
        
        return new MinecraftAuthenticationResult(mcToken, profile.Uuid, profile.Name);
    }

    public override async Task<MinecraftAuthenticationResult?> AuthenticateAsync(ProgressCallback? callback)
    {
        AuthenticationResult? result = await app.AcquireTokenWithDeviceCode(scopes, async (result) =>
        {
            BrowserLoginCallback?.Invoke(new BrowserLoginCallbackParameters
            {
                Message = result.Message,
                Code = result.UserCode,
                Url = result.VerificationUrl,
                ExpiresAt = result.ExpiresOn.ToString()
            });
        }).ExecuteAsync();
                
        cache?.Set("msaMinecraftTokens", result.AccessToken);

        if (result != null) return await LoginXboxLive(result.AccessToken, callback);

        return null;
    }

    public override async Task<bool> DisconnectAsync()
    {
        return false;
    }

    public override async Task<bool> HasMinecraftAsync(MinecraftAuthenticationResult result)
    {
        string token = result.AccessToken;

        MinecraftStore? store = await Api.GetAsyncAuthBearer<MinecraftStore>
            ("https://api.minecraftservices.com/entitlements/mcstore", token);

        return store.Items.Length > 0;
    }
}