using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using Modrinth.Models;

namespace ddLaunch.Views.Pages;

public partial class ModDetailsPage : UserControl
{
    public Modification Mod { get; private set; }
    public Box TargetBox { get; private set; }

    public ModDetailsPage()
    {
        InitializeComponent();

        SetInstalled(false);
        LoadCircle.IsVisible = false;
    }

    public ModDetailsPage(Modification mod, Box targetBox)
    {
        InitializeComponent();

        Mod = mod;
        TargetBox = targetBox;
        DataContext = mod;

        SetInstalled(targetBox.HasModification(mod));
        GetModAdditionalInfos();
    }

    async void GetModAdditionalInfos()
    {
        LoadCircle.IsVisible = true;

        await ModPlatformManager.Platform.DownloadAdditionalInfosAsync(Mod);

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
        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        // TODO: Version selection

        string[] versions =
            await ModPlatformManager.Platform.GetVersionsForMinecraftVersionAsync(Mod.Id,
                TargetBox.Manifest.ModLoaderId,
                TargetBox.Manifest.Version);

        await ModPlatformManager.Platform.InstallModificationAsync(TargetBox, Mod, versions[0]);

        SetInstalled(true);
    }
}