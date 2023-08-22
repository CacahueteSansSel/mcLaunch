using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Mods.Packs;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages.BoxDetails;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages;

public partial class BoxDetailsPage : UserControl
{
    public static BoxDetailsPage? LastOpened { get; private set; }
    
    public Box Box { get; }
    public SubControl SubControl { get; private set; }

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

        VersionBadge.Text = box.Manifest.Version;
        ModLoaderBadge.Icon = Box.ModLoader?.LoadIcon();
        ModLoaderBadge.Text = box.ModLoader?.Name ?? "Unknown";

        box.LoadBackground();
        
        RunBoxChecks();
    }

    async void RunBoxChecks()
    {
        SubControlButtons.IsEnabled = false;

        string[] changes = await Box.RunIntegrityChecks();
        if (changes.Length > 0)
        {
            Box.SaveManifest();
            
            ShowWarning($"Some changes have been applied to your box: \n{string.Join('\n', changes.Select(c => $"    - {c}"))}");
        }
        
        SubControlButtons.IsEnabled = true;

        if (Box.HasReadmeFile) SetSubControl(new ReadmeSubControl(Box.ReadReadmeFile()));
        else SetSubControl(new ModListSubControl());

        ReadmeButton.IsVisible = Box.HasReadmeFile;
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

    async void PopulateSubControl()
    {
        SubControl.Box = Box;
        SubControl.ParentPage = this;
        
        await SubControl.PopulateAsync();
    }

    public async void Run(string? serverAddress = null, string? serverPort = null)
    {
        if (Box.Manifest.ModLoader == null)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Can't run Minecraft",
                $"The modloader {Box.Manifest.ModLoaderId.Capitalize()} isn't supported"));

            return;
        }

        Navigation.ShowPopup(new GameLaunchPopup());
        
        Box.SetExposeLauncher(Utilities.Settings.Instance.ExposeLauncherNameToMinecraft);
        Box.SetLauncherVersion(BuildStatic.BuildVersion.ToString());

        await Box.PrepareAsync();
        Process java;

        if (serverAddress != null) java = Box.Run(serverAddress, serverPort ?? "25565");
        else java = Box.Run();

        await Task.Delay(500);

        if (java.HasExited)
        {
            Navigation.ShowPopup(new CrashPopup(java.ExitCode, Box.Manifest.Id)
                .WithCustomLog(java.StandardError.ReadToEnd()));
            
            return;
        }

        // TODO: crash report parser
        // RegExp for mod dependencies error (Forge) : /(Failure message): .+/g

        string backgroundProcessFilename
            = Path.GetFullPath("mcLaunch.MinecraftGuard" + (OperatingSystem.IsWindows() ? ".exe" : ""));

        if (File.Exists(backgroundProcessFilename))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = backgroundProcessFilename,
                Arguments = $"{java.Id.ToString()} {Box.Manifest.Id}",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            });
        }

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
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "png"
                },
                Name = "PNG Image"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

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
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select the icon image...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "png"
                },
                Name = "PNG Image"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

        Bitmap iconBitmap = new Bitmap(files[0]);
        Box.SetAndSaveIcon(iconBitmap);
    }

    private void OpenFolderButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenFolder(Box.Path);
    }

    private async void ExportButtonClicked(object? sender, RoutedEventArgs e)
    {
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Title = $"Export {Box.Manifest.Name}";
        sfd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "box"
                },
                Name = "mcLaunch Box Binary File"
            }
        };

        string? filename = await sfd.ShowAsync(MainWindow.Instance);
        if (filename == null) return;

        Navigation.ShowPopup(new StatusPopup($"Exporting {Box.Manifest.Name}",
            "Please wait while we package your box in a file..."));

        BoxBinaryModificationPack bb = new();
        await bb.ExportAsync(Box, filename);

        Navigation.HidePopup();
        Navigation.ShowPopup(new MessageBoxPopup("Success !",
            $"The box {Box.Manifest.Name} have been exported successfully"));
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
                    Navigation.ShowPopup(new MessageBoxPopup("Failed to delete box", $"Failed to delete the box {Box.Manifest.Name} : {exception.Message}"));
                }
            }));
    }

    private void SubControlModButtonClicked(object? sender, RoutedEventArgs e)
    {
        SetSubControl(new ModListSubControl());
    }

    private void SubControlResourcePackButtonClicked(object? sender, RoutedEventArgs e)
    {
        // TODO: Resources packs SubControl
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
}