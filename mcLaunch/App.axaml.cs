using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Managers;
using mcLaunch.Managers;
using mcLaunch.Utilities;

namespace mcLaunch;

public class App : Application
{
    public static ArgumentsParser Args { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public async Task InitManagers()
    {
        Args = new ArgumentsParser(Environment.GetCommandLineArgs().Skip(1).ToArray());

        Settings.Load();
        DownloadManager.Init();
        await MinecraftManager.InitAsync();
        ModLoaderManager.Init();
        ModPlatformManager.Init(new MultiplexerMinecraftContentPlatform(
            new ModrinthMinecraftContentPlatform().WithIcon("modrinth"),
            new CurseForgeMinecraftContentPlatform(Credentials.Get("curseforge")).WithIcon("curseforge")
        ));
        CacheManager.Init();
        AuthenticationManager.Init(Credentials.Get("azure"), Credentials.Get("tokens"));
        DefaultsManager.Init();
        AnonymityManager.Init();
        DiscordManager.Init();
        MinecraftVersion.ModelArguments.Default =
            JsonSerializer.Deserialize<MinecraftVersion.ModelArguments>(InternalSettings.Get("default_args.json"))!;

        AppDomain.CurrentDomain.ProcessExit += ShutdownManagers;
    }

    private void ShutdownManagers(object? sender, EventArgs e)
    {
        DiscordManager.Shutdown();
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        InitManagers();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}