﻿using System;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views.DataContexts;

public class DataContextModLoader
{
    public ModLoaderSupport ModLoader { get; }
    public string Name => ModLoader.Name;
    public ModLoaderVersion LatestVersion => ModLoader.LatestVersion;
    public Bitmap Icon { get; }

    public DataContextModLoader(ModLoaderSupport modLoader)
    {
        ModLoader = modLoader;

        using Stream stream =
            AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{modLoader.Id.ToLower()}.png"));
        Icon = new Bitmap(stream);
    }
}

public class MinecraftVersionSelectionDataContext : ReactiveObject
{
    ModLoaderVersion latestVersion;
    DataContextModLoader selectedModLoader;
    private DataContextModLoader[] modLoaders;

    public ManifestMinecraftVersion[] Versions { get; }

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

    public MinecraftVersionSelectionDataContext()
    {
        Versions = Settings.Instance.EnableSnapshots
            ? MinecraftManager.Manifest!.Versions
            : MinecraftManager.ManifestVersions;
        ModLoaders = ModLoaderManager.All.Select(m => new DataContextModLoader(m)).ToArray();

        selectedModLoader = ModLoaders[0];
    }
}