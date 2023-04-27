﻿using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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

        SetInstalled(targetBox.HasModificationSoft(mod));
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
                await ModPlatformManager.Platform.InstallModificationAsync(TargetBox, Mod, version, true);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);
        
                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }, async () =>
            {
                await ModPlatformManager.Platform.InstallModificationAsync(TargetBox, Mod, version, false);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);
        
                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }));

            return;
        }

        await ModPlatformManager.Platform.InstallModificationAsync(TargetBox, Mod, version, false);

        LoadingButtonFrame.IsVisible = false;
        SetInstalled(true);
        
        BoxDetailsPage.LastOpened?.Reload();

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
        
        BoxDetailsPage.LastOpened?.Reload();

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

        if (!string.IsNullOrWhiteSpace(Mod.Changelog))
        {
            Navigation.ShowPopup(new ChangelogPopup(Mod, versions[0], async () =>
            {
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
            }, () =>
            {
                UpdateButton.IsVisible = true;
                UpdateButton.IsEnabled = true;

                UninstallButton.IsVisible = true;
                UninstallButton.IsEnabled = true;
            }));

            return;
        }

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