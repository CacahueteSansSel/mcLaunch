using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Core.Utilities;
using mcLaunch.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages.BoxDetails;
using mcLaunch.Views.Popups;
using mcLaunch.Views.Windows;
using FileSystemUtilities = mcLaunch.Utilities.FileSystemUtilities;

namespace mcLaunch.Views.Pages;

public partial class BoxDetailsPage : UserControl, ITopLevelPageControl
{
    public BoxDetailsPage()
    {
        InitializeComponent();
    }

    public BoxDetailsPage(Box box)
    {
        LastOpened = this;
        InitializeComponent();

        Box = box;
        DataContext = box;

        Box.SetWatching(true);

        if (box.Manifest.Type == BoxType.Temporary)
        {
            ContentsBox.IsVisible = false;
            return;
        }

        ContentsBox.IsVisible = true;

        MinecraftButtonText.Text = box.Manifest.Version;
        ModloaderButtonLoaderIcon.Source = Box.ModLoader?.LoadIcon();
        ModloaderButtonText.Text = $"{box.ModLoader?.Name ?? "Unknown"} {box.Manifest.ModLoaderVersion}";

        box.LoadBackground();
        DefaultBackground.IsVisible = box.Manifest.Background == null;

        RunBoxChecks();
    }

    public static BoxDetailsPage? LastOpened { get; private set; }

    public Box Box { get; }
    public SubControl SubControl { get; private set; }

    public string Title =>
        $"{Box.Manifest.Name} by {Box.Manifest.Author} on {Box.Manifest.ModLoaderId} {Box.Manifest.Version}";

    private async void RunBoxChecks()
    {
        SubControlButtons.IsEnabled = false;
        LoadingIcon.IsVisible = true;

        string[] changes = await Box.RunIntegrityChecks();
        if (changes.Length > 0)
        {
            Box.SaveManifest();

            ShowWarning(
                $"Some changes have been applied to your box: \n{string.Join('\n', changes.Select(c => $"    - {c}"))}");
        }

        LoadingIcon.IsVisible = false;
        SubControlButtons.IsEnabled = true;

        if (Box.HasReadmeFile) SetSubControl(new ReadmeSubControl(Box.ReadReadmeFile()));
        else SetSubControl(new ContentsSubControl());

        ReadmeButton.IsVisible = Box.HasReadmeFile;
        CrashReportButton.IsVisible = Box.HasCrashReports;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Box?.SetWatching(true);
        Reload();

        DiscordManager.SetPresenceBox(Box);

        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        Box?.SetWatching(false);
        base.OnUnloaded(e);
    }

    public void ShowWarning(string text)
    {
        WarningStripe.IsVisible = true;
        WarningText.Text = text;
    }

    public void HideWarning()
    {
        WarningStripe.IsVisible = false;
    }

    public void SetSubControl(SubControl control)
    {
        SubControl = control;
        SubControlContainer.Children.Clear();
        SubControlContainer.Children.Add(SubControl);

        SubControlTitleText.Text = control.Title;

        PopulateSubControl();
    }

    public void Reload()
    {
        PopulateSubControl();
    }

    private async void PopulateSubControl()
    {
        if (SubControl == null) return;

        SubControl.Box = Box;
        SubControl.ParentPage = this;

        await SubControl.PopulateAsync();
    }

    public async void Run(string? serverAddress = null, string? serverPort = null, MinecraftWorld? world = null)
    {
        await RunAsync(serverAddress, serverPort, world);
    }

    public async Task RunAsync(string? serverAddress = null, string? serverPort = null, MinecraftWorld? world = null)
    {
        if (Box.Manifest.ModLoader == null)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Can't run Minecraft",
                $"The modloader {Box.Manifest.ModLoaderId.Capitalize()} isn't supported"));

            return;
        }

        Navigation.ShowPopup(new GameLaunchPopup());
        DiscordManager.SetPresenceLaunching(Box);

        Box.UseDedicatedGraphics = Utilities.Settings.Instance.ForceDedicatedGraphics;
        Box.SetExposeLauncher(Utilities.Settings.Instance.ExposeLauncherNameToMinecraft);
        Box.SetLauncherVersion(CurrentBuild.Version.ToString());

        Result boxPrepareResult = await Box.PrepareAsync();
        if (boxPrepareResult.IsError)
        {
            boxPrepareResult.ShowErrorPopup();
            return;
        }
        
        Process java;

        if (world != null) java = Box.Run(world);
        else if (serverAddress != null) java = Box.Run(serverAddress, serverPort ?? "25565");
        else java = Box.Run();

        await Task.Delay(1000);

        if (java.HasExited)
        {
            Navigation.ShowPopup(new CrashPopup(java.ExitCode, Box.Manifest.Id)
                .WithCustomLog(java.StartInfo.RedirectStandardError
                    ? java.StandardError.ReadToEnd()
                    : "Minecraft exited in early startup process"));

            return;
        }

        // TODO: crash report parser
        // RegExp for mod dependencies error (Forge) : /(Failure message): .+/g

        if (PlatformSpecific.ProcessExists("mcLaunch.MinecraftGuard"))
            PlatformSpecific.LaunchProcess("mcLaunch.MinecraftGuard",
                $"{java.Id.ToString()} {Box.Manifest.Id} {Box.Manifest.Type.ToString().ToLower()}",
                hidden: true);

        Environment.Exit(0);
    }

    private async void RunButtonClicked(object? sender, RoutedEventArgs e)
    {
        Run();
    }

    private async void EditBackgroundButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select the background image...";
        ofd.Filters = new List<FileDialogFilter>
        {
            new()
            {
                Extensions = new List<string>
                {
                    "png"
                },
                Name = "PNG Image"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null || files.Length == 0) return;

        Bitmap backgroundBitmap = new Bitmap(files[0]);
        Box.SetAndSaveBackground(backgroundBitmap);
    }

    private async void EditButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new EditBoxPopup(Box));
    }

    private void BoxIconCursorEntered(object? sender, PointerEventArgs e)
    {
        EditIconButton.IsVisible = true;
        EditIconButton.IsEnabled = true;
    }

    private void BoxIconCursorLeft(object? sender, PointerEventArgs e)
    {
        EditIconButton.IsVisible = false;
        EditIconButton.IsEnabled = false;
    }

    private async void EditIconButtonClicked(object? sender, RoutedEventArgs e)
    {
        Bitmap[] files = await FileSystemUtilities.PickBitmaps(false, "Select a new icon image");
        if (files.Length == 0) return;

        Bitmap? bmp = files.FirstOrDefault();
        if (bmp != null) Box.SetAndSaveIcon(bmp);
    }

    private void OpenFolderButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenFolder(Box.Path);
    }

    private async void ExportButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ExportBoxPopup(Box));
    }

    private void DeleteBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Delete {Box.Manifest.Name} ?",
            "Everything will be lost (mods, worlds, configs, etc.) and there is no coming back",
            () =>
            {
                try
                {
                    Directory.Delete(Box.Path, true);
                    MainPage.Instance.PopulateBoxList();

                    Navigation.Pop();
                }
                catch (Exception exception)
                {
                    Navigation.ShowPopup(new MessageBoxPopup("Failed to delete box",
                        $"Failed to delete the box {Box.Manifest.Name} : {exception.Message}"));
                }
            }));
    }

    private void SubControlModButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ContentsSubControl(MinecraftContentType.Modification));
    }

    private void SubControlResourcePackButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ContentsSubControl(MinecraftContentType.ResourcePack));
    }

    private void SubControlDatapackButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ContentsSubControl(MinecraftContentType.DataPack, false));
    }

    private void SubControlShaderButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ContentsSubControl(MinecraftContentType.ShaderPack));
    }

    private void SubControlWorldButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new WorldListSubControl());
    }

    private void SubControlScreenshotClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ScreenshotListSubControl());
    }

    private void SubControlServerButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ServerListSubControl());
    }

    private void SubControlSettingsClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new GameSettingsSubControl());
    }

    private void WarningStripeCloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        HideWarning();
    }

    private void SubControlReadmeButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ReadmeSubControl(Box.ReadReadmeFile()));
    }

    private void SubControlCrashReportClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new CrashReportListSubControl());
    }

    private void SubControlBackupsClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new BackupListSubControl());
    }

    private async void MinecraftButtonClicked(object? sender, RoutedEventArgs e)
    {
        VersionSelectWindow selectWindow = new("Select the new Minecraft version");

        ManifestMinecraftVersion? newVersion = await selectWindow
            .ShowDialog<ManifestMinecraftVersion?>(MainWindow.Instance);

        if (newVersion == null) return;

        Navigation.ShowPopup(new ConfirmMessageBoxPopup("Warning",
            "Changing the Minecraft version can break mods, your worlds, and other important things. " +
            "Proceed with caution ! Do you wish to continue ?",
            () =>
            {
                Box.Manifest.Version = newVersion.Id;
                Box.SaveManifest();

                Navigation.Pop();
                Navigation.Push(new BoxDetailsPage(Box));
            }));
    }

    private void MinecraftButtonCursorEntered(object? sender, PointerEventArgs e)
    {
        MinecraftButtonEditIcon.IsVisible = true;
        MinecraftButtonVanillaIcon.IsVisible = false;
    }

    private void MinecraftButtonCursorExited(object? sender, PointerEventArgs e)
    {
        MinecraftButtonEditIcon.IsVisible = false;
        MinecraftButtonVanillaIcon.IsVisible = true;
    }

    private async void ModloaderButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModLoaderSupportWrapper modLoaderSupportWrapper = new(Box.ModLoader);

        await modLoaderSupportWrapper.FetchVersionsAsync(Box.Manifest.Version);

        Navigation.ShowPopup(new VersionSelectionPopup(modLoaderSupportWrapper, version =>
        {
            if (version is not ModLoaderVersionWrapper mlVersion) return;

            Navigation.ShowPopup(new ConfirmMessageBoxPopup("Warning",
                "Changing the mod loader version can break some mods and other important things. " +
                "Proceed with caution ! Do you wish to continue ?",
                () =>
                {
                    Box.Manifest.ModLoaderVersion = mlVersion.Id;
                    Box.SaveManifest();

                    Navigation.Pop();
                    Navigation.Push(new BoxDetailsPage(Box));
                }));
        }));
    }

    private void ModloaderButtonCursorEntered(object? sender, PointerEventArgs e)
    {
        ModloaderButtonEditIcon.IsVisible = true;
        ModloaderButtonLoaderIcon.IsVisible = false;
    }

    private void ModloaderButtonCursorExited(object? sender, PointerEventArgs e)
    {
        ModloaderButtonEditIcon.IsVisible = false;
        ModloaderButtonLoaderIcon.IsVisible = true;
    }
}