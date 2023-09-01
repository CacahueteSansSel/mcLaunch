using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Core;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;
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

        if (manifest.Icon != null)
        {
            Bitmap? icon = manifest.Icon.IconLarge ?? manifest.Icon.IconSmall;
            icon!.Save($"{path}/icon.png");
            
            manifest.Icon = await IconCollection.FromFileAsync($"{path}/icon.png");
        }

        if (!Directory.Exists("forge"))
        {
            // Extract the forge wrapper from resources
            // This is a copy of portablemc's old wrapper code for the forge installer, along with license
            // & source code
            // We might want to find a new way for installing Forge in the future
            // TODO: Implement a new way for installing Forge

            await using Stream zipStream =
                AssetLoader.Open(new Uri("avares://mcLaunch/resources/internal/forge_wrapper.zip"));
            await using MemoryStream tmpStream = new();
            string forgeWrapperPath = Path.GetFullPath("forge");

            await zipStream.CopyToAsync(tmpStream);

            using ZipArchive zip = new ZipArchive(tmpStream);
            zip.ExtractToDirectory(forgeWrapperPath, true);
        }

        await manifest.Setup();

        Directory.CreateDirectory($"{path}/minecraft");

        MinecraftOptions? defaultOptions = DefaultsManager.LoadDefaultMinecraftOptions();
        if (defaultOptions != null) defaultOptions.SaveTo($"{path}/minecraft/options.txt");

        await PostProcessBoxAsync(new Box(path), manifest);

        return path;
    }

    public static async Task<Box> CreateFromModificationPack(ModificationPack pack,
        Action<string, float> progressCallback)
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
                (float) index / pack.Modifications.Length / 2);

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
                0.5f + (float) index / pack.AdditionalFiles.Length / 2);

            string filename = $"{box.Folder.Path}/{additionalFile.Path}";
            string folderPath = filename.Replace(Path.GetFileName(filename), "").Trim('/');

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            await File.WriteAllBytesAsync(filename, additionalFile.Data);
            index++;
        }

        box.SaveManifest();

        return new Box(manifest, path, false);
    }

    public static async Task<Box> CreateFromPlatformModpack(PlatformModpack pack,
        PlatformModpack.ModpackVersion version,
        Action<string, float> progressCallback)
    {
        // TODO: maybe use the modpack's own extension (.mrpack for Modrinth) instead of .zip
        string modpackTempFilename = Path.GetFullPath($"temp/{pack.Id}.zip");
        
        DownloadManager.Begin($"{pack.Name} ({version.Name})");
        DownloadManager.Add(version.ModLoaderFileUrl, modpackTempFilename, EntryAction.Download);
        DownloadManager.End();

        void ProgressUpdate(string status, float percent, int sectionIndex)
        {
            progressCallback?.Invoke($"Downloading {status}", percent / 2);
        }
        
        DownloadManager.OnDownloadProgressUpdate += ProgressUpdate;

        await DownloadManager.DownloadAll();
        
        DownloadManager.OnDownloadProgressUpdate -= ProgressUpdate;

        ModificationPack modpack = await pack.Platform.LoadModpackFileAsync(modpackTempFilename);

        Box box = await CreateFromModificationPack(modpack, (status, percent) =>
        {
            progressCallback?.Invoke(status, 0.5f + percent);
        });
        
        if (pack.Icon != null) box.SetAndSaveIcon(pack.Icon);
        if (pack.Background != null) box.SetAndSaveBackground(pack.Background);
        
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