﻿using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Mods.Platforms;

namespace mcLaunch.Core.Boxes;

public static class BoxManager
{
    static MinecraftFolder systemFolder = new("system");

    static AssetsDownloader assetsDownloader = new(systemFolder);
    static LibrariesDownloader librariesDownloader = new(systemFolder);
    static JVMDownloader jvmDownloader = new(systemFolder);

    public static string BoxesPath => Path.GetFullPath("boxes");
    public static MinecraftFolder SystemFolder => systemFolder;
    public static AssetsDownloader AssetsDownloader => assetsDownloader;
    public static LibrariesDownloader LibrariesDownloader => librariesDownloader;
    public static JVMDownloader JVMDownloader => jvmDownloader;

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

    static async Task PostProcessBoxAsync(Box box, BoxManifest manifest)
    {
        if (manifest.ModLoaderId == "fabric")
        {
            // Install Fabric API automatically from Modrinth

            try
            {
                string fabricApiId = "P7dR8mSH";
                Modification fabricApi = await ModrinthModPlatform.Instance.GetModAsync(fabricApiId);

                string[] versions = await ModrinthModPlatform.Instance.GetModVersionList(
                    fabricApiId, "fabric", manifest.Version);

                await ModrinthModPlatform.Instance.InstallModAsync(box, fabricApi, versions[0],
                    false);
            }
            catch (Exception e)
            {
                
            }
        }
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

        Directory.CreateDirectory($"{path}/minecraft");

        await PostProcessBoxAsync(new Box(path), manifest);

        return path;
    }

    public static async Task<Box> CreateFromModificationPack(ModificationPack pack, Action<string, float> progressCallback)
    {
        BoxManifest manifest = new BoxManifest(pack.Name, pack.Description ?? "no description", pack.Author,
            pack.ModloaderId, pack.ModloaderVersion, null,
            await MinecraftManager.GetManifestAsync(pack.MinecraftVersion));

        if (pack.Id != null) manifest.Id = pack.Id;

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

        Regex driveLetterRegex = new Regex("[A-Z]:[\\/\\\\]");
        
        index = 0;
        foreach (var additionalFile in pack.AdditionalFiles)
        {
            if (additionalFile.Path.EndsWith('/') || additionalFile.Path.Contains("..")
                || driveLetterRegex.IsMatch(additionalFile.Path)) continue;
            
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
        if (!JVMDownloader.HasJVM(Cacahuete.MinecraftLib.Core.Utilities.GetJavaPlatformIdentifier(), 
                (version.JavaVersion?.Component ?? "jre-legacy")))
        {
            DownloadManager.Begin(customName ?? $"Java {version.JavaVersion.MajorVersion}");
            await JVMDownloader.DownloadForCurrentPlatformAsync(version.JavaVersion.Component);
            DownloadManager.End();
        }
        
        DownloadManager.Begin(customName ?? $"Minecraft {version.Id}");
        
        await systemFolder.InstallVersionAsync(version);
        await assetsDownloader.DownloadAsync(version, null);
        await librariesDownloader.DownloadAsync(version, null);

        DownloadManager.End();

        if (downloadAllAfter) await DownloadManager.DownloadAll();
    }
}