using mcLaunch.Launchsite.Auth;
using mcLaunch.Launchsite.Cache;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Core.Managers;

public static class AuthenticationManager
{
    public static AuthenticationPlatform? Platform { get; private set; }

    public static MinecraftAuthenticationResult? Account { get; private set; }
    public static MinecraftProfile? Profile => Account?.Profile;
    public static bool IsInitialized => Platform != null;
    public static CredentialsCache Cache { get; private set; }

    public static event Action<MinecraftAuthenticationResult> OnLogin;
    public static event Action OnDisconnect;

    public static void Init(string microsoftAppId, string credentialsKey)
    {
        Cache = new CredentialsCache(AppdataFolderManager.GetValidPath("system/balance"), credentialsKey);
        Platform = new MicrosoftAuthenticationPlatform(microsoftAppId, Cache);
    }

    public static async Task DisconnectAsync()
    {
        await Platform.DisconnectAsync();
        Account = null;

        OnDisconnect?.Invoke();
    }

    public static void SetAccount(MinecraftAuthenticationResult account)
    {
        Account = account;
        OnLogin?.Invoke(account);
    }

    public static async Task<MinecraftAuthenticationResult?> TryLoginAsync()
    {
        Account = await Platform.TryLoginAsync();

        if (Account != null) OnLogin?.Invoke(Account);

        return Account;
    }

    public static async Task<MinecraftAuthenticationResult?> AuthenticateAsync(
        Action<BrowserLoginCallbackParameters> deviceCodeCallback = null,
        AuthenticationPlatform.ProgressCallback progressCallback = null)
    {
        Platform.WithBrowserLoginCallback(deviceCodeCallback);
        Account = await Platform.AuthenticateAsync(progressCallback);

        if (Account != null) OnLogin?.Invoke(Account);

        return Account;
    }

    public static async Task<bool> HasMinecraftAsync()
    {
        return await Platform.HasMinecraftAsync(Account);
    }
}