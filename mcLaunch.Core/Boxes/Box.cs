using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Models;
using DynamicData;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Mods.Platforms;
using SharpNBT;

namespace mcLaunch.Core.Boxes;

public class Box
{
    private string manifestPath;
    private bool exposeLauncher = false;
    public string Path { get; }
    public MinecraftFolder Folder { get; }
    public Minecraft Minecraft { get; private set; }
    public Process MinecraftProcess { get; }
    public MinecraftVersion Version { get; private set; }
    public MinecraftOptions Options { get; private set; }
    public BoxManifest Manifest { get; }

    public bool IsRunning => MinecraftProcess != null && !MinecraftProcess.HasExited;

    public Box(BoxManifest manifest, string path, bool createMinecraft = true)
    {
        Path = path;
        Manifest = manifest;
        manifestPath = $"{path}/box.json";

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        Folder = new MinecraftFolder($"{path}/minecraft");
        if (createMinecraft) CreateMinecraftAsync();

        if (File.Exists($"{Folder.CompletePath}/options.txt"))
        {
            Options = new MinecraftOptions($"{Folder.CompletePath}/options.txt");
        }
    }

    public Box(string path)
    {
        Path = path;
        manifestPath = $"{path}/box.json";

        Manifest = JsonSerializer.Deserialize<BoxManifest>(File.ReadAllText(manifestPath))!;
        RunPostDeserializationChecks();

        if (File.Exists($"{path}/icon.png"))
        {
            Manifest.Icon = new Bitmap($"{path}/icon.png");
        }

        Folder = new MinecraftFolder($"{path}/minecraft");

        if (File.Exists($"{Folder.CompletePath}/options.txt"))
        {
            Options = new MinecraftOptions($"{Folder.CompletePath}/options.txt");
        }
    }

    public async Task<string[]> RunIntegrityChecks()
    {
        List<string> changes = new();
        List<BoxStoredModification> modsToRemove = new();

        foreach (BoxStoredModification mod in Manifest.Modifications)
        {
            bool exists = false;
            foreach (string filename in mod.Filenames)
            {
                string path = $"{Folder.CompletePath}/{filename}";

                if (File.Exists(path))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                modsToRemove.Add(mod);
                changes.Add($"Mod {mod.Name} has been removed because it is not present on disk anymore");
            }
        }

        Manifest.Modifications.RemoveMany(modsToRemove);

        return changes.ToArray();
    }

    async void RunPostDeserializationChecks()
    {
        if (await Manifest?.RunPostDeserializationChecks())
        {
            SaveManifest();
        }
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

        ModPlatform.ModDependency[] deps = await ModPlatformManager.Platform.GetModDependenciesAsync(mod.Id,
            Manifest.ModLoaderId, version, Manifest.Version);

        foreach (ModPlatform.ModDependency dep in deps)
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
                
                await ModPlatformManager.Platform.InstallModAsync(this, dep.Mod, dep.VersionId, false);
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
                
            }
        }

        return worlds.ToArray();
    }

    public MinecraftServer[] LoadServers()
    {
        if (!File.Exists($"{Folder.Path}/servers.dat")) return Array.Empty<MinecraftServer>();

        List<MinecraftServer> servers = new();
        CompoundTag tag = NbtFile.Read($"{Folder.Path}/servers.dat", FormatOptions.Java);
        ListTag serversTag = (ListTag) tag["servers"];

        foreach (CompoundTag server in serversTag)
            servers.Add(new MinecraftServer(server));

        return servers.ToArray();
    }

    public string[] GetScreenshotPaths()
    {
        if (!Directory.Exists($"{Folder.Path}/screenshots")) return Array.Empty<string>();

        return Directory.GetFiles($"{Folder.Path}/screenshots", "*.png");
    }

    public async Task CreateMinecraftAsync()
    {
        if (Minecraft != null) return;

        await SetupVersionAsync();

        Minecraft = new Minecraft(Version, Folder)
            .WithCustomLauncherDetails("mcLaunch", "1.0.0", exposeLauncher)
            .WithUser(AuthenticationManager.Account!, AuthenticationManager.Platform!)
            .WithDownloaders(BoxManager.AssetsDownloader, BoxManager.LibrariesDownloader, BoxManager.JVMDownloader)
            .WithSystemFolder(BoxManager.SystemFolder);
    }

    public void SetExposeLauncher(bool exposeLauncher)
    {
        this.exposeLauncher = exposeLauncher;
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

    public void SetAndSaveIcon(Bitmap icon)
    {
        icon.Save($"{Path}/icon.png");
        Manifest.Icon = icon;
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
            m.PlatformId.ToLower() == "curseforge").ToArray();

        foreach (BoxStoredModification mod in modsToMigrate)
        {
            statusCallback?.Invoke(mod, cur, Manifest.Modifications.Count);
            cur++;
            
            SHA1 sha = SHA1.Create();

            foreach (string filename in mod.Filenames)
            {
                string realFilename = $"{Folder.Path}/{filename}";
                
                FileStream fs = new FileStream(realFilename, FileMode.Open);
                string hash = Convert.ToHexString(await sha.ComputeHashAsync(fs)).ToLower();
                fs.Close();

                ModVersion? modVersion = await ModrinthModPlatform.Instance.GetModVersionFromSha1(hash);
                if (modVersion == null) continue;
                
                Manifest.RemoveModification(mod.Id, this);
                bool success = await ModrinthModPlatform.Instance.InstallModAsync(this, modVersion.Mod, 
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
    {
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(Manifest));
    }

    public Process Run()
    {
        return Minecraft.Run();
    }

    public Process Run(string serverAddress, string serverPort)
    {
        return Minecraft
            .WithServer(serverAddress, serverPort)
            .Run();
    }
}