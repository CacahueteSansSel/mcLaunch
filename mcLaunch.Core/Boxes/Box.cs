using System.Diagnostics;
using System.Formats.Tar;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using DynamicData;
using JetBrains.Annotations;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Core.Utilities;
using Modrinth.Exceptions;
using SharpNBT;
using AuthenticationManager = mcLaunch.Core.Managers.AuthenticationManager;

namespace mcLaunch.Core.Boxes;

public class Box : IEquatable<Box>
{
    private bool exposeLauncher;
    private string launcherVersion = "0.0.0";
    private readonly string manifestPath;
    private FileSystemWatcher watcher;

    public Box(BoxManifest manifest, string path, bool createMinecraft = true)
    {
        Path = path;
        Manifest = manifest;
        manifestPath = $"{path}/box.json";

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        Folder = new MinecraftFolder($"{path}/minecraft");
        if (createMinecraft) CreateMinecraftAsync();
        CreateWatcher();

        if (File.Exists($"{Folder.CompletePath}/options.txt"))
            Options = new MinecraftOptions($"{Folder.CompletePath}/options.txt");

        QuickPlay = new QuickPlayManager(Folder);
    }

    public Box(string path)
    {
        Path = path;
        manifestPath = $"{path}/box.json";

        Folder = new MinecraftFolder($"{path}/minecraft");
        CreateWatcher();

        if (File.Exists($"{Folder.CompletePath}/options.txt"))
            Options = new MinecraftOptions($"{Folder.CompletePath}/options.txt");

        QuickPlay = new QuickPlayManager(Folder);

        ReloadManifest(true);
        if (File.Exists($"{path}/icon.png") && Manifest.Icon == null)
            LoadIcon();
    }

    public bool UseDedicatedGraphics { get; set; }
    public string Path { get; }
    public MinecraftFolder Folder { get; }
    public Minecraft Minecraft { get; private set; }
    public Process MinecraftProcess { get; }
    public MinecraftVersion Version { get; private set; }
    public MinecraftOptions? Options { get; private set; }
    public QuickPlayManager QuickPlay { get; }
    public BoxManifest Manifest { get; private set; }
    public ModLoaderSupport? ModLoader => ModLoaderManager.Get(Manifest.ModLoaderId);
    public Version MinecraftVersion => new(Manifest.Version);
    public bool SupportsQuickPlay => MinecraftVersion >= new Version("1.20");
    public IBoxEventListener? EventListener { get; set; }

    public bool IsRunning => MinecraftProcess != null && !MinecraftProcess.HasExited;
    public bool HasReadmeFile => File.Exists($"{Folder.Path}/README.md");
    public bool HasLicenseFile => File.Exists($"{Folder.Path}/LICENSE.md");

    public bool HasCrashReports => Directory.Exists($"{Folder.Path}/crash-reports")
                                   && Directory.GetFiles($"{Folder.Path}/crash-reports").Length > 0;

    public bool HasWorlds => Directory.GetDirectories($"{Folder.Path}/saves").Length > 0;

    public bool Equals(Box? other)
    {
        return other?.Manifest.Id == Manifest.Id;
    }

    private void CreateWatcher()
    {
        if (!Directory.Exists(Folder.CompletePath))
            Directory.CreateDirectory(Folder.CompletePath);

        watcher = new FileSystemWatcher(Folder.CompletePath);
        watcher.IncludeSubdirectories = true;
        watcher.Created += OnFileCreated;
        watcher.Deleted += OnFileDeleted;
    }

    private async void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        if (DownloadManager.IsProcessing) return;

        string relativePath = e.FullPath.Replace(Folder.CompletePath, "")
            .Trim('\\').Trim('/')
            .Replace('\\', '/');

        if (!relativePath.StartsWith("mods")) return;
        // A mod was removed

        try
        {
            foreach (BoxStoredContent mod in Manifest.Content)
            {
                if (!mod.Filenames.Contains(relativePath)) continue;

                Manifest.RemoveContent(mod.Id, this);
                EventListener?.OnContentRemoved(mod.Id);

                break;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (DownloadManager.IsProcessing) return;

        string relativePath = e.FullPath.Replace(Folder.CompletePath, "")
            .Trim('\\').Trim('/')
            .Replace('\\', '/');

        MinecraftContentType contentType;

        if (relativePath.StartsWith("mods")) contentType = MinecraftContentType.Modification;
        else if (relativePath.StartsWith("resourcepacks")) contentType = MinecraftContentType.ResourcePack;
        else if (relativePath.StartsWith("shaderpacks")) contentType = MinecraftContentType.ShaderPack;
        else if (relativePath.StartsWith("datapacks")) contentType = MinecraftContentType.DataPack;
        else return;

        try
        {
            await using MemoryStream fs = new MemoryStream(await File.ReadAllBytesAsync(e.FullPath));
            ContentVersion? version = await ModPlatformManager.Platform.GetContentVersionFromData(fs);
            if (version == null || version.Content == null) return;

            // Add the mod to the list
            Manifest.AddContent(version.Content.Id, contentType, version.Id,
                version.Content.ModPlatformId, [relativePath]);

            EventListener?.OnContentAdded(version.Content);
        }
        catch (Exception exception)
        {
            Console.Write(exception);
        }
    }

    private async void LoadIcon()
    {
        try
        {
            Manifest.Icon = await IconCollection.FromFileAsync($"{Path}/icon.png");
        }
        catch (Exception e)
        {
            // TODO: Set the manifest icon to default
        }
    }

    public bool HasBackup(string name)
    {
        return Manifest.Backups.Any(backup => backup.Name == name);
    }

    public BoxBackup? GetBackup(string name)
    {
        return Manifest.Backups.FirstOrDefault(backup => backup.Name == name);
    }

    public async Task<BoxBackup?> CreateBackupAsync(string name)
    {
        if (HasBackup(name)) return null;

        string path = $"backups/{name}.tar.gz";
        string fullPath = $"{Path}/{path}";

        Directory.CreateDirectory($"{Path}/backups");

        File.Copy($"{Path}/box.json", $"{Folder.CompletePath}/box.json");
        if (File.Exists($"{Path}/icon.png"))
            File.Copy($"{Path}/icon.png", $"{Folder.CompletePath}/icon.png");
        if (File.Exists($"{Path}/background.png"))
            File.Copy($"{Path}/background.png", $"{Folder.CompletePath}/background.png");

        await TarFile.CreateFromDirectoryAsync(Folder.CompletePath,
            fullPath, false);

        if (File.Exists($"{Folder.CompletePath}/box.json"))
            File.Delete($"{Folder.CompletePath}/box.json");
        if (File.Exists($"{Folder.CompletePath}/icon.png"))
            File.Delete($"{Folder.CompletePath}/icon.png");
        if (File.Exists($"{Folder.CompletePath}/background.png"))
            File.Delete($"{Folder.CompletePath}/background.png");

        BoxBackup backup = new(name, BoxBackupType.Complete, DateTime.Now, path);
        Manifest.Backups.Add(backup);
        SaveManifest();

        return backup;
    }

    public async Task<bool> RestoreBackupAsync(string name)
    {
        BoxBackup? backup = GetBackup(name);
        if (backup == null) return false;

        switch (backup.Type)
        {
            case BoxBackupType.Complete:
                List<BoxBackup> backups = Manifest.Backups;

                string archiveFullPath = $"{Path}/{backup.Filename}";
                if (!File.Exists(archiveFullPath)) return false;

                await TarFile.ExtractToDirectoryAsync(archiveFullPath,
                    Folder.CompletePath, true);

                if (File.Exists($"{Folder.CompletePath}/box.json"))
                {
                    File.Copy($"{Folder.CompletePath}/box.json", $"{Path}/box.json", true);
                    File.Delete($"{Folder.CompletePath}/box.json");
                }

                if (File.Exists($"{Folder.CompletePath}/icon.png"))
                {
                    File.Copy($"{Folder.CompletePath}/icon.png", $"{Path}/icon.png", true);
                    File.Delete($"{Folder.CompletePath}/icon.png");
                }

                if (File.Exists($"{Folder.CompletePath}/background.png"))
                {
                    File.Copy($"{Folder.CompletePath}/background.png", $"{Path}/background.png", true);
                    File.Delete($"{Folder.CompletePath}/background.png");
                }

                // Ensure the backups are still listed even when restoring an earlier backup
                ReloadManifest(true);
                Manifest.Backups = backups;
                SaveManifest();

                // Reload icon and background
                LoadIcon();
                LoadBackground();

                return true;
            case BoxBackupType.Partial:
                break;
        }

        return false;
    }

    public string[] InstallDatapack(string versionId, string filename)
    {
        List<string> paths = new();

        foreach (string worldPath in Directory.GetDirectories($"{Folder.Path}/saves"))
        {
            string datapackFolderPath = $"{worldPath}/datapacks";
            if (!Directory.Exists(datapackFolderPath))
                Directory.CreateDirectory(datapackFolderPath);

            string finalPath = FileSystemUtilities.NormalizePath($"{datapackFolderPath}/" +
                                                                 $"{System.IO.Path.GetFileName(filename)}");

            File.Copy(filename, finalPath, true);
            paths.Add(finalPath.Replace(Folder.CompletePath, "")
                .TrimStart(System.IO.Path.DirectorySeparatorChar));
        }

        BoxStoredContent? content = Manifest.GetContentByVersion(versionId);
        if (content != null) content.Filenames = [..content.Filenames, ..paths.ToArray()];

        return paths.ToArray();
    }

    public bool MatchesQuery(string query)
    {
        string queryLower = query.ToLower();

        return Manifest.Name.ToLower().Contains(queryLower)
               || Manifest.Author.ToLower().Contains(queryLower)
               || Manifest.ModLoaderId.ToLower().Contains(queryLower)
               || Manifest.Version.ToLower().Contains(queryLower);
    }

    public void SetWatching(bool isWatching)
    {
        watcher.EnableRaisingEvents = isWatching;
    }

    public string? ReadReadmeFile()
    {
        return HasReadmeFile ? File.ReadAllText($"{Folder.Path}/README.md") : null;
    }

    public void ReloadManifest(bool force = false)
    {
        // Check if the manifest needs reloading
        SHA1 sha = SHA1.Create();
        string hash = Convert.ToHexString(sha.ComputeHash(File.ReadAllBytes(manifestPath)));
        if (!force && Manifest != null && hash == Manifest.FileHash) return;

        bool isReload = Manifest != null;

        IconCollection icon = null;
        Bitmap? background = null;

        if (isReload)
        {
            // We backup the manifest's icon and background to avoid loading those every time
            icon = Manifest.Icon;
            background = Manifest.Background;
        }

        Manifest = JsonSerializer.Deserialize<BoxManifest>(File.ReadAllText(manifestPath))!;
        RunPostDeserializationChecks();

        Manifest.FileHash = hash;

        if (isReload)
        {
            Manifest.Icon = icon;
            Manifest.Background = background;
        }
    }

    public async Task<string[]> RunIntegrityChecks()
    {
        List<string> changes = new();
        List<BoxStoredContent> modsToRemove = new();

        ReloadManifest();

        foreach (BoxStoredContent mod in Manifest.Content)
        {
            bool exists = false;
            foreach (string filename in mod.Filenames)
            {
                string path = $"{Folder.CompletePath}/{filename}";
                if (!File.Exists(path)) continue;
                exists = true;

                break;
            }

            if (exists) continue;

            modsToRemove.Add(mod);
            changes.Add($"Mod {mod.Name} has been removed because it is not present on disk anymore");
        }

        if (await AddMissingModsToList()) SaveManifest();

        Manifest.Content.RemoveMany(modsToRemove);

        return changes.ToArray();
    }

    public async Task<bool> AddMissingModsToList()
    {
        // Check for unknown mods to add to the list
        List<string> unknownModsFilenames = GetUnlistedMods();
        bool save = false;

        foreach (string modFilename in unknownModsFilenames)
        {
            await using MemoryStream fs =
                new MemoryStream(await File.ReadAllBytesAsync($"{Folder.CompletePath}/{modFilename}"));
            ContentVersion? version = await ModPlatformManager.Platform.GetContentVersionFromData(fs);
            if (version == null || version.Content == null) continue;

            // Add the mod to the list
            Manifest.AddContent(version.Content.Id, MinecraftContentType.Modification, version.Id,
                version.Content.ModPlatformId, new[] {modFilename});

            save = true;
        }

        return save;
    }

    private async void RunPostDeserializationChecks()
    {
        if (await Manifest?.RunPostDeserializationChecks())
            SaveManifest();
    }

    private async Task<Result> SetupVersionAsync()
    {
        if (Version != null) return new Result();
        Result<MinecraftVersion> setupResult = await Manifest.Setup();
        if (setupResult.IsError) return setupResult;

        Version = setupResult.Data!;
        return new Result();
    }

    public async Task<bool> UpdateModAsync(MinecraftContent mod, bool installOptional = false)
    {
        ContentVersion[] versions =
            await ModPlatformManager.Platform.GetContentVersionsAsync(mod,
                mod.Type == MinecraftContentType.Modification ? Manifest.ModLoaderId : null,
                Manifest.Version);

        if (versions.Length == 0) return false;

        Manifest.RemoveContent(mod.Id, this);

        ContentVersion version = versions[0];

        PaginatedResponse<MinecraftContentPlatform.ContentDependency> deps =
            await ModPlatformManager.Platform.GetContentDependenciesAsync(
                mod.Id,
                mod.Type == MinecraftContentType.Modification ? Manifest.ModLoaderId : null,
                version.Id, Manifest.Version);

        foreach (MinecraftContentPlatform.ContentDependency dep in deps.Items)
        {
            if (dep?.Content == null || dep.Type == MinecraftContentPlatform.DependencyRelationType.Incompatible)
                continue;

            if (!Manifest.HasContentStrict(dep.Content.Id, mod.ModPlatformId)
                && Manifest.HasContentSoft(dep.Content))
                // The dependency is installed from another platform
                continue;

            if (Manifest.HasContentStrict(dep.Content.Id, mod.ModPlatformId)
                && Manifest.GetContent(dep.Content.Id).VersionId != dep.VersionId)
            {
                // The mod is installed on this box & the required version does not match the installed one

                string versionId = dep.VersionId ?? dep.Content.LatestVersion;

                try
                {
                    await ModPlatformManager.Platform.InstallContentAsync(this, dep.Content, versionId, false);
                }
                catch (ModrinthApiException e)
                {
                    if (e.Response.StatusCode == HttpStatusCode.NotFound)
                        // It seems that this version doesn't work anymore, so we ignore it
                        continue;

                    throw;
                }
            }
        }

        return await ModPlatformManager.Platform.InstallContentAsync(this, mod, version.Id, installOptional);
    }

    public MinecraftWorld[] LoadWorlds()
    {
        if (!Directory.Exists($"{Folder.Path}/saves")) return Array.Empty<MinecraftWorld>();

        List<MinecraftWorld> worlds = new();

        foreach (string folder in Directory.GetDirectories($"{Folder.Path}/saves"))
            try
            {
                worlds.Add(new MinecraftWorld(System.IO.Path.GetFullPath(folder)));
            }
            catch (Exception e)
            {
                // ignored
            }

        return worlds.ToArray();
    }

    public MinecraftServer[] LoadServers()
    {
        if (!File.Exists($"{Folder.Path}/servers.dat"))
            return Array.Empty<MinecraftServer>();

        List<MinecraftServer> servers = new();
        CompoundTag tag = NbtFile.Read($"{Folder.Path}/servers.dat", FormatOptions.Java);
        ListTag serversTag = (ListTag) tag["servers"];

        foreach (CompoundTag server in serversTag)
            try
            {
                servers.Add(new MinecraftServer(server));
            }
            catch (Exception e)
            {
                // ignored
            }

        return servers.ToArray();
    }

    public MinecraftCrashReport[] LoadCrashReports()
    {
        if (!Directory.Exists($"{Folder.Path}/crash-reports"))
            return Array.Empty<MinecraftCrashReport>();

        return Directory.GetFiles($"{Folder.Path}/crash-reports")
            .Select(file => new MinecraftCrashReport(file))
            .ToArray();
    }

    public string[] GetScreenshotPaths()
    {
        return !Directory.Exists($"{Folder.Path}/screenshots")
            ? Array.Empty<string>()
            : Directory.GetFiles($"{Folder.Path}/screenshots", "*.png");
    }

    public async Task<Result> CreateMinecraftAsync()
    {
        if (Minecraft != null) return new Result();

        Result result = await SetupVersionAsync();
        if (result.IsError) return result;

        Minecraft = new Minecraft(Version, Folder)
            .WithSystemFolder(BoxManager.SystemFolder)
            .WithUseDedicatedGraphics(UseDedicatedGraphics)
            .WithCustomLauncherDetails("mcLaunch", launcherVersion, exposeLauncher)
            .WithUser(AuthenticationManager.Account!, AuthenticationManager.Platform!)
            .WithDownloaders(BoxManager.AssetsDownloader, BoxManager.LibrariesDownloader, BoxManager.JVMDownloader);

        return new Result();
    }

    public void SetExposeLauncher(bool exposeLauncher)
    {
        this.exposeLauncher = exposeLauncher;
    }

    public void SetLauncherVersion(string version)
    {
        launcherVersion = version;
    }

    public List<string> GetUnlistedMods()
    {
        if (!Directory.Exists($"{Path}/minecraft/mods"))
            return new List<string>();

        List<string> mods = new();

        foreach (string file in Directory.GetFiles($"{Path}/minecraft/mods", "*.jar"))
        {
            string absPath = file.Replace(Path, "").Replace('\\', '/')
                .Replace("minecraft/", "").Replace(Path, "").Trim('/').Trim();
            if (Manifest.Content.FirstOrDefault(mod => mod.Filenames.Contains(absPath)) != null)
                continue;

            mods.Add(absPath);
        }

        return mods;
    }

    public List<string> GetAdditionalFiles()
    {
        List<string> files = new();

        // TODO: Ask the user for the folders/files to export
        string[] inclusions =
        {
            "config",
            "servers.dat",
            "options.txt"
        };

        foreach (string file in Directory.GetFiles($"{Path}/minecraft", "*", SearchOption.AllDirectories))
        {
            string absPath = file.Replace(Path, "").Replace('\\', '/')
                .Replace("minecraft/", "").Replace(Path, "").Trim('/').Trim();
            if (inclusions.Count(ex => absPath.ToLower().StartsWith(ex)) == 0) continue;

            files.Add(absPath);
        }

        return files;
    }

    public async void SetAndSaveIcon(Bitmap icon)
    {
        icon.Save($"{Path}/icon.png");
        Manifest.Icon = await IconCollection.FromBitmapAsync(icon);
    }

    public void SetAndSaveBackground(Bitmap background)
    {
        background.Save($"{Path}/background.png");
        Manifest.Background = background;
    }

    public void LoadBackground()
    {
        if (Manifest.Background != null) return;
        if (!File.Exists($"{Path}/background.png")) return;

        Manifest.Background = new Bitmap($"{Path}/background.png");
    }

    [MustUseReturnValue("Use the return value to catch problems if any")]
    public async Task<Result> PrepareAsync()
    {
        Result result = await SetupVersionAsync();
        if (result.IsError) return result;
        
        result = await CreateMinecraftAsync();
        if (result.IsError) return result;

        await BoxManager.SetupVersionAsync(Version);

        return new Result();
    }

    public async Task<MinecraftContent[]> MigrateToModrinthAsync(Action<BoxStoredContent, int, int> statusCallback)
    {
        int cur = 0;
        List<MinecraftContent> migratedMods = new();

        BoxStoredContent[] modsToMigrate = Manifest.Content.Where(m =>
            m.PlatformId.ToLower() != "modrinth").ToArray();

        foreach (BoxStoredContent mod in modsToMigrate)
        {
            statusCallback?.Invoke(mod, cur, Manifest.Content.Count);
            cur++;

            foreach (string filename in mod.Filenames)
            {
                string realFilename = $"{Folder.Path}/{filename}";

                FileStream fs = new FileStream(realFilename, FileMode.Open);

                ContentVersion? modVersion =
                    await ModrinthMinecraftContentPlatform.Instance.GetContentVersionFromData(fs);
                if (modVersion == null) continue;

                fs.Close();

                Manifest.RemoveContent(mod.Id, this);
                bool success = await ModrinthMinecraftContentPlatform.Instance.InstallContentAsync(this,
                    modVersion.Content,
                    modVersion.Id, false);

                if (success) migratedMods.Add(modVersion.Content);
            }
        }

        return migratedMods.ToArray();
    }

    public async Task<MinecraftContent[]> MigrateToCurseForgeAsync(Action<BoxStoredContent, int, int> statusCallback)
    {
        int cur = 0;
        List<MinecraftContent> migratedMods = new();

        BoxStoredContent[] modsToMigrate = Manifest.Content.Where(m =>
            m.PlatformId.ToLower() != "curseforge").ToArray();

        foreach (BoxStoredContent mod in modsToMigrate)
        {
            statusCallback?.Invoke(mod, cur, Manifest.Content.Count);
            cur++;

            foreach (string filename in mod.Filenames)
            {
                string realFilename = $"{Folder.Path}/{filename}";

                await using FileStream fs = new FileStream(realFilename, FileMode.Open);

                ContentVersion? modVersion =
                    await CurseForgeMinecraftContentPlatform.Instance.GetContentVersionFromData(fs);
                if (modVersion == null) continue;

                Manifest.RemoveContent(mod.Id, this);
                bool success = await CurseForgeMinecraftContentPlatform.Instance.InstallContentAsync(this,
                    modVersion.Content,
                    modVersion.Id, false);

                if (success) migratedMods.Add(modVersion.Content);
            }
        }

        return migratedMods.ToArray();
    }

    public bool HasContentStrict(MinecraftContent mod)
    {
        return Manifest.HasContentStrict(mod.Id, mod.Platform.Name);
    }

    public bool HasContentSoft(MinecraftContent mod)
    {
        return Manifest.HasContentSoft(mod);
    }

    public void SaveManifest()
    {
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(Manifest));
    }

    // Launch Minecraft normally
    public Process Run()
    {
        Manifest.LastLaunchTime = DateTime.Now;
        SaveManifest();

        return Minecraft.Run();
    }

    // Launch Minecraft and directly connect to a server 
    public Process Run(string serverAddress, string serverPort)
    {
        return Minecraft
            .WithServer(serverAddress, serverPort)
            .Run();
    }

    // Launch a Minecraft world directly using QuickPlay
    public Process Run(MinecraftWorld world)
    {
        string profilePath = QuickPlay.Create(QuickPlayWorldType.Singleplayer,
            (QuickPlayGameMode) world.GameMode, world.FolderName);

        return Minecraft
            .WithSingleplayerQuickPlay(profilePath, world.FolderName)
            .Run();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Box);
    }

    public override int GetHashCode()
    {
        return Manifest.Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"Box {Manifest.Id} {Manifest.Name}";
    }
}