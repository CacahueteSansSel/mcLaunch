using System;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;
using ReactiveUI;

namespace mcLaunch.Views.DataContexts;

public class DataContextModLoader
{
    public DataContextModLoader(ModLoaderSupport modLoader)
    {
        ModLoader = modLoader;

        using Stream stream =
            AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{modLoader.Id.ToLower()}.png"));
        Icon = new Bitmap(stream);
    }

    public ModLoaderSupport ModLoader { get; }
    public string Name => ModLoader.Name;
    public ModLoaderVersion LatestVersion => ModLoader.LatestVersion;
    public Bitmap Icon { get; }
}

public class MinecraftVersionSelectionDataContext : ReactiveObject
{
    private ModLoaderVersion latestVersion;
    private DataContextModLoader[] modLoaders;
    private DataContextModLoader selectedModLoader;
    private string? customText;

    public MinecraftVersionSelectionDataContext()
    {
        Versions = Settings.Instance.EnableSnapshots
            ? MinecraftManager.Manifest!.Versions
            : MinecraftManager.ManifestVersions;
        ModLoaders = ModLoaderManager.All.Select(m => new DataContextModLoader(m)).ToArray();

        selectedModLoader = ModLoaders[0];
    }

    public ManifestMinecraftVersion[] Versions { get; }
    
    public string CustomText
    {
        get => customText;
        set => this.RaiseAndSetIfChanged(ref customText, value);
    }

    public DataContextModLoader[] ModLoaders
    {
        get => modLoaders;
        set => this.RaiseAndSetIfChanged(ref modLoaders, value);
    }

    public DataContextModLoader SelectedModLoader
    {
        get => selectedModLoader;
        set => this.RaiseAndSetIfChanged(ref selectedModLoader, value);
    }

    public ModLoaderVersion LatestVersion
    {
        get => latestVersion;
        set => this.RaiseAndSetIfChanged(ref latestVersion, value);
    }
}