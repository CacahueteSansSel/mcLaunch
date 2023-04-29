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
using XboxAuthNet.XboxLive;

namespace Cacahuete.MinecraftLib.Auth;

public class MicrosoftAuthenticationPlatform : AuthenticationPlatform
{
    private static Dictionary<string, string> errors = new()
    {
        {"8015dc03", "The device or user was banned"},
        {"8015dc04", "The device or user was banned"},
        {"8015dc0b", "This resource is not available in the country associated with the user"},
        {"8015dc0c", "Access to this resource requires age verification"},
        {"8015dc0d", "Access to this resource requires age verification"},
        {"8015dc0e", "Child's account is not in the family"},
        {"8015dc09", "Creating a new account is required"},
        {"8015dc10", "Account maintenance is required"}
    };

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
            .WithClientName("mcLaunch")
            .WithClientVersion("1.0.0")
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
            if (e.Message.Contains(", ") && e.Message.Contains("xbox"))
            {
                string code = e.Message.Split(',')[0].Trim();

                return new AuthenticationResult(errors.TryGetValue(code, out string? error) ? error : "Unknown error",
                    code);
            }

            return new AuthenticationResult(e.Message);
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
        catch (XboxAuthException e)
        {
            if (e.Error != null)
            {
                return new AuthenticationResult(
                    errors.TryGetValue(e.Error, out string? error) ? error : "Unknown error", e.Error);
            }

            return new AuthenticationResult(e.Message);
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