using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;
using mcLaunch.Views.Windows;

namespace mcLaunch.Views.Pages;

public partial class AdvancedFeaturesPage : UserControl, ITopLevelPageControl
{
    public AdvancedFeaturesPage()
    {
        InitializeComponent();
    }

    public string Title => "Advanced Features";

    async void RunExtractMinecraftResources(object? sender, RoutedEventArgs e)
    {
        VersionSelectWindow versionSelectWindow = new();

        ManifestMinecraftVersion? version 
            = await versionSelectWindow.ShowDialog<ManifestMinecraftVersion?>(MainWindow.Instance);
        MinecraftFolder systemFolder = BoxManager.SystemFolder;
        AssetsDownloader assetsDownloader = new AssetsDownloader(systemFolder);

        var folders = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select target folder"
        });
        string folder = folders.First().Path.LocalPath;
        
        MinecraftVersion? mcVersion = await version.GetAsync();
        AssetIndex index = await mcVersion.GetAssetIndexAsync();
        string jarFilename = $"{systemFolder.GetVersionPath(mcVersion)}/{mcVersion.Id}.jar";
        
        Navigation.ShowPopup(new StatusPopup($"Extracting Minecraft {mcVersion.Id}", "Please wait for the resources to be extracted"));
        StatusPopup.Instance.ShowDownloadBanner = true;
        StatusPopup.Instance.Status = $"Downloading Minecraft {mcVersion.Id} (1/3)...";
        
        DownloadManager.Begin($"Minecraft {mcVersion.Id}");
        await systemFolder.InstallVersionAsync(mcVersion);
        DownloadManager.End();
        
        await DownloadManager.ProcessAll();
        
        using ZipArchive jar = ZipFile.Open(jarFilename, ZipArchiveMode.Read);

        StatusPopup.Instance.ShowDownloadBanner = false;
        StatusPopup.Instance.Status = $"Copying external assets (2/3)...";
        
        await Task.Run(() =>
        {
            Asset[] assets = index.ParseAll();
            int count = 0;
            foreach (Asset asset in assets)
            {
                string assetFolderPath = Path.GetDirectoryName(asset.Name)!;
                string finalPath = $"{folder}/{asset.Name}";
                string localFilePath = assetsDownloader.GetAssetLocalPath(asset);

                Directory.CreateDirectory($"{folder}/{assetFolderPath}");
                if (File.Exists(localFilePath))
                    File.Copy(localFilePath, finalPath, true);

                count++;
                StatusPopup.Instance.StatusPercent = (float)count / assets.Length / 2f;
            }
        });

        StatusPopup.Instance.Status = $"Copying internal jar assets (3/3)...";
        ZipArchiveEntry[] jarEntries = jar.Entries.Where(entry => entry.FullName.StartsWith("assets")).ToArray();
        int count = 0;
        
        foreach (var entry in jarEntries)
        {
            string entryPath = entry.FullName.Replace("assets/", "").Trim();
            string assetFolderPath = Path.GetDirectoryName(entryPath)!;
            
            Directory.CreateDirectory($"{folder}/{assetFolderPath}");

            await using Stream stream = entry.Open();
            await using FileStream fs = new($"{folder}/{entryPath}", FileMode.Create);

            await stream.CopyToAsync(fs);

            count++;
            StatusPopup.Instance.StatusPercent = 0.5f + (float)count / jarEntries.Length / 2f;
        }
        
        Navigation.HidePopup();
        
        PlatformSpecific.OpenFolder(folder);
    }
}