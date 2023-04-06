using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using CmlLib.Core.Auth.Microsoft.Cache;
using CmlLib.Core.Auth.Microsoft.MsalClient;
using CmlLib.Core.Auth.Microsoft.XboxLive;
using Microsoft.Identity.Client;

namespace Cacahuete.MinecraftLib.Auth;

public class MicrosoftAuthenticationPlatform : AuthenticationPlatform
{
    public override string UserType { get; } = "msa";
    public override string ClientId => appId;
    public override bool IsLoggedIn => minecraftSession != null && minecraftSession.CheckIsValid();
    string appId;
    MSession? minecraftSession;
    IPublicClientApplication app;
    JavaEditionLoginHandler handler;

    public MicrosoftAuthenticationPlatform(string appId)
    {
        this.appId = appId;

        app = MsalMinecraftLoginHelper.CreateDefaultApplicationBuilder(appId)
            .Build();

        handler = new LoginHandlerBuilder()
            .ForJavaEdition()
            .WithMsalOAuth(app, factory => factory.CreateInteractiveApi())
            .WithXboxAuthNetApi(builder => builder.WithDummyDeviceTokenApi())
            .Build();
    }

    public override async Task<AuthenticationResult?> TryLoginAsync()
    {
        try
        {
            JavaEditionSessionCache sessionCache = await handler.LoginFromCache();
            if (!sessionCache.CheckValidation()) return null;

            minecraftSession = sessionCache.GameSession;

            return new AuthenticationResult(minecraftSession);
        }
        catch (MsalUiRequiredException e)
        {
            return null;
        }
    }

    public override async Task<AuthenticationResult?> AuthenticateAsync()
    {
        try
        {
            JavaEditionSessionCache sessionCache = await handler.LoginFromOAuth();
            if (!sessionCache.CheckValidation()) return null;

            minecraftSession = sessionCache.GameSession;

            return new AuthenticationResult(minecraftSession);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception of type: {e.GetType().FullName}");
            return null;
        }
    }

    public override async Task<bool> DisconnectAsync()
    {
        await handler.ClearCache();

        return true;
    }

    public override async Task<bool> HasMinecraftAsync(AuthenticationResult result)
    {
        string token = result.AccessToken;

        MinecraftStore? store = await Api.GetAsyncAuthBearer<MinecraftStore>
            ("https://api.minecraftservices.com/entitlements/mcstore", token);

        return store.Items.Length > 0;
    }
}