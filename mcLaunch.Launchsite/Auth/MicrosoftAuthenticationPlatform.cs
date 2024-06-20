using mcLaunch.Launchsite.Cache;
using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;
using mcLaunch.Launchsite.Models.Auth;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace mcLaunch.Launchsite.Auth;

public class MicrosoftAuthenticationPlatform : AuthenticationPlatform
{
    private static readonly string[] scopes = {"XboxLive.signin"};
    private readonly IPublicClientApplication app;
    private readonly CredentialsCache? cache;
    private readonly StorageCreationProperties storage;

    public MicrosoftAuthenticationPlatform(string clientId, CredentialsCache? cache)
    {
        ClientId = clientId;
        this.cache = cache;

        storage =
            new StorageCreationPropertiesBuilder(AuthConfig.CacheFileName, Path.GetFullPath(cache.RootPath))
                .WithLinuxKeyring(
                    AuthConfig.LinuxKeyRingSchema,
                    AuthConfig.LinuxKeyRingCollection,
                    AuthConfig.LinuxKeyRingLabel,
                    AuthConfig.LinuxKeyRingAttr1,
                    AuthConfig.LinuxKeyRingAttr2)
                .WithMacKeyChain(
                    AuthConfig.KeyChainServiceName,
                    AuthConfig.KeyChainAccountName)
                .Build();

        app = PublicClientApplicationBuilder.Create(clientId)
            .WithTenantId("consumers")
            .WithClientName("mcLaunch")
            .WithClientVersion("1.0.0")
            .Build();

        RegisterCache();
    }

    public override string UserType { get; }
    public override string ClientId { get; }
    public override bool IsLoggedIn { get; }

    private async void RegisterCache()
    {
        var cacheHelper = await MsalCacheHelper.CreateAsync(storage);
        cacheHelper.RegisterCache(app.UserTokenCache);
    }

    public override async Task<MinecraftAuthenticationResult?> TryLoginAsync()
    {
        var accounts = await app.GetAccountsAsync();

        try
        {
            AuthenticationResult result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                .ExecuteAsync();

            string token = result.AccessToken;

            return await LoginXboxLive(token, null);
        }
        catch (MsalUiRequiredException e)
        {
            return null;
        }
    }

    private async Task<MinecraftAuthenticationResult?> LoginXboxLive(string accessToken, ProgressCallback? callback)
    {
        MinecraftAuthenticationResult? cachedAuth = cache?.Get<MinecraftAuthenticationResult>("mcAuth");
        if (cachedAuth != null && cachedAuth.Validate()) return cachedAuth;

        // https://wiki.vg/Microsoft_Authentication_Scheme

        callback?.Invoke("Logging in to Xbox Live... (step 1)", 1, 4);

        // Authenticate with Xbox Live
        XblAuthResponse? resp = await Api.PostAsync<XblAuthRequest, XblAuthResponse>(
            "https://user.auth.xboxlive.com/user/authenticate", new XblAuthRequest(accessToken));

        if (resp == null) return new MinecraftAuthenticationResult("Failed to login to Xbox Live (step 1)");

        callback?.Invoke("Logging in to Xbox Live... (step 2)", 2, 4);

        string xblToken = resp.Token;
        string uhs = resp.DisplayClaims["xui"][0].Uhs;

        // Obtain XSTS token for Minecraft
        XblAuthResponse? xstsResp = await Api.PostAsync<XblXstsRequest, XblAuthResponse>(
            "https://xsts.auth.xboxlive.com/xsts/authorize", new XblXstsRequest(xblToken));

        if (xstsResp == null)
            return new MinecraftAuthenticationResult("Failed to login to Xbox Live (step 2: failed to get XSTS token)");

        if (xstsResp.IsError)
            return new MinecraftAuthenticationResult(
                $"Failed to login to Xbox Live (step 2: failed to get XSTS token) error code {xstsResp.XErr} {xstsResp.Message}");

        callback?.Invoke("Logging in to Minecraft... (step 1)", 3, 4);

        MinecraftLoginResponse? mcResp = await Api.PostAsync<MinecraftLoginRequest, MinecraftLoginResponse>(
            "https://api.minecraftservices.com/authentication/login_with_xbox",
            MinecraftLoginRequest.CreateXbox(uhs, xstsResp.Token));

        if (mcResp == null) return new MinecraftAuthenticationResult("Failed to login to Minecraft (step 1)");

        string mcToken = mcResp.AccessToken;

        callback?.Invoke("Logging in to Minecraft... (step 2: fetching profile)", 4, 4);

        MinecraftProfile? profile = await Api.GetAsyncAuthBearer<MinecraftProfile>(
            "https://api.minecraftservices.com/minecraft/profile",
            mcToken);

        if (profile == null || string.IsNullOrEmpty(profile.Uuid))
            return new MinecraftAuthenticationResult(
                "Failed to login to Minecraft (step 2: fetching Minecraft profile) : Do you really own Minecraft ?");

        MinecraftAuthenticationResult authResult = new MinecraftAuthenticationResult(mcToken, profile);

        cache?.Set("mcAuth", authResult);

        return authResult;
    }

    public override async Task<MinecraftAuthenticationResult?> AuthenticateAsync(ProgressCallback? callback)
    {
        AuthenticationResult? result = await app.AcquireTokenWithDeviceCode(scopes, async result =>
        {
            BrowserLoginCallback?.Invoke(new BrowserLoginCallbackParameters
            {
                Message = result.Message,
                Code = result.UserCode,
                Url = result.VerificationUrl,
                ExpiresAt = result.ExpiresOn.ToString()
            });
        }).ExecuteAsync();

        if (result != null) return await LoginXboxLive(result.AccessToken, callback);

        return null;
    }

    public override async Task<bool> DisconnectAsync()
    {
        cache?.ClearAll();

        return false;
    }

    public override async Task<bool> HasMinecraftAsync(MinecraftAuthenticationResult result)
    {
        string token = result.AccessToken;

        MinecraftStore? store = await Api.GetAsyncAuthBearer<MinecraftStore>
            ("https://api.minecraftservices.com/entitlements/mcstore", token);

        if (store == null) return true;

        return store.Items.Length > 0;
    }

    public override async Task<MinecraftProfile> FetchProfileAsync(string accessToken)
    {
        MinecraftProfile? profile = await Api.GetAsyncAuthBearer<MinecraftProfile>(
            "https://api.minecraftservices.com/minecraft/profile",
            accessToken);

        return profile;
    }
}