using System.Diagnostics;
using System.Text.Json;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Core.Mods;
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

    public MinecraftWorld[] LoadWorlds()
    {
        if (!Directory.Exists($"{Folder.Path}/saves")) return Array.Empty<MinecraftWorld>();
        
        List<MinecraftWorld> worlds = new();

        foreach (string folder in Directory.GetDirectories($"{Folder.Path}/saves"))
            worlds.Add(new MinecraftWorld(System.IO.Path.GetFullPath(folder)));

        return worlds.ToArray();
    }

    public MinecraftServer[] LoadServers()
    {
        if (!File.Exists($"{Folder.Path}/servers.dat")) return Array.Empty<MinecraftServer>();
        
        List<MinecraftServer> servers = new();
        CompoundTag tag = NbtFile.Read($"{Folder.Path}/servers.dat", FormatOptions.Java);
        ListTag serversTag = (ListTag)tag["servers"];

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
        string[] inclusions = {
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