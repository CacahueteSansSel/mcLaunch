using System.Text.Json;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;

namespace ddLaunch.Core.Boxes;

public static class BoxManager
{
    static MinecraftFolder systemFolder = new("system");

    static AssetsDownloader assetsDownloader = new(systemFolder);
    static LibrariesDownloader librariesDownloader = new(systemFolder);

    public static string BoxesPath => Path.GetFullPath("boxes");
    public static MinecraftFolder SystemFolder => systemFolder;
    public static AssetsDownloader AssetsDownloader => assetsDownloader;
    public static LibrariesDownloader LibrariesDownloader => librariesDownloader;

    public static Box[] LoadLocalBoxes()
    {
        if (!Directory.Exists(BoxesPath))
        {
            Directory.CreateDirectory(BoxesPath);
            return Array.Empty<Box>();
        }

        List<Box> boxes = new();

        foreach (string boxPath in Directory.GetDirectories(BoxesPath))
        {
            boxes.Add(new Box(boxPath));
        }

        return boxes.ToArray();
    }

    public static async Task<string> Create(BoxManifest manifest)
    {
        string path = $"{BoxesPath}/{manifest.Id}";
        Directory.CreateDirectory(path);
        
        await File.WriteAllTextAsync($"{path}/box.json", JsonSerializer.Serialize(manifest));

        if (manifest.Icon is Bitmap bmp)
        {
            bmp.Save($"{path}/icon.png");
        }

        await manifest.Setup();

        return path;
    }

    public static async Task SetupVersionAsync(MinecraftVersion version, string? customName = null, bool downloadAllAfter = true)
    {
        DownloadManager.Begin(customName ?? $"Minecraft {version.Id}");
        
        await systemFolder.InstallVersionAsync(version);
        await assetsDownloader.DownloadAsync(version, null);
        await librariesDownloader.DownloadAsync(version, null);
        
        DownloadManager.End();
        
        if (downloadAllAfter) await DownloadManager.DownloadAll();
    }
}