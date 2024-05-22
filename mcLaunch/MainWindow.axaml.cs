using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Avalonia.Controls;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.GitHub;
using mcLaunch.Launchsite.Auth;
using mcLaunch.Launchsite.Http;
using mcLaunch.Managers;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using mcLaunch.Views.Windows;

namespace mcLaunch;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Instance = this;
        InitializeComponent();

        SetTitle("mcLaunch");

        if (OperatingSystem.IsLinux())
            // Hide the top header bar on Linux, because we can't have borderless windows on Linux with Avalonia
            // apparently
            TopHeaderBar.IsVisible = false;

        UpdateBar.IsVisible = false;
        Api.SetUserAgent(new ProductInfoHeaderValue("mcLaunch", CurrentBuild.Version.ToString()));

        IconCollection.Default = IconCollection.FromResources("default_mod_logo.png");
        IconCollection.Default.LoadAsync();

        DataContext = new MainWindowDataContext(null, false);

        MainWindowDataContext.Instance.ShowStartingPage();
        Initialize();
    }

    public static MainWindow Instance { get; private set; }

    public void SetTitle(string title)
    {
        Title = $"{title} - mcLaunch";
        TopHeaderBar?.SetTitle(title);
    }

    public void SetDecorations(bool showDecorations)
    {
        TopBar.IsVisible = showDecorations;
        TopHeaderBar.IsVisible = showDecorations && !OperatingSystem.IsLinux();
    }

    async Task ProcessFastLaunchBoxAsync()
    {
        string boxId = App.Args.Get("box-id");
        Box? box = (await BoxManager.LoadLocalBoxesAsync(true, false))
            .FirstOrDefault(b => b.Manifest.Id == boxId);

        Navigation.ShowPopup(new ConfirmMessageBoxPopup("Keep this FastLaunch instance ?",
            "Do you want to keep this FastLaunch instance ? If you delete it, it will be lost forever !",
            () =>
            {
                if (box == null) return;

                Navigation.ShowPopup(new EditBoxPopup(box, false));
            }, () =>
            {
                if (box == null) return;

                Directory.Delete(box.Path, true);
            })
        );

        await Task.Run(async () =>
        {
            while (MainWindowDataContext.Instance.IsPopup)
                await Task.Delay(100);
        });
    }

    private async void Initialize()
    {
        await Task.Run(async () =>
        {
            while (!AuthenticationManager.IsInitialized)
                await Task.Delay(1);
        });

        if (AppdataFolderManager.NeedsMigration)
        {
            Navigation.ShowPopup(new DataMigrationPopup());

            await Task.Run(async () =>
            {
                while (MainWindowDataContext.Instance.IsPopup || DataMigrationPopup.IsActive)
                    await Task.Delay(100);
            });
        }

        if (App.Args.Contains("from-guard"))
        {
            if (!int.TryParse(App.Args.Get("exit-code"), out int exitCode))
                return;

            if (exitCode == 0) await ProcessFastLaunchBoxAsync();
            else Navigation.ShowPopup(new CrashPopup(exitCode, App.Args.Get("box-id")));
        }

        bool macOSFileExists = File.Exists(AppdataFolderManager.GetPath("crash_report"))
                               && OperatingSystem.IsMacOS();

        if (App.Args.Contains("crash") || macOSFileExists)
        {
            string crashReportFilename = macOSFileExists
                ? await File.ReadAllTextAsync(AppdataFolderManager.GetPath("crash_report"))
                : App.Args.Get("crash");

            if (macOSFileExists) File.Delete(AppdataFolderManager.GetPath("crash_report"));

            await new CrashWindow(await File.ReadAllTextAsync(crashReportFilename)).ShowDialog(this);
        }

        MinecraftAuthenticationResult? authResult = await AuthenticationManager.TryLoginAsync();

        if (authResult != null && authResult.IsSuccess)
        {
            if (!authResult.Validate())
                try
                {
                    // Try to re-login with the Microsoft token
                    authResult = await AuthenticationManager.TryLoginAsync();

                    if (authResult == null)
                    {
                        MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
                        return;
                    }

                    Debug.WriteLine("Successfully relogged-in");
                }
                catch (Exception e)
                {
                    MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
                    return;
                }

            AuthenticationManager.SetAccount(authResult);

            if (!await AuthenticationManager.HasMinecraftAsync())
            {
                MainWindowDataContext.Instance.Push(
                    new ErrorPage(
                        "This account does not own Minecraft. This launcher only supports paid Minecraft accounts"),
                    false);

                await AuthenticationManager.DisconnectAsync();
                return;
            }

            MainWindowDataContext.Instance.Push<MainPage>();

            if (!Settings.SeenVersionsList.Contains(CurrentBuild.Version.ToString()))
                Navigation.ShowPopup(new LauncherUpdatedPopup());
        }
        else
        {
            MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
        }

        // Check for updates
        if (await UpdateManager.IsUpdateAvailableAsync())
        {
            UpdateBar.IsVisible = true;
            UpdateBar.SetUpdateDetails(await GitHubRepository.GetLatestReleaseAsync());
        }

        string? filename = App.Args.GetOrDefault(0);
        if (filename != null && File.Exists(filename)) await BoxImportUtilities.ImportAsync(filename);
    }
}