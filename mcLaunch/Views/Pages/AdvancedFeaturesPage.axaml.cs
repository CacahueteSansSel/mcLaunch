using System;
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
using mcLaunch.Views.Windows;

namespace mcLaunch.Views.Pages;

public partial class AdvancedFeaturesPage : UserControl, ITopLevelPageControl
{
    public AdvancedFeaturesPage()
    {
        InitializeComponent();
    }

    public string Title => "Advanced Features";

    private async void RunExtractMinecraftResources(object? sender, RoutedEventArgs e)
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

        DownloadManager.Begin($"Minecraft {mcVersion.Id}");
        await systemFolder.InstallVersionAsync(mcVersion);
        DownloadManager.End();
        
        await DownloadManager.ProcessAll();
        
        using ZipArchive jar = ZipFile.Open(jarFilename, ZipArchiveMode.Read);

        await Task.Run(() =>
        {
            foreach (Asset asset in index.ParseAll())
            {
                string assetFolderPath = Path.GetDirectoryName(asset.Name)!;
                string finalPath = $"{folder}/{asset.Name}";
                string localFilePath = assetsDownloader.GetAssetLocalPath(asset);

                Directory.CreateDirectory($"{folder}/{assetFolderPath}");
                if (File.Exists(localFilePath))
                    File.Copy(localFilePath, finalPath, true);
            }
        });

        foreach (var entry in jar.Entries.Where(entry => entry.FullName.StartsWith("assets")))
        {
            string entryPath = entry.FullName.Replace("assets/", "").Trim();
            string assetFolderPath = Path.GetDirectoryName(entryPath)!;
            
            Directory.CreateDirectory($"{folder}/{assetFolderPath}");

            await using Stream stream = entry.Open();
            await using FileStream fs = new($"{folder}/{entryPath}", FileMode.Create);

            await stream.CopyToAsync(fs);
        }
        
        PlatformSpecific.OpenFolder(folder);
    }
}