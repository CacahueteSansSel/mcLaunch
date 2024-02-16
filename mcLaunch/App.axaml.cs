using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Auth;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods.Platforms;
using mcLaunch.Managers;
using mcLaunch.Utilities;

namespace mcLaunch;

public partial class App : Application
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
        ModPlatformManager.Init(new MultiplexerModPlatform(
            new ModrinthModPlatform().WithIcon("modrinth"),
            new CurseForgeModPlatform(Credentials.Get("curseforge")).WithIcon("curseforge")
        ));
        CacheManager.Init();
        AuthenticationManager.Init(Credentials.Get("azure"), Credentials.Get("tokens"));
        DefaultsManager.Init();
        AnonymityManager.Init();
        MinecraftVersion.ModelArguments.Default =
            JsonSerializer.Deserialize<MinecraftVersion.ModelArguments>(InternalSettings.Get("default_args.json"))!;
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        InitManagers();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}