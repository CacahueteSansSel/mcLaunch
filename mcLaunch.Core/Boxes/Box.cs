using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using DynamicData;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Mods.Platforms;
using Modrinth.Exceptions;
using SharpNBT;
using AuthenticationManager = mcLaunch.Core.Managers.AuthenticationManager;

namespace mcLaunch.Core.Boxes;

public class Box
{
    private string manifestPath;
    private bool exposeLauncher = false;
    private string launcherVersion = "0.0.0";
    FileSystemWatcher watcher;
    public bool UseDedicatedGraphics { get; set; }
    public string Path { get; }
    public MinecraftFolder Folder { get; }
    public Minecraft Minecraft { get; private set; }
    public Process MinecraftProcess { get; }
    public MinecraftVersion Version { get; private set; }
    public MinecraftOptions Options { get; private set; }
    public QuickPlayManager QuickPlay { get; private set; }
    public BoxManifest Manifest { get; private set; }
    public ModLoaderSupport? ModLoader => ModLoaderManager.Get(Manifest.ModLoaderId);
    public Version MinecraftVersion => new(Manifest.Version);
    public bool SupportsQuickPlay => MinecraftVersion >= new Version("1.20");
    public IBoxEventListener? EventListener { get; set; }

    public bool IsRunning => MinecraftProcess != null && !MinecraftProcess.HasExited;
    public bool HasReadmeFile => File.Exists($"{Folder.Path}/README.md");
    public bool HasLicenseFile => File.Exists($"{Folder.Path}/LICENSE.md");

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
        {
            Options = new MinecraftOptions($"{Folder.CompletePath}/options.txt");
        }

        QuickPlay = new QuickPlayManager(Folder);
    }

    public Box(string path)
    {
        Path = path;
        manifestPath = $"{path}/box.json";

        Folder = new MinecraftFolder($"{path}/minecraft");
        CreateWatcher();

        if (File.Exists($"{Folder.CompletePath}/options.txt"))
        {
            Options = new MinecraftOptions($"{Folder.CompletePath}/options.txt");
        }

        QuickPlay = new QuickPlayManager(Folder);

        ReloadManifest(true);
        if (File.Exists($"{path}/icon.png") && Manifest.Icon == null)
            LoadIcon();
    }

    void CreateWatcher()
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
        string relativePath = e.FullPath.Replace(Folder.CompletePath, "")
            .Trim('\\').Trim('/')
            .Replace('\\', '/');

        if (!relativePath.StartsWith("mods")) return;
        // A mod was removed

        foreach (BoxStoredModification mod in Manifest.Modifications)
        {
            if (!mod.Filenames.Contains(relativePath)) continue;

            Manifest.RemoveModification(mod.Id, this);
            EventListener?.OnModRemoved(mod.Id);

            break;
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        string relativePath = e.FullPath.Replace(Folder.CompletePath, "")
            .Trim('\\').Trim('/')
            .Replace('\\', '/');

        if (!relativePath.StartsWith("mods")) return;
        // A mod was added

        try
        {
            await using MemoryStream fs = new MemoryStream(await File.ReadAllBytesAsync(e.FullPath));
            ModVersion? version = await ModPlatformManager.Platform.GetModVersionFromData(fs);
            if (version == null || version.Mod == null) return;

            // Add the mod to the list
            Manifest.AddModification(version.Mod.Id, version.VersionId,
                version.Mod.ModPlatformId, new[] { relativePath });

            EventListener?.OnModAdded(version.Mod);
        }
        catch (Exception exception)
        {
            Console.Write(exception);
        }
    }

    async void LoadIcon()
    {
        try
        {
            Manifest.Icon = await IconCollection.FromFileAsync($"{Path}/icon.png", 155);
        }
        catch (Exception e)
        {
            // TODO: Set the manifest icon to default
        }
    }

    public void SetWatching(bool isWatching)
    {
        watcher.EnableRaisingEvents = isWatching;
    }

    public string? ReadReadmeFile() => HasReadmeFile ? File.ReadAllText($"{Folder.Path}/README.md") : null;

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
        List<BoxStoredModification> modsToRemove = new();

        ReloadManifest();

        foreach (BoxStoredModification mod in Manifest.Modifications)
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

        Manifest.Modifications.RemoveMany(modsToRemove);

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
            ModVersion? version = await ModPlatformManager.Platform.GetModVersionFromData(fs);
            if (version == null || version.Mod == null) continue;

            // Add the mod to the list
            Manifest.AddModification(version.Mod.Id, version.VersionId,
                version.Mod.ModPlatformId, new[] { modFilename });

            save = true;
        }

        return save;
    }

    async void RunPostDeserializationChecks()
    {
        if (await Manifest?.RunPostDeserializationChecks())
            SaveManifest();
    }

    async Task SetupVersionAsync()
    {
        if (Version != null) return;
        Version = await Manifest.Setup();
    }

    public async Task<bool> UpdateModAsync(Modification mod, bool installOptional = false)
    {
        string[] versions =
            await ModPlatformManager.Platform.GetModVersionList(mod.Id,
                Manifest.ModLoaderId,
                Manifest.Version);

        if (versions.Length == 0) return false;

        Manifest.RemoveModification(mod.Id, this);

        string version = versions[0];

        PaginatedResponse<ModPlatform.ModDependency> deps = await ModPlatformManager.Platform.GetModDependenciesAsync(
            mod.Id,
            Manifest.ModLoaderId, version, Manifest.Version);

        foreach (ModPlatform.ModDependency dep in deps.Items)
        {
            if (!Manifest.HasModificationStrict(dep.Mod.Id, mod.ModPlatformId)
                && Manifest.HasModificationSoft(dep.Mod))
            {
                // The dependency is installed from another platform
                continue;
            }

            if (Manifest.HasModificationStrict(dep.Mod.Id, mod.ModPlatformId)
                && Manifest.GetModification(dep.Mod.Id).VersionId != dep.VersionId)
            {
                // The mod is installed on this box & the required version does not match the installed one

                string versionId = dep.VersionId ?? dep.Mod.LatestVersion;

                try
                {
                    await ModPlatformManager.Platform.InstallModAsync(this, dep.Mod, versionId, false);
                }
                catch (ModrinthApiException e)
                {
                    if (e.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // It seems that this version doesn't work anymore, so we ignore it
                        continue;
                    }

                    throw;
                }
            }
        }

        return await ModPlatformManager.Platform.InstallModAsync(this, mod, version, installOptional);
    }

    public MinecraftWorld[] LoadWorlds()
    {
        if (!Directory.Exists($"{Folder.Path}/saves")) return Array.Empty<MinecraftWorld>();

        List<MinecraftWorld> worlds = new();

        foreach (string folder in Directory.GetDirectories($"{Folder.Path}/saves"))
        {
            try
            {
                worlds.Add(new MinecraftWorld(System.IO.Path.GetFullPath(folder)));
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        return worlds.ToArray();
    }

    public MinecraftServer[] LoadServers()
    {
        if (!File.Exists($"{Folder.Path}/servers.dat")) return Array.Empty<MinecraftServer>();

        List<MinecraftServer> servers = new();
        CompoundTag tag = NbtFile.Read($"{Folder.Path}/servers.dat", FormatOptions.Java);
        ListTag serversTag = (ListTag)tag["servers"];

        foreach (CompoundTag server in serversTag)
        {
            try
            {
                servers.Add(new MinecraftServer(server));
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        return servers.ToArray();
    }

    public string[] GetScreenshotPaths()
    {
        return !Directory.Exists($"{Folder.Path}/screenshots")
            ? Array.Empty<string>()
            : Directory.GetFiles($"{Folder.Path}/screenshots", "*.png");
    }

    public async Task CreateMinecraftAsync()
    {
        if (Minecraft != null) return;

        await SetupVersionAsync();

        Minecraft = new Minecraft(Version, Folder)
            .WithSystemFolder(BoxManager.SystemFolder)
            .WithUseDedicatedGraphics(UseDedicatedGraphics)
            .WithCustomLauncherDetails("mcLaunch", launcherVersion, exposeLauncher)
            .WithUser(AuthenticationManager.Account!, AuthenticationManager.Platform!)
            .WithDownloaders(BoxManager.AssetsDownloader, BoxManager.LibrariesDownloader, BoxManager.JVMDownloader);
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
            if (Manifest.Modifications.FirstOrDefault(mod => mod.Filenames.Contains(absPath)) != null)
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

    public async Task PrepareAsync()
    {
        await SetupVersionAsync();
        await CreateMinecraftAsync();

        await BoxManager.SetupVersionAsync(Version);
    }

    public async Task<Modification[]> MigrateToModrinthAsync(Action<BoxStoredModification, int, int> statusCallback)
    {
        int cur = 0;
        List<Modification> migratedMods = new();

        BoxStoredModification[] modsToMigrate = Manifest.Modifications.Where(m =>
            m.PlatformId.ToLower() != "modrinth").ToArray();

        foreach (BoxStoredModification mod in modsToMigrate)
        {
            statusCallback?.Invoke(mod, cur, Manifest.Modifications.Count);
            cur++;

            foreach (string filename in mod.Filenames)
            {
                string realFilename = $"{Folder.Path}/{filename}";

                await using FileStream fs = new FileStream(realFilename, FileMode.Open);

                ModVersion? modVersion = await ModrinthModPlatform.Instance.GetModVersionFromData(fs);
                if (modVersion == null) continue;

                Manifest.RemoveModification(mod.Id, this);
                bool success = await ModrinthModPlatform.Instance.InstallModAsync(this, modVersion.Mod,
                    modVersion.VersionId, false);

                if (success) migratedMods.Add(modVersion.Mod);
            }
        }

        return migratedMods.ToArray();
    }

    public async Task<Modification[]> MigrateToCurseForgeAsync(Action<BoxStoredModification, int, int> statusCallback)
    {
        int cur = 0;
        List<Modification> migratedMods = new();

        BoxStoredModification[] modsToMigrate = Manifest.Modifications.Where(m =>
            m.PlatformId.ToLower() != "curseforge").ToArray();

        foreach (BoxStoredModification mod in modsToMigrate)
        {
            statusCallback?.Invoke(mod, cur, Manifest.Modifications.Count);
            cur++;

            foreach (string filename in mod.Filenames)
            {
                string realFilename = $"{Folder.Path}/{filename}";

                await using FileStream fs = new FileStream(realFilename, FileMode.Open);

                ModVersion? modVersion = await CurseForgeModPlatform.Instance.GetModVersionFromData(fs);
                if (modVersion == null) continue;

                Manifest.RemoveModification(mod.Id, this);
                bool success = await CurseForgeModPlatform.Instance.InstallModAsync(this, modVersion.Mod,
                    modVersion.VersionId, false);

                if (success) migratedMods.Add(modVersion.Mod);
            }
        }

        return migratedMods.ToArray();
    }

    public bool HasModification(Modification mod)
        => Manifest.HasModificationStrict(mod.Id, mod.Platform.Name);

    public bool HasModificationSoft(Modification mod)
        => Manifest.HasModificationSoft(mod);

    public void SaveManifest()
        => File.WriteAllText(manifestPath, JsonSerializer.Serialize(Manifest));

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
            (QuickPlayGameMode)world.GameMode, world.FolderName);

        return Minecraft
            .WithSingleplayerQuickPlay(profilePath, world.FolderName)
            .Run();
    }
}