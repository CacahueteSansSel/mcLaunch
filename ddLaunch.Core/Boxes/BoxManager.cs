using System.Text.Json;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;

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

    public static async Task<Box> CreateFromModificationPack(ModificationPack pack)
    {
        BoxManifest manifest = new BoxManifest(pack.Name, "Imported from a CurseForge modpack", pack.Author,
            pack.ModloaderId, pack.ModloaderVersion, null,
            await MinecraftManager.GetManifestAsync(pack.MinecraftVersion));

        string path = await Create(manifest);

        Box box = new Box(manifest, path);

        // Wait any download to finish
        await DownloadManager.WaitForPendingDownloads();

        foreach (var mod in pack.Modifications)
        {
            await pack.InstallModificationAsync(box, mod);
        }

        foreach (var additionalFile in pack.AdditionalFiles)
        {
            await File.WriteAllBytesAsync($"{path}/{additionalFile.Path}", additionalFile.Data);
        }
        
        box.SaveManifest();

        return box;
    }

    public static async Task SetupVersionAsync(MinecraftVersion version, string? customName = null,
        bool downloadAllAfter = true)
    {
        DownloadManager.Begin(customName ?? $"Minecraft {version.Id}");

        await systemFolder.InstallVersionAsync(version);
        await assetsDownloader.DownloadAsync(version, null);
        await librariesDownloader.DownloadAsync(version, null);

        DownloadManager.End();

        if (downloadAllAfter) await DownloadManager.DownloadAll();
    }
}