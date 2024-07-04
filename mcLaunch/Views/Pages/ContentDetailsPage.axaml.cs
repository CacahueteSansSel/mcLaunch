using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages;

public partial class ContentDetailsPage : UserControl, ITopLevelPageControl
{
    private bool isInstalling;

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

        switch (shownContent.Type)
        {
            case MinecraftContentType.Modification:
                InstallButton.Content = "Install";
                UninstallButton.Content = "Uninstall";
                break;
            case MinecraftContentType.ResourcePack:
            case MinecraftContentType.ShaderPack:
            case MinecraftContentType.World:
                InstallButton.Content = "Download";
                UninstallButton.Content = "Remove";
                break;
            case MinecraftContentType.DataPack:
                InstallButton.Content = "Download on all worlds";
                InstallButton.Width = 250;
                UninstallButton.Content = "Remove from all worlds";
                UninstallButton.Width = 250;
                break;
        }

        ModPlatformBadge.Text = shownContent.Platform.Name;
        //ModPlatformBadge.SetIcon(shownContent.Platform.Name.ToLower());
        
        if (shownContent.License != null)
        {
            ModLicenseBadge.Text = shownContent.GetLicenseDisplayName();
            ModLicenseBadge.IsVisible = true;
        }
        else
        {
            ModLicenseBadge.IsVisible = false;
        }

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
        else
        {
            TestButton.IsVisible = false;
        }

        DefaultBackground.IsVisible = shownContent.BackgroundPath == null;
    }

    public MinecraftContent ShownContent { get; }
    public Box? TargetBox { get; }

    public string Title => $"({ShownContent.Type}) {ShownContent.Name} from {ShownContent.Author} " +
                           $"on {ShownContent.ModPlatformId}";

    private async void FetchAdditionalInfos()
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

    private async void InstallContentFromVersion(IVersion incomingVersion)
    {
        ContentVersion version = (ContentVersion) incomingVersion;

        LoadingButtonFrame.IsVisible = true;

        PaginatedResponse<MinecraftContentPlatform.ContentDependency> deps =
            await ModPlatformManager.Platform.GetContentDependenciesAsync(
                ShownContent.Id,
                ShownContent.Type == MinecraftContentType.Modification
                    ? TargetBox.Manifest.ModLoaderId
                    : null,
                version.Id, TargetBox.Manifest.Version);

        MinecraftContentPlatform.ContentDependency[] optionalDeps =
            deps.Items.Where(dep =>
                    dep.Type == MinecraftContentPlatform.DependencyRelationType.Optional &&
                    !TargetBox.HasContentSoft(dep.Content))
                .ToArray();

        if (optionalDeps.Length > 0)
        {
            Navigation.ShowPopup(new OptionalModsPopup(TargetBox, ShownContent, optionalDeps, async (mods) =>
            {
                await DownloadManager.WaitForPendingProcesses();
                
                foreach (MinecraftContentPlatform.ContentDependency dependency in mods)
                {
                    string? versionId = dependency.VersionId;

                    if (versionId == null)
                    {
                        // If no version is provided, take the latest one

                        ContentVersion[] versions = await ModPlatformManager.Platform.GetContentVersionsAsync(dependency.Content,
                            ShownContent.Type == MinecraftContentType.Modification
                                ? TargetBox.Manifest.ModLoaderId
                                : null, TargetBox.Manifest.Version);
                        
                        // No version is available, so we continue
                        if (versions.Length == 0) continue;

                        versionId = versions[0].Id;
                    }
                    
                    await ModPlatformManager.Platform.InstallContentAsync(TargetBox, 
                        dependency.Content, 
                        versionId, 
                        true, 
                        false);
                }
                
                await ModPlatformManager.Platform.InstallContentAsync(TargetBox, ShownContent, version.Id, false, false);

                await DownloadManager.ProcessAll();
                
                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);

                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }, async () =>
            {
                await DownloadManager.WaitForPendingProcesses();
                
                await ModPlatformManager.Platform.InstallContentAsync(TargetBox, ShownContent, version.Id, false, true);

                LoadingButtonFrame.IsVisible = false;
                SetInstalled(true);

                BoxDetailsPage.LastOpened?.Reload();

                isInstalling = false;
            }));

            return;
        }
        
        await DownloadManager.WaitForPendingProcesses();

        if (!await ModPlatformManager.Platform.InstallContentAsync(TargetBox, ShownContent,
                version.Id, false, true))
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error",
                $"{ShownContent.Name} failed to download : the content may lack any download url", MessageStatus.Error));

            LoadingButtonFrame.IsVisible = false;
            SetInstalled(false);
            isInstalling = false;

            return;
        }

        LoadingButtonFrame.IsVisible = false;
        SetInstalled(true);

        isInstalling = false;
    }

    private async void InstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (TargetBox == null) return;

        if (TargetBox != null && !TargetBox.HasWorlds && ShownContent.Type == MinecraftContentType.DataPack)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Cannot download datapack",
                "You need to have at least one world to download a datapack", MessageStatus.Error));

            return;
        }

        isInstalling = true;

        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        LoadingButtonFrame.IsVisible = true;

        ContentVersion[] versions =
            await ModPlatformManager.Platform.GetContentVersionsAsync(ShownContent,
                ShownContent.Type == MinecraftContentType.Modification
                    ? TargetBox.Manifest.ModLoaderId
                    : null,
                TargetBox.Manifest.Version);

        if (ShownContent.Type == MinecraftContentType.DataPack)
            versions = versions.Where(version => version.ModLoader == "datapack").ToArray();

        if (versions.Length == 0)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Installation failed",
                $"Unable to install {ShownContent.Name} : no compatible version found " +
                $"for Minecraft {TargetBox.Manifest.Version} or {TargetBox.Manifest.ModLoaderId}", MessageStatus.Error));

            LoadingButtonFrame.IsVisible = false;
            SetInstalled(false);
            return;
        }

        LoadingButtonFrame.IsVisible = false;

        Navigation.ShowPopup(new VersionSelectionPopup(new MinecraftContentVersionProvider(versions, ShownContent),
            InstallContentFromVersion));
    }

    private async void UninstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (TargetBox == null) return;

        InstallButton.IsVisible = false;
        InstallButton.IsEnabled = false;

        UninstallButton.IsVisible = false;
        UninstallButton.IsEnabled = false;

        Result result = TargetBox.Manifest.RemoveContent(ShownContent.Id, TargetBox);
        if (result.IsError)
        {
            result.ShowErrorPopup();
            SetInstalled(true);
            
            return;
        }

        TargetBox.SaveManifest();

        BoxDetailsPage.LastOpened?.Reload();

        SetInstalled(false);
    }

    private async Task FinishInstallAsync()
    {
        if (TargetBox == null) return;

        bool success = await TargetBox.UpdateModAsync(ShownContent);

        if (!success)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Installation failed",
                $"Unable to install {ShownContent.Name} : no compatible version found " +
                $"for Minecraft {TargetBox.Manifest.Version} or {TargetBox.Manifest.ModLoaderId}", MessageStatus.Error));

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

    private async void CreateFastLaunchModpackFromVersion(IVersion incomingVersion)
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
        Result<string> pathResult = await BoxManager.Create(manifest);
        if (pathResult.IsError)
        {
            pathResult.ShowErrorPopup();
            return;
        }

        string path = pathResult.Data!;

        Box box = new Box(path);
        box.SetAndSaveIcon(new Bitmap(AssetLoader.Open(
            new Uri("avares://mcLaunch/resources/fastlaunch_box_logo.png"))));

        await ModPlatformManager.Platform.InstallContentAsync(box, ShownContent, version.Id, false, true);

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