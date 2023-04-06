using Cacahuete.MinecraftLib.Auth;

namespace ddLaunch.Core.Managers;

public static class AuthenticationManager
{
    public static AuthenticationPlatform? Platform { get; private set; }
    
    public static AuthenticationResult? Account { get; private set; }
    public static bool IsInitialized => Platform != null;

    public static event Action<AuthenticationResult> OnLogin; 
    public static event Action OnDisconnect; 

    public static void Init(string microsoftAppId)
    {
        Platform = new MicrosoftAuthenticationPlatform(microsoftAppId);
    }

    public static async Task DisconnectAsync()
    {
        await Platform.DisconnectAsync();
        Account = null;
        
        OnDisconnect?.Invoke();
    }

    public static async Task<bool> TryLoginAsync()
    {
        Account = await Platform.TryLoginAsync();
        
        if (Account != null) OnLogin?.Invoke(Account);

        return Account != null;
    }

    public static async Task<bool> AuthenticateAsync()
    {
        Account = await Platform.AuthenticateAsync();
        
        if (Account != null) OnLogin?.Invoke(Account);

        return Account != null;
    }

    public static async Task<bool> HasMinecraftAsync()
    {
        return await Platform.HasMinecraftAsync(Account);
    }
}