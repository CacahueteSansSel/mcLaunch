using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages;

public partial class ModpackDetailsPage : UserControl
{
    private PlatformModpack modpack;
    
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

        string[] supportedMinecraftVersions = this.modpack.FetchAndSortSupportedMinecraftVersions();
        if (supportedMinecraftVersions.Length > 0)
        {
            bool firstAndLastAreSame = supportedMinecraftVersions.Length == 1 ||
                                       supportedMinecraftVersions[0] == supportedMinecraftVersions.Last();
            ModpackVersionsBadge.Text = firstAndLastAreSame
                ? $"{supportedMinecraftVersions[0]}"
                : $"{supportedMinecraftVersions[0]} - {supportedMinecraftVersions.Last()}";
        }
        else ModpackVersionsBadge.IsVisible = false;

        string[] supportedModloaders = this.modpack.FetchModLoaders();
        ModpackPlatformBadge.Text = string.Join(", ", supportedModloaders);

        ModpackPlatformBadge.IsVisible = supportedModloaders.Length > 0;
    }

    private void CloneButtonClicked(object? sender, RoutedEventArgs e)
    {
        CloneButton.IsVisible = false;
        
        Navigation.ShowPopup(new ModpackVersionSelectionPopup(modpack, CreateModpack));
    }

    private async void CreateModpack(PlatformModpack.ModpackVersion version)
    {
        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Cloning {modpack.Name}", "Please wait for the modpack to be cloned"));
        StatusPopup.Instance.Status = "Downloading...";
        StatusPopup.Instance.ShowDownloadBanner = true;

        Box box = await BoxManager.CreateFromPlatformModpack(modpack, version, (msg, percent) =>
        {
            StatusPopup.Instance.Status = $"{msg}";
            StatusPopup.Instance.StatusPercent = percent;
        });
        
        StatusPopup.Instance.ShowDownloadBanner = false;
        
        Navigation.HidePopup();

        Navigation.Push(new BoxDetailsPage(box));
        MainPage.Instance.PopulateBoxList();
    }

    private void OpenButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenUrl(modpack.Url);
    }
}