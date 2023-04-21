using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;
using ReactiveUI;

namespace ddLaunch.Views.Popups;

public partial class NewBoxPopup : UserControl
{
    public NewBoxPopup()
    {
        InitializeComponent();
        this.DataContext = new Data();
        
        Random rng = new Random();
            
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        Bitmap bmp = new Bitmap(assets.Open(new Uri($"avares://ddLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));
        BoxIconImage.Source = bmp;

        if (AuthenticationManager.Account != null)
        {
            AuthorNameTb.Text = AuthenticationManager.Account.Username;
        }
    }

    private void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void CreateBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        string boxName = BoxNameTb.Text;
        string boxAuthor = AuthorNameTb.Text;
        ManifestMinecraftVersion minecraftVersion = ((Data) DataContext).SelectedVersion;
        ModLoaderSupport modloader = ((Data) DataContext).SelectedModLoader;

        if (string.IsNullOrWhiteSpace(boxName)
            || string.IsNullOrWhiteSpace(boxAuthor)
            || minecraftVersion == null)
        {
            return;
        }
        
        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Creating {boxName}", "We are creating the box... Please wait..."));

        Bitmap bmp;
        if (BoxIconImage.Source is Bitmap bitmap)
        {
            bmp = bitmap;
        }
        else
        {
            Random rng = new Random(BoxNameTb.Text.GetHashCode());
            
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            bmp = new Bitmap(assets.Open(new Uri($"avares://ddLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));
        }
        
        // We fetch automatically the latest version of the modloader for now
        // TODO: Allow the user to select a specific modloader version
        ModLoaderVersion[]? modloaderVersions = await modloader.GetVersionsAsync(minecraftVersion.Id);

        if (modloaderVersions == null || modloaderVersions.Length == 0)
        {
            Navigation.HidePopup();
            Navigation.ShowPopup(new MessageBoxPopup("Failed to initialize the mod loader", $"Failed to get any version of {modloader.Name} for Minecraft {minecraftVersion.Id}"));
            return;
        }
        
        BoxManifest newBoxManifest = new BoxManifest(boxName, null, boxAuthor, modloader.Id, modloaderVersions[0].Name, bmp, minecraftVersion);
        
        await BoxManager.Create(newBoxManifest);
        
        Navigation.HidePopup();
        
        MainPage.Instance?.PopulateBoxList();
    }

    public class Data : ReactiveObject
    {
        ModLoaderVersion latestVersion;
        ManifestMinecraftVersion selectedVersion;
        ModLoaderSupport selectedModLoader;
        
        public ManifestMinecraftVersion[] Versions { get; }
        public ModLoaderSupport[] ModLoaders { get; }

        public ManifestMinecraftVersion SelectedVersion
        {
            get => selectedVersion;
            set => this.RaiseAndSetIfChanged(ref selectedVersion, value);
        }

        public ModLoaderSupport SelectedModLoader
        {
            get => selectedModLoader;
            set => this.RaiseAndSetIfChanged(ref selectedModLoader, value);
        }

        public ModLoaderVersion LatestVersion
        {
            get => latestVersion;
            set => this.RaiseAndSetIfChanged(ref latestVersion, value);
        }

        public Data()
        {
            Versions = MinecraftManager.ManifestVersions;
            ModLoaders = ModLoaderManager.All.ToArray();

            selectedVersion = Versions[0];
            selectedModLoader = ModLoaders[0];
        }
    }

    private async void SelectFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select the icon image...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
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

    private void NewMinecraftVersionSelectedCallback(object? sender, SelectionChangedEventArgs e)
    {
        
    }

    private void BoxNameTextChanged(object? sender, KeyEventArgs e)
    {
        Random rng = new Random(BoxNameTb.Text.GetHashCode());
            
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        Bitmap bmp = new Bitmap(assets.Open(new Uri($"avares://ddLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));
        
        BoxIconImage.Source = bmp;
    }
}