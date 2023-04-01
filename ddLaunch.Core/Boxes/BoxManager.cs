using System.Text.Json;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using ddLaunch.Core.Utilities;

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

    public static async Task<Box> CreateFromModificationPack(ModificationPack pack, Action<string, float> progressCallback)
    {
        BoxManifest manifest = new BoxManifest(pack.Name, "Imported from a CurseForge modpack", pack.Author,
            pack.ModloaderId, pack.ModloaderVersion, null,
            await MinecraftManager.GetManifestAsync(pack.MinecraftVersion));

        string path = await Create(manifest);

        Box box = new Box(manifest, path, false);

        progressCallback?.Invoke($"Preparing Minecraft {pack.MinecraftVersion} ({pack.ModloaderId.Capitalize()})", 0f);

        await box.CreateMinecraftAsync();

        int index = 0;
        foreach (var mod in pack.Modifications)
        {
            progressCallback?.Invoke($"Installing modification {index}/{pack.Modifications.Length}", 
                (float)index / pack.Modifications.Length / 2);
            
            await pack.InstallModificationAsync(box, mod);

            index++;
        }

        index = 0;
        foreach (var additionalFile in pack.AdditionalFiles)
        {
            progressCallback?.Invoke($"Writing file override {index}/{pack.AdditionalFiles.Length}", 
                0.5f + (float)index / pack.AdditionalFiles.Length / 2);
            
            string filename = $"{box.Folder.Path}/{additionalFile.Path}";
            string folderPath = filename.Replace(Path.GetFileName(filename), "").Trim('/');

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            
            await File.WriteAllBytesAsync(filename, additionalFile.Data);
            index++;
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