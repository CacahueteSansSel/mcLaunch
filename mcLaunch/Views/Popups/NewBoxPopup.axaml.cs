using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.DataContexts;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Windows;

namespace mcLaunch.Views.Popups;

public partial class NewBoxPopup : UserControl, IMinecraftVersionSelectionListener
{
    INewBoxPopupListener? listener;
    
    public NewBoxPopup(INewBoxPopupListener? listener = null)
    {
        InitializeComponent();
        DataContext = new MinecraftVersionSelectionDataContext();

        this.listener = listener;
        if (this.listener != null && this.listener.CustomShownText != null)
        {
            ((MinecraftVersionSelectionDataContext) DataContext!).CustomText = this.listener.CustomShownText;
            CustomText.IsVisible = true;
        }

        Random rng = new Random();

        Bitmap bmp =
            new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 5)}.png")));
        BoxIconImage.Source = bmp;

        if (AuthenticationManager.Account != null) AuthorNameTb.Text = AuthenticationManager.Account.Username;

        VersionSelector.Listener = this;
        VersionSelector.OnVersionChanged += version => { FetchModLoadersLatestVersions(version.Id); };

        FetchModLoadersLatestVersions(VersionSelector.Version.Id);
    }

    private async void FetchModLoadersLatestVersions(string versionId)
    {
        if (listener != null) await listener.InitializeAsync();
        
        CreateButton.IsEnabled = false;
        MinecraftVersionSelectionDataContext ctx = (MinecraftVersionSelectionDataContext) DataContext!;
        List<ModLoaderSupport> all = [];

        foreach (ModLoaderSupport ml in ModLoaderManager.All
                     .Where(m => Settings.Instance.EnableAdvancedModLoaders || !m.IsAdvanced))
        {
            if (listener != null && !listener.ShouldShowModLoader(ml)) continue;
            
            ModLoaderVersion? version = await ml.FetchLatestVersion(versionId);
            if (version != null) all.Add(ml);
        }

        ctx.ModLoaders = all.Select(m => new DataContextModLoader(m)).ToArray();
        ctx.SelectedModLoader = ctx.ModLoaders[0];
        CreateButton.IsEnabled = true;
    }

    private async void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (listener != null) 
            await listener.WhenCancelledAsync();
        
        Navigation.HidePopup();
    }

    private async void CreateBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BoxNameTb.Text) || string.IsNullOrWhiteSpace(AuthorNameTb.Text))
            return;

        string boxName = BoxNameTb.Text;
        string boxAuthor = AuthorNameTb.Text;
        ManifestMinecraftVersion minecraftVersion = VersionSelector.Version;
        ModLoaderSupport modloader = ((MinecraftVersionSelectionDataContext) DataContext).SelectedModLoader.ModLoader;

        if (string.IsNullOrWhiteSpace(boxName)
            || string.IsNullOrWhiteSpace(boxAuthor)
            || minecraftVersion == null)
            return;

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Creating {boxName}", "We are creating the box... Please wait..."));

        IconCollection icon;
        if (BoxIconImage.Source is Bitmap bitmap)
        {
            icon = await IconCollection.FromBitmapAsync(bitmap);
        }
        else
        {
            Random rng = new Random(BoxNameTb.Text.GetHashCode());
            icon = IconCollection.FromResources($"box_icons/{rng.Next(0, 5)}.png");
        }

        // We fetch automatically the latest version of the modloader for now
        // TODO: Allow the user to select a specific modloader version
        ModLoaderVersion[]? modloaderVersions = await modloader.GetVersionsAsync(minecraftVersion.Id);

        if (modloaderVersions == null || modloaderVersions.Length == 0)
        {
            Navigation.HidePopup();
            Navigation.ShowPopup(new MessageBoxPopup("Failed to initialize the mod loader",
                $"Failed to get any version of {modloader.Name} for Minecraft {minecraftVersion.Id}", MessageStatus.Error));
            return;
        }

        BoxManifest newBoxManifest = new BoxManifest(boxName, null, boxAuthor, modloader.Id, modloaderVersions[0].Name,
            icon, minecraftVersion);

        StatusPopup.Instance.ShowDownloadBanner = true;

        Result<string> result = await BoxManager.Create(newBoxManifest);
        if (result.IsError)
        {
            Navigation.HidePopup();
            Navigation.ShowPopup(new MessageBoxPopup("Failed to create the box", 
                result.ErrorMessage ?? "No details specified", MessageStatus.Error));
            return;
        }

    await MainPage.Instance?.PopulateBoxListAsync();

        Box box = new Box(newBoxManifest, result.Data!);
        
        if (listener == null || await listener.WhenBoxCreatedAsync(box)) 
            Navigation.Push(new BoxDetailsPage(box));

        StatusPopup.Instance.ShowDownloadBanner = false;
        Navigation.HidePopup();
    }

    private async void SelectFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select the icon image...";
        ofd.Filters = new List<FileDialogFilter>
        {
            new()
            {
                Extensions = new List<string>
                {
                    "png"
                },
                Name = "PNG Image"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null || files.Length == 0) return;

        BoxIconImage.Source = new Bitmap(files[0]);
    }

    private void BoxNameTextChanged(object? sender, KeyEventArgs e)
    {
        Random rng = new Random((BoxNameTb.Text ?? string.Empty).GetHashCode());

        Bitmap bmp =
            new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 5)}.png")));

        BoxIconImage.Source = bmp;
    }

    public bool ShouldShowMinecraftVersion(ManifestMinecraftVersion version)
    {
        if (listener != null)
            return listener.ShouldShowMinecraftVersion(version);

        return true;
    }
}

public interface INewBoxPopupListener
{
    string? CustomShownText { get; }

    Task InitializeAsync();
    bool ShouldShowModLoader(ModLoaderSupport modLoader);
    bool ShouldShowMinecraftVersion(ManifestMinecraftVersion version);
    Task<bool> WhenBoxCreatedAsync(Box box);
    Task WhenCancelledAsync();
}