using System.Diagnostics;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;

namespace ddLaunch.Core.Boxes;

public class Box
{
    string manifestPath;
    public MinecraftFolder Folder { get; }
    public Minecraft Minecraft { get; private set; }
    public Process MinecraftProcess { get; }
    public MinecraftVersion Version { get; private set; }
    public BoxManifest Manifest { get; }

    public bool IsRunning => MinecraftProcess != null && !MinecraftProcess.HasExited;

    public Box(BoxManifest manifest, string path)
    {
        manifestPath = $"{path}/box.json";

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));

        Folder = new MinecraftFolder($"{path}/minecraft");
        CreateMinecraft();
    }

    public Box(string path)
    {
        manifestPath = $"{path}/box.json";

        Manifest = JsonSerializer.Deserialize<BoxManifest>(File.ReadAllText(manifestPath))!;

        if (File.Exists($"{path}/icon.png"))
        {
            Manifest.Icon = new Bitmap($"{path}/icon.png");
        }

        Folder = new MinecraftFolder($"{path}/minecraft");
    }

    async Task SetupVersionAsync()
    {
        Version = await Manifest.Setup();
    }

    async Task CreateMinecraft()
    {
        await SetupVersionAsync();

        Minecraft = new Minecraft(Version, Folder)
            .WithCustomLauncherDetails("ddLaunch", "1.0.0")
            .WithUser("Carton", Guid.Parse("29a01aaf-5e92-41d4-86eb-e325d38ee6f8"))
            .WithDownloaders(BoxManager.AssetsDownloader, BoxManager.LibrariesDownloader)
            .WithSystemFolder(BoxManager.SystemFolder);
    }

    public async Task PrepareAsync()
    {
        await SetupVersionAsync();
        await CreateMinecraft();

        await BoxManager.SetupVersionAsync(Version);
    }

    public bool HasModification(Modification mod)
        => Manifest.HasModification(mod.Id, mod.Platform.Name);

    public void SaveManifest()
    {
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(Manifest));
    }

    public Process Run()
    {
        return Minecraft.Run();
    }
}