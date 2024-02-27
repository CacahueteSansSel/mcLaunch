using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;

namespace mcLaunch.Managers;

public static class DiscordManager
{
    private const string DiscordAppId = "1212095257563373720";

    private static DiscordRpcClient _client;

    public static void Init()
    {
        _client = new DiscordRpcClient(DiscordAppId);
        _client.Logger = new ConsoleLogger(LogLevel.None);

        _client.OnReady += OnReady;

        _client.Initialize();

        SetPresenceStart();
    }

    public static void Shutdown()
    {
        _client.Dispose();
    }

    private static void OnReady(object sender, ReadyMessage args)
    {
    }

    public static void SetPresence(string state, string? details, string asset, string assetDescription)
    {
        if (!Settings.Instance!.UseDiscordRpc) return;

        _client.SetPresence(new RichPresence
        {
            Details = details ?? $"mcLaunch v{CurrentBuild.Version}",
            State = state,
            Assets = new Assets
            {
                LargeImageKey = asset,
                LargeImageText = assetDescription
            },
            Timestamps = Timestamps.Now,
            Buttons =
            [
                new Button
                {
                    Label = "Visit mcLaunch website",
                    Url = "https://mclaunch.cacahuete.dev"
                }
            ]
        });
    }

    public static void SetPresenceStart()
    {
        SetPresence("Starting", null, "power", "Starting");
    }

    public static void SetPresenceBoxList()
    {
        SetPresence("Among their boxes", $"{BoxManager.BoxCount} boxes", "boxes", "Watching boxes");
    }

    public static void SetPresenceModpacksList()
    {
        SetPresence("Browsing modpacks", null, "boxes", "Browsing modpacks");
    }

    public static void SetPresenceModsList()
    {
        SetPresence("Browsing mods", null, "mods", "Browsing mods");
    }

    public static void SetPresenceBox(Box box)
    {
        if (!Settings.Instance!.ShowBoxInfosOnDiscordRpc)
        {
            SetPresence("Viewing a box", null, "boxes", "Viewing a box");

            return;
        }

        SetPresence($"Viewing {box.Manifest.Name}",
            $"Minecraft {box.Manifest.Version} {box.Manifest.ModLoader?.Name}",
            "boxes", "Viewing a box");
    }

    public static void SetPresenceEditingBox(Box box)
    {
        if (!Settings.Instance!.ShowBoxInfosOnDiscordRpc)
        {
            SetPresence("Editing a box", null, "edit", "Editing a box");

            return;
        }

        SetPresence($"Editing {box.Manifest.Name}",
            $"Minecraft {box.Manifest.Version} {box.Manifest.ModLoader?.Name}",
            "boxes", "Editing a box");
    }

    public static void SetPresenceLaunching(Box box)
    {
        if (!Settings.Instance!.ShowBoxInfosOnDiscordRpc)
        {
            SetPresence("Launching a box", null, "launch", "Launching Minecraft");

            return;
        }

        SetPresence($"Launching {box.Manifest.Name}",
            $"Minecraft {box.Manifest.Version} {box.Manifest.ModLoader?.Name}",
            "launch", "Launching Minecraft");
    }
}