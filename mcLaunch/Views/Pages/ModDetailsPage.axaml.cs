using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;
using Modrinth.Models;

namespace mcLaunch.Views.Pages;

public partial class ModDetailsPage : UserControl
{
    bool isInstalling = false;
    public Modification Mod { get; private set; }
    public Box? TargetBox { get; private set; }

    public ModDetailsPage()
    {
        InitializeComponent();

        SetInstalled(false);
        LoadCircle.IsVisible = false;
        LoadingButtonFrame.IsVisible = false;
    }

    public ModDetailsPage(Modification mod, Box? targetBox)
    {
        InitializeComponent();

        Mod = mod;
        TargetBox = targetBox;
        DataContext = mod;

        ModPlatformBadge.Text = mod.Platform.Name;
        ModPlatformBadge.Icon =
            new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{mod.Platform.Name.ToLower()}.png")));

        SetInstalled(targetBox != null && targetBox.HasModificationSoft(mod));
        GetModAdditionalInfos();

        UpdateButton.IsEnabled = mod.IsUpdateRequired;
        UpdateButton.IsVisible = mod.IsUpdateRequired;

        OpenInBrowserButton.IsVisible = mod.Url != null;

        if (TargetBox == null)
        {
            InstallButton.IsVisible = false;
            UpdateButton.IsVisible = false;
            UninstallButton.IsVisible = false;
        }
    }

    async void GetModAdditionalInfos()
    {
        LoadCircle.IsVisible = true;

        await Mod.DownloadBackgroundAsync();
        await ModPlatformManager.Platform.DownloadModInfosAsync(Mod);

        if (Mod.Background == null && TargetBox != null)
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
        if (TargetBox == null) return;
        
        isInstalling = true;

        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        LoadingButtonFrame.IsVisible = true;

        // TODO: Version selection

        string[] versions =
            await ModPlatformManager.Platform.GetModVersionList(Mod.Id,
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

        string version = versions[0];

        ModPlatform.ModDependency[] deps = await ModPlatformManager.Platform.GetModDependenciesAsync(Mod.Id,
            TargetBox.Manifest.ModLoaderId, version, TargetBox.Manifest.Version);

        ModPlatform.ModDependency[] optionalDeps =
            deps.Where(dep =>
                    dep.Type == ModPlatform.DependencyRelationType.Optional && !TargetBox.HasModificationSoft(dep.Mod))
                .ToArray();

        if (optionalDeps.Length > 0)
        {
            Navigation.ShowPopup(new OptionalModsPopup(TargetBox, Mod, optionalDeps, async () =>
            {
                await ModPlatformManager.Platform.InstallModAsync(TargetBox, Mod, version, true);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);
        
                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }, async () =>
            {
                await ModPlatformManager.Platform.InstallModAsync(TargetBox, Mod, version, false);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);
        
                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }));

            return;
        }

        await ModPlatformManager.Platform.InstallModAsync(TargetBox, Mod, version, false);

        LoadingButtonFrame.IsVisible = false;
        SetInstalled(true);
        
        BoxDetailsPage.LastOpened?.Reload();

        isInstalling = false;
    }

    private async void UninstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (TargetBox == null) return;
        
        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        TargetBox.Manifest.RemoveModification(Mod.Id, TargetBox);

        TargetBox.SaveManifest();
        
        BoxDetailsPage.LastOpened?.Reload();

        SetInstalled(false);
    }

    async Task FinishInstallAsync()
    {
        if (TargetBox == null) return;
        
        bool success = await TargetBox.UpdateModAsync(Mod, false);
        
        if (!success)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Installation failed",
                $"Unable to install {Mod.Name} : no compatible version found " +
                $"for Minecraft {TargetBox.Manifest.Version} or {TargetBox.Manifest.ModLoaderId}"));

            LoadingButtonFrame.IsVisible = false;
            SetInstalled(false);
            return;
        }

        Mod.IsUpdateRequired = false;

        UninstallButton.IsVisible = true;
        UninstallButton.IsEnabled = true;
    }

    private async void UpdateButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (TargetBox == null) return;
        
        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        UpdateButton.IsVisible = false;
        UpdateButton.IsEnabled = false;

        if (!string.IsNullOrWhiteSpace(Mod.Changelog))
        {
            Navigation.ShowPopup(new ChangelogPopup(Mod, async () =>
            {
                await FinishInstallAsync();
            }, () =>
            {
                LoadingButtonFrame.IsVisible = false;

                UpdateButton.IsVisible = true;
                UpdateButton.IsEnabled = true;
                
                InstallButton.IsVisible = false;
                InstallButton.IsEnabled = false;
                
                UninstallButton.IsVisible = true;
                UninstallButton.IsEnabled = true;
            }));

            return;
        }
        
        await FinishInstallAsync();
    }

    private void OpenButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenUrl(Mod.Url);
    }
}