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
using mcLaunch.Core.Contents;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;
using Modrinth.Models;

namespace mcLaunch.Views.Pages;

public partial class ContentDetailsPage : UserControl, ITopLevelPageControl
{
    bool isInstalling = false;
    public MinecraftContent ShownContent { get; private set; }
    public Box? TargetBox { get; private set; }

    public string Title => $"({ShownContent.Type}) {ShownContent.Name} from {ShownContent.Author} " +
                           $"on {ShownContent.ModPlatformId}";

    public ContentDetailsPage()
    {
        InitializeComponent();

        SetInstalled(false);
        LoadCircle.IsVisible = false;
        LoadingButtonFrame.IsVisible = false;
    }

    public ContentDetailsPage(MinecraftContent shownContent, Box? targetBox)
    {
        InitializeComponent();

        using StreamReader rd = new(AssetLoader.Open(new Uri("avares://mcLaunch/resources/html/base.css")));
        DescHtmlPanel.BaseStylesheet = rd.ReadToEnd();

        ShownContent = shownContent;
        TargetBox = targetBox;
        DataContext = shownContent;

        ModPlatformBadge.Text = shownContent.Platform.Name;
        ModPlatformBadge.Icon =
            new Bitmap(
                AssetLoader.Open(
                    new Uri($"avares://mcLaunch/resources/icons/{shownContent.Platform.Name.ToLower()}.png")));
        if (shownContent.License != null)
        {
            ModLicenseBadge.Text = shownContent.GetLicenseDisplayName();
            ModLicenseBadge.IsVisible = true;
        }
        else ModLicenseBadge.IsVisible = false;

        ModOpenSource.IsVisible = shownContent.IsOpenSource;
        ModClosedSource.IsVisible = !shownContent.IsOpenSource;

        SetInstalled(targetBox != null && targetBox.HasContentSoft(shownContent));
        FetchAdditionalInfos();

        UpdateButton.IsEnabled = shownContent.IsUpdateRequired;
        UpdateButton.IsVisible = shownContent.IsUpdateRequired;

        OpenInBrowserButton.IsVisible = shownContent.Url != null;

        if (TargetBox == null)
        {
            InstallButton.IsVisible = false;
            UpdateButton.IsVisible = false;
            UninstallButton.IsVisible = false;

            TestButton.IsVisible = true;
        }
        else TestButton.IsVisible = false;

        DefaultBackground.IsVisible = shownContent.BackgroundPath == null;
    }

    async void FetchAdditionalInfos()
    {
        LoadCircle.IsVisible = true;

        await ShownContent.DownloadBackgroundAsync();
        await ModPlatformManager.Platform.DownloadContentInfosAsync(ShownContent);

        if (ShownContent.Background == null && TargetBox != null)
            ShownContent.Background = TargetBox.Manifest.Background;

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
        ContentVersion version = (ContentVersion) incomingVersion;

        LoadingButtonFrame.IsVisible = true;

        PaginatedResponse<MinecraftContentPlatform.ContentDependency> deps =
            await ModPlatformManager.Platform.GetContentDependenciesAsync(
                ShownContent.Id,
                TargetBox.Manifest.ModLoaderId, version.Id, TargetBox.Manifest.Version);

        MinecraftContentPlatform.ContentDependency[] optionalDeps =
            deps.Items.Where(dep =>
                    dep.Type == MinecraftContentPlatform.DependencyRelationType.Optional &&
                    !TargetBox.HasContentSoft(dep.Content))
                .ToArray();

        if (optionalDeps.Length > 0)
        {
            Navigation.ShowPopup(new OptionalModsPopup(TargetBox, ShownContent, optionalDeps, async () =>
            {
                await ModPlatformManager.Platform.InstallContentAsync(TargetBox, ShownContent, version.Id, true);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);

                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }, async () =>
            {
                await ModPlatformManager.Platform.InstallContentAsync(TargetBox, ShownContent, version.Id, false);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);

                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }));

            return;
        }

        await ModPlatformManager.Platform.InstallContentAsync(TargetBox, ShownContent, version.Id, false);

        LoadingButtonFrame.IsVisible = false;
        SetInstalled(true);

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

        ContentVersion[] versions =
            await ModPlatformManager.Platform.GetContentVersionsAsync(ShownContent,
                TargetBox.Manifest.ModLoaderId,
                TargetBox.Manifest.Version);

        if (versions.Length == 0)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Installation failed",
                $"Unable to install {ShownContent.Name} : no compatible version found " +
                $"for Minecraft {TargetBox.Manifest.Version} or {TargetBox.Manifest.ModLoaderId}"));

            LoadingButtonFrame.IsVisible = false;
            SetInstalled(false);
            return;
        }

        LoadingButtonFrame.IsVisible = false;

        Navigation.ShowPopup(new VersionSelectionPopup(new MinecraftContentVersionProvider(versions, ShownContent),
            CreateModpackFromVersion));
    }

    private async void UninstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (TargetBox == null) return;

        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        TargetBox.Manifest.RemoveContent(ShownContent.Id, TargetBox);

        TargetBox.SaveManifest();

        BoxDetailsPage.LastOpened?.Reload();

        SetInstalled(false);
    }

    async Task FinishInstallAsync()
    {
        if (TargetBox == null) return;

        bool success = await TargetBox.UpdateModAsync(ShownContent, false);

        if (!success)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Installation failed",
                $"Unable to install {ShownContent.Name} : no compatible version found " +
                $"for Minecraft {TargetBox.Manifest.Version} or {TargetBox.Manifest.ModLoaderId}"));

            LoadingButtonFrame.IsVisible = false;
            SetInstalled(false);
            return;
        }

        ShownContent.IsUpdateRequired = false;

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

        if (!string.IsNullOrWhiteSpace(ShownContent.Changelog))
        {
            Navigation.ShowPopup(new ChangelogPopup(ShownContent, async () => { await FinishInstallAsync(); }, () =>
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
        PlatformSpecific.OpenUrl(ShownContent.Url);
    }

    async void CreateFastLaunchModpackFromVersion(IVersion incomingVersion)
    {
        Navigation.ShowPopup(new StatusPopup("Preparing launch",
            $"Preparing launching Fast Launch box for {ShownContent.Name}..."));
        StatusPopup.Instance.ShowDownloadBanner = true;

        ContentVersion version = (ContentVersion) incomingVersion;
        ModLoaderSupport? modLoader = ModLoaderManager.Get(version.ModLoader);
        ModLoaderVersion? modLoaderVersion = await modLoader!.FetchLatestVersion(version.MinecraftVersion);
        ManifestMinecraftVersion minecraftVersion = await MinecraftManager.GetManifestAsync(version.MinecraftVersion);

        BoxManifest manifest = new BoxManifest(ShownContent.Name ?? "Unnamed", string.Empty, string.Empty,
            version.ModLoader, modLoaderVersion!.Name, null, minecraftVersion, BoxType.Temporary);
        string path = await BoxManager.Create(manifest);
        Box box = new Box(path);
        box.SetAndSaveIcon(new Bitmap(AssetLoader.Open(
            new Uri("avares://mcLaunch/resources/fastlaunch_box_logo.png"))));

        await ModPlatformManager.Platform.InstallContentAsync(box, ShownContent, version.Id, false);

        StatusPopup.Instance.ShowDownloadBanner = false;
        Navigation.HidePopup();

        BoxDetailsPage detailsPage = new BoxDetailsPage(box);
        await detailsPage.RunAsync();
    }

    private async void TestButtonClicked(object? sender, RoutedEventArgs e)
    {
        ContentVersion[] versions = await ShownContent.Platform.GetContentVersionsAsync(ShownContent, null, null);

        Navigation.ShowPopup(new VersionSelectionPopup(new MinecraftContentVersionProvider(versions, ShownContent),
            CreateFastLaunchModpackFromVersion));
    }
}