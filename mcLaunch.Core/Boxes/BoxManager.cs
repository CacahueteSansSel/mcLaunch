using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Media.Imaging;
using Avalonia.Utilities;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Core.Boxes;

public static class BoxManager
{
    public static string BoxesPath => AppdataFolderManager.GetValidPath("boxes");
    public static MinecraftFolder SystemFolder { get; } = new(AppdataFolderManager.GetValidPath("system"));

    public static AssetsDownloader AssetsDownloader { get; } = new(SystemFolder);

    public static LibrariesDownloader LibrariesDownloader { get; } = new(SystemFolder);

    public static JvmDownloader JvmDownloader { get; } = new(SystemFolder);

    public static int BoxCount => Directory.GetDirectories(BoxesPath).Length;

    public static async Task<Box[]> LoadLocalBoxesAsync(bool includeTemp = false, bool runChecks = true)
    {
        if (!Directory.Exists(BoxesPath))
        {
            Directory.CreateDirectory(BoxesPath);
            return Array.Empty<Box>();
        }

        List<Box> boxes = new();

        foreach (string boxPath in Directory.GetDirectories(BoxesPath))
        {
            // Don't load invalid boxes
            if (!File.Exists($"{boxPath}/box.json")) continue;

            Box box = new Box(boxPath, false);
            await box.ReloadManifestAsync(true, runChecks);

            if (box.Manifest.Type == BoxType.Temporary && !includeTemp) continue;

            boxes.Add(box);
        }

        return boxes.ToArray();
    }

    private static async Task PostProcessBoxAsync(Box box, BoxManifest manifest)
    {
        switch (manifest.ModLoaderId)
        {
            case "fabric":
                // Install Fabric API automatically from Modrinth

                try
                {
                    MinecraftContent fabricApi =
                        await ModrinthMinecraftContentPlatform.Instance.GetContentAsync("P7dR8mSH");

                    ContentVersion[] versions = await ModrinthMinecraftContentPlatform.Instance.GetContentVersionsAsync(
                        fabricApi, "fabric", manifest.Version);

                    await ModrinthMinecraftContentPlatform.Instance.InstallContentAsync(box, fabricApi, versions[0].Id,
                        false, true);
                }
                catch (Exception)
                {
                    // ignored
                }

                break;
        }
    }

    public static async Task<Result<string>> Create(BoxManifest manifest)
    {
        string path = $"{BoxesPath}/{manifest.Id}";
        Directory.CreateDirectory(path);

        await File.WriteAllTextAsync($"{path}/box.json", JsonSerializer.Serialize(manifest));

        if (manifest.Icon != null && manifest.Icon.IconLarge != null && manifest.Icon.IconSmall != null)
        {
            Bitmap? icon = manifest.Icon.IconLarge ?? manifest.Icon.IconSmall;
            await Task.Run(() => icon!.Save($"{path}/icon.png"));

            manifest.Icon = await IconCollection.FromFileAsync($"{path}/icon.png");
        }

        var result = await manifest.Setup();
        if (result.IsError) return new Result<string>(result);

        Directory.CreateDirectory($"{path}/minecraft");

        MinecraftOptions? defaultOptions = DefaultsManager.LoadDefaultMinecraftOptions();
        if (defaultOptions != null) defaultOptions.SaveTo($"{path}/minecraft/options.txt");

        await PostProcessBoxAsync(new Box(path), manifest);

        return new Result<string>(path);
    }

    public static async Task<Result<Box>> CreateFromModificationPack(ModificationPack pack, string authorFallback,
        Action<string, float> progressCallback)
    {
        BoxManifest manifest = new BoxManifest(pack.Name, pack.Description ?? "no description",
            string.IsNullOrWhiteSpace(pack.Author) ? authorFallback : pack.Author,
            pack.ModloaderId, pack.ModloaderVersion, null,
            await MinecraftManager.GetManifestAsync(pack.MinecraftVersion));

        if (pack.Id != null) manifest.Id = pack.Id;

        Result<string> path = await Create(manifest);
        if (path.IsError) return new Result<Box>(path);

        Box box = new Box(manifest, path.Data!, false);

        progressCallback?.Invoke($"Preparing Minecraft {pack.MinecraftVersion} ({pack.ModloaderId.Capitalize()})", 0f);

        await box.CreateMinecraftAsync();

        int index = 0;

        foreach (var mod in pack.Modifications)
        {
            progressCallback?.Invoke($"Looking up modification {index}/{pack.Modifications.Length}",
                MathUtilities.Clamp((float) index / pack.Modifications.Length, 0f, 1f) * (1f/3f));

            await pack.InstallModificationAsync(box, mod);

            index++;
        }
        
        progressCallback?.Invoke("Downloading modifications",
            1f / 3f);

        await DownloadManager.ProcessAll();
        
        progressCallback?.Invoke("Extracting files",
            2f / 3f);

        Regex driveLetterRegex = new Regex("[A-Z]:[\\/\\\\]");

        index = 0;
        foreach (var additionalFile in pack.AdditionalFiles)
        {
            if (additionalFile.Path.EndsWith('/') || additionalFile.Path.Contains("..")
                                                  || driveLetterRegex.IsMatch(additionalFile.Path)) continue;

            progressCallback?.Invoke("Extracting files",
                (2f/3f) + MathUtilities.Clamp((float) index / pack.AdditionalFiles.Length, 0f, 1f) * (1f/3f));

            string filename = $"{box.Folder.Path}/{additionalFile.Path}";
            string folderPath = filename.Replace(Path.GetFileName(filename), "");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            await File.WriteAllBytesAsync(filename, additionalFile.Data);
            index++;
        }

        await box.SaveManifestAsync();

        return new Result<Box>(new Box(manifest, path.Data!, false));
    }

    public static async Task<Result<Box>> CreateFromPlatformModpack(PlatformModpack pack,
        PlatformModpack.ModpackVersion version,
        Action<string, float> progressCallback)
    {
        // TODO: maybe use the modpack's own extension (.mrpack for Modrinth) instead of .zip
        string modpackTempFilename = Path.GetFullPath($"temp/{pack.Id}.zip");

        DownloadManager.Begin($"{pack.Name} ({version.Name})");
        DownloadManager.Add(version.ModpackFileUrl!, modpackTempFilename, version.ModpackFileHash,
            EntryAction.Download);
        DownloadManager.End();

        void ProgressUpdate(string status, float percent, int sectionIndex)
        {
            progressCallback?.Invoke($"Downloading modpack ({Path.GetFileName(status)})", percent / 2);
        }

        DownloadManager.OnDownloadProgressUpdate += ProgressUpdate;

        await DownloadManager.ProcessAll();

        DownloadManager.OnDownloadProgressUpdate -= ProgressUpdate;

        progressCallback?.Invoke("Initializing Minecraft", 0.5f);
            
        ModificationPack? modpack;

        try
        {
            modpack = await pack.Platform.LoadModpackFileAsync(modpackTempFilename);
            if (modpack == null) return Result<Box>.Error("Unable to find the modpack");
        }
        catch (Exception e)
        {
            return Result<Box>.Error($"The downloaded archive is not valid (error: {e.Message})");
        }

        Result<Box> boxResult = await CreateFromModificationPack(modpack, pack.Author,
            (status, percent) => { progressCallback?.Invoke(status, 0.5f + percent); });
        if (boxResult.IsError) return boxResult;

        Box box = boxResult.Data!;

        await using Stream? iconStream = pack.IconPath == null
            ? null
            : await DownloadManager.DownloadFileAsync(pack.IconPath);
        await using Stream? backgroundStream = pack.BackgroundPath == null
            ? null
            : await DownloadManager.DownloadFileAsync(pack.BackgroundPath);

        if (iconStream != null) box.SetAndSaveIcon(iconStream, false);
        if (backgroundStream != null) box.SetAndSaveBackground(backgroundStream, false);

        await box.SaveManifestAsync();

        return boxResult;
    }

    public static async Task SetupVersionAsync(MinecraftVersion version, string? customName = null,
        bool downloadAllAfter = true)
    {
        if (!JvmDownloader.HasJvm(Launchsite.Core.Utilities.GetJavaPlatformIdentifier(),
                version.JavaVersion?.Component ?? "jre-legacy"))
        {
            DownloadManager.Begin(customName ?? $"Java {version.JavaVersion.MajorVersion}");
            await JvmDownloader.DownloadForCurrentPlatformAsync(version.JavaVersion.Component);
            DownloadManager.End();
        }

        DownloadManager.Begin(customName ?? $"Minecraft {version.Id}");

        await SystemFolder.InstallVersionAsync(version);
        await AssetsDownloader.DownloadAsync(version, null);
        await LibrariesDownloader.DownloadAsync(version, null);

        DownloadManager.End();

        if (downloadAllAfter) await DownloadManager.ProcessAll();
    }
}