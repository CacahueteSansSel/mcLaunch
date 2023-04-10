using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Auth;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods.Platforms;
using ddLaunch.Utilities;

namespace ddLaunch;

public partial class App : Application
{
    public static ArgumentsParser Args { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public async void InitManagers()
    {
        Args = new ArgumentsParser(Environment.GetCommandLineArgs().Skip(1).ToArray());
        
        DownloadManager.Init();
        await MinecraftManager.InitAsync();
        ModLoaderManager.Init();
        ModPlatformManager.Init(new MultiplexerModPlatform(
            new ModrinthModPlatform(),
            new CurseForgeModPlatform(Credentials.Get("curseforge"))
        ));
        CacheManager.Init();
        AuthenticationManager.Init(Credentials.Get("azure"));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitManagers();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}