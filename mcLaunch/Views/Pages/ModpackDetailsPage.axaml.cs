using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Core;
using mcLaunch.Launchsite.Core;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages;

public partial class ModpackDetailsPage : UserControl, ITopLevelPageControl
{
    private readonly PlatformModpack modpack;

    public ModpackDetailsPage()
    {
        InitializeComponent();
    }

    public ModpackDetailsPage(PlatformModpack modpack)
    {
        InitializeComponent();

        using StreamReader rd = new(AssetLoader.Open(new Uri("avares://mcLaunch/resources/html/base.css")));
        DescHtmlPanel.BaseStylesheet = rd.ReadToEnd();

        this.modpack = modpack;
        DataContext = modpack;
        OpenInBrowserButton.IsVisible = modpack.Url != null;
        DefaultBackground.IsVisible = this.modpack.BackgroundPath == null;

        string[] supportedMinecraftVersions = this.modpack.FetchAndSortSupportedMinecraftVersions();
        if (supportedMinecraftVersions.Length > 0)
        {
            bool firstAndLastAreSame = supportedMinecraftVersions.Length == 1 ||
                                       supportedMinecraftVersions[0] == supportedMinecraftVersions.Last();
            ModpackVersionsBadge.Text = firstAndLastAreSame
                ? $"{supportedMinecraftVersions[0]}"
                : $"{supportedMinecraftVersions[0]} - {supportedMinecraftVersions.Last()}";
        }
        else
            ModpackVersionsBadge.IsVisible = false;

        string[] supportedModloaders = this.modpack.FetchModLoaders();
        ModpackPlatformBadge.Text = string.Join(", ", supportedModloaders);

        ModpackPlatformBadge.IsVisible = supportedModloaders.Length > 0;
    }

    public string Title => $"{modpack.Name} by {modpack.Author} on {modpack.ModPlatformId}";

    private void CloneButtonClicked(object? sender, RoutedEventArgs e)
    {
        CloneButton.IsVisible = false;

        Navigation.ShowPopup(new VersionSelectionPopup(modpack, CreateModpack));
    }

    private async void CreateModpack(IVersion version)
    {
        if (version is not PlatformModpack.ModpackVersion modpackVersion) return;

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Cloning {modpack.Name}", "Please wait for the modpack to be cloned"));
        StatusPopup.Instance.Status = "Downloading...";
        StatusPopup.Instance.ShowDownloadBanner = true;

        Result<Box> boxResult = await BoxManager.CreateFromPlatformModpack(modpack, modpackVersion, (msg, percent) =>
        {
            StatusPopup.Instance.Status = $"{msg}";
            StatusPopup.Instance.StatusPercent = percent;
        });
        if (boxResult.IsError)
        {
            boxResult.ShowErrorPopup();
            return;
        }

        Box box = boxResult.Data!;
        StatusPopup.Instance.ShowDownloadBanner = false;

        Navigation.HidePopup();

        Navigation.Push(new BoxDetailsPage(box));
        await MainPage.Instance.PopulateBoxListAsync();
    }

    private void OpenButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenUrl(modpack.Url);
    }

    private void UpButtonClicked(object? sender, RoutedEventArgs e)
    {
        ScrollArea.Offset = Vector.Zero;
    }
}