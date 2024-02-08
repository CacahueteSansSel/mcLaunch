using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
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

        using StreamReader rd = new(AssetLoader.Open(new Uri("avares://mcLaunch/resources/html/base.css")));
        DescHtmlPanel.BaseStylesheet = rd.ReadToEnd();

        Mod = mod;
        TargetBox = targetBox;
        DataContext = mod;

        ModPlatformBadge.Text = mod.Platform.Name;
        ModPlatformBadge.Icon =
            new Bitmap(
                AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{mod.Platform.Name.ToLower()}.png")));
        if (mod.License != null)
        {
            ModLicenseBadge.Text = mod.GetLicenseDisplayName();
            ModLicenseBadge.IsVisible = true;
        }
        else ModLicenseBadge.IsVisible = false;

        ModOpenSource.IsVisible = mod.IsOpenSource;
        ModClosedSource.IsVisible = !mod.IsOpenSource;

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

            TestButton.IsVisible = true;
        }
        else TestButton.IsVisible = false;
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

    async void CreateModpackFromVersion(IVersion incomingVersion)
    {
        ModVersion version = (ModVersion)incomingVersion;

        LoadingButtonFrame.IsVisible = true;

        PaginatedResponse<ModPlatform.ModDependency> deps = await ModPlatformManager.Platform.GetModDependenciesAsync(
            Mod.Id,
            TargetBox.Manifest.ModLoaderId, version.Id, TargetBox.Manifest.Version);

        ModPlatform.ModDependency[] optionalDeps =
            deps.Items.Where(dep =>
                    dep.Type == ModPlatform.DependencyRelationType.Optional && !TargetBox.HasModificationSoft(dep.Mod))
                .ToArray();

        if (optionalDeps.Length > 0)
        {
            Navigation.ShowPopup(new OptionalModsPopup(TargetBox, Mod, optionalDeps, async () =>
            {
                await ModPlatformManager.Platform.InstallModAsync(TargetBox, Mod, version.Id, true);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);

                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }, async () =>
            {
                await ModPlatformManager.Platform.InstallModAsync(TargetBox, Mod, version.Id, false);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);

                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }));

            return;
        }

        await ModPlatformManager.Platform.InstallModAsync(TargetBox, Mod, version.Id, false);

        LoadingButtonFrame.IsVisible = false;
        SetInstalled(true);

        BoxDetailsPage.LastOpened?.Reload();

        isInstalling = false;
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

        ModVersion[] versions =
            await ModPlatformManager.Platform.GetModVersionsAsync(Mod,
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

        LoadingButtonFrame.IsVisible = false;

        Navigation.ShowPopup(new VersionSelectionPopup(new ModVersionContentProvider(versions, Mod),
            CreateModpackFromVersion));
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
            Navigation.ShowPopup(new ChangelogPopup(Mod, async () => { await FinishInstallAsync(); }, () =>
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

    async void CreateFastLaunchModpackFromVersion(IVersion incomingVersion)
    {
        Navigation.ShowPopup(new StatusPopup("Preparing launch", 
            $"Preparing launching Fast Launch box for {Mod.Name}..."));
        StatusPopup.Instance.ShowDownloadBanner = true;
        
        ModVersion version = (ModVersion)incomingVersion;
        ModLoaderSupport? modLoader = ModLoaderManager.Get(version.ModLoader);
        ModLoaderVersion? modLoaderVersion = await modLoader!.FetchLatestVersion(version.MinecraftVersion);
        ManifestMinecraftVersion minecraftVersion = await MinecraftManager.GetManifestAsync(version.MinecraftVersion);
        
        BoxManifest manifest = new BoxManifest(Mod.Name ?? "Unnamed", string.Empty, string.Empty, 
            version.ModLoader, modLoaderVersion!.Name, null, minecraftVersion, BoxType.Temporary);
        string path = await BoxManager.Create(manifest);
        Box box = new Box(path);
        box.SetAndSaveIcon(new Bitmap(AssetLoader.Open(
            new Uri("avares://mcLaunch/resources/fastlaunch_box_logo.png"))));
        
        await ModPlatformManager.Platform.InstallModAsync(box, Mod, version.Id, false);
        
        StatusPopup.Instance.ShowDownloadBanner = false;
        Navigation.HidePopup();

        BoxDetailsPage detailsPage = new BoxDetailsPage(box);
        await detailsPage.RunAsync();
    }

    private async void TestButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModVersion[] versions = await Mod.Platform.GetModVersionsAsync(Mod, null, null);

        Navigation.ShowPopup(new VersionSelectionPopup(new ModVersionContentProvider(versions, Mod),
            CreateFastLaunchModpackFromVersion));
    }
}