using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using ddLaunch.Utilities;
using ddLaunch.Views.Popups;
using Modrinth.Models;

namespace ddLaunch.Views.Pages;

public partial class ModDetailsPage : UserControl
{
    bool isInstalling = false;
    public Modification Mod { get; private set; }
    public Box TargetBox { get; private set; }

    public ModDetailsPage()
    {
        InitializeComponent();

        SetInstalled(false);
        LoadCircle.IsVisible = false;
        LoadingButtonFrame.IsVisible = false;
    }

    public ModDetailsPage(Modification mod, Box targetBox)
    {
        InitializeComponent();

        Mod = mod;
        TargetBox = targetBox;
        DataContext = mod;

        SetInstalled(targetBox.HasModification(mod));
        GetModAdditionalInfos();

        UpdateButton.IsEnabled = mod.IsUpdateRequired;
        UpdateButton.IsVisible = mod.IsUpdateRequired;
    }

    async void GetModAdditionalInfos()
    {
        LoadCircle.IsVisible = true;

        await Mod.DownloadBackgroundAsync();
        await ModPlatformManager.Platform.DownloadAdditionalInfosAsync(Mod);

        if (Mod.Background == null)
            Mod.Background = TargetBox.Manifest.Background;

        LoadCircle.IsVisible = false;
    }

    public void SetInstalled(bool isInstalled)
    {
        InstallButton.IsVisible = !isInstalled;
        InstallButton.IsEnabled = !isInstalled;

        UninstallButton.IsVisible = isInstalled;
        UninstallButton.IsEnabled = isInstalled;
    }

    private async void InstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        isInstalling = true;
        
        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;
        
        LoadingButtonFrame.IsVisible = true;

        // TODO: Version selection

        string[] versions =
            await ModPlatformManager.Platform.GetVersionsForMinecraftVersionAsync(Mod.Id,
                TargetBox.Manifest.ModLoaderId,
                TargetBox.Manifest.Version);

        if (versions.Length == 0)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Installation failed",
                $"Unable to install {Mod.Name} : no compatible version found " +
                $"for Minecraft {TargetBox.Manifest.Version} or {TargetBox.Manifest.ModLoaderId}"));
            
            LoadingButtonFrame.IsVisible = false;
            SetInstalled(false);
            return;
        }

        await ModPlatformManager.Platform.InstallModificationAsync(TargetBox, Mod, versions[0]);
        
        LoadingButtonFrame.IsVisible = false;
        SetInstalled(true);

        isInstalling = false;
    }

    private async void UninstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        TargetBox.Manifest.RemoveModification(Mod.Id);

        TargetBox.SaveManifest();

        SetInstalled(false);
    }

    private async void UpdateButtonClicked(object? sender, RoutedEventArgs e)
    {
        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        UpdateButton.IsVisible = false;
        UpdateButton.IsEnabled = false;
        
        TargetBox.Manifest.RemoveModification(Mod.Id);
        InstallButtonClicked(sender, e);

        await Task.Run(async () =>
        {
            while (isInstalling)
                await Task.Delay(1);
        });

        Mod.IsUpdateRequired = false;

        UninstallButton.IsVisible = true;
        UninstallButton.IsEnabled = true;
    }
}