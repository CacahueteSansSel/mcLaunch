using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Web;
using Cacahuete.MinecraftLib.Http;
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

    public override async Task<AuthenticationResult?> TryLogin()
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
        JavaEditionSessionCache sessionCache = await handler.LoginFromOAuth();
        if (!sessionCache.CheckValidation()) return null;

        minecraftSession = sessionCache.GameSession;

        return new AuthenticationResult(minecraftSession);
    }

    public override async Task<bool> DisconnectAsync()
    {
        await handler.ClearCache();

        return true;
    }
}