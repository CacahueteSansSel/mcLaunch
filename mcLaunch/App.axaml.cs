using System;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Models;
using mcLaunch.Managers;
using mcLaunch.Tests;
using mcLaunch.Utilities;
using mcLaunch.Views.Windows;
using ReactiveUI;

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

        #if DEBUG
        TestsManager.Load();
        #endif
        
        CurrentBuild.Load();
        Settings.Load();
        DownloadManager.Init(CurrentBuild.Version.ToString());
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
        if (!Design.IsDesignMode) DiscordManager.Init();
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

        DataContext = new AppDataContext();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }

    public class AppDataContext
    {
        public AppDataContext()
        {
            AboutCommand = ReactiveCommand.Create(ShowAboutWindow);
        }

        public ReactiveCommand<Unit, Unit> AboutCommand { get; set; }

        private void ShowAboutWindow()
        {
            Dispatcher.UIThread.Post(() => { new AboutWindow().Show(MainWindow.Instance); });
        }
    }
}