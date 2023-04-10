using System.Diagnostics;
using System.Text.Json;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;

namespace ddLaunch.Core.Boxes;

public class Box
{
    string manifestPath;
    public string Path { get; }
    public MinecraftFolder Folder { get; }
    public Minecraft Minecraft { get; private set; }
    public Process MinecraftProcess { get; }
    public MinecraftVersion Version { get; private set; }
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
    }

    public Box(string path)
    {
        Path = path;
        manifestPath = $"{path}/box.json";

        Manifest = JsonSerializer.Deserialize<BoxManifest>(File.ReadAllText(manifestPath))!;
        Manifest?.RunPostDeserializationChecks();

        if (File.Exists($"{path}/icon.png"))
        {
            Manifest.Icon = new Bitmap($"{path}/icon.png");
        }

        Folder = new MinecraftFolder($"{path}/minecraft");
    }

    async Task SetupVersionAsync()
    {
        if (Version != null) return;
        Version = await Manifest.Setup();
    }

    public async Task CreateMinecraftAsync()
    {
        if (Minecraft != null) return;
        
        await SetupVersionAsync();

        Minecraft = new Minecraft(Version, Folder)
            .WithCustomLauncherDetails("ddLaunch", "1.0.0")
            .WithUser(AuthenticationManager.Account!, AuthenticationManager.Platform!)
            .WithDownloaders(BoxManager.AssetsDownloader, BoxManager.LibrariesDownloader)
            .WithSystemFolder(BoxManager.SystemFolder);
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

    public void SaveManifest()
    {
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(Manifest));
    }

    public Process Run()
    {
        return Minecraft.Run();
    }
}