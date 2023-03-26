﻿using System.Text.Json.Serialization;
using Avalonia.Media;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Utilities;

namespace ddLaunch.Core.Boxes;

public class BoxManifest
{
    ManifestMinecraftVersion version;
    public string Name { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string ModLoaderId { get; set; }
    public string ModLoaderVersion { get; set; }
    public string DescriptionLine => $"{ModLoaderId.ToUpper()} {Version}";
    public List<BoxStoredModification> Modifications { get; set; } = new();

    [JsonIgnore] public IImage Icon { get; set; }

    [JsonIgnore] public ModLoaderSupport? ModLoader => ModLoaderManager.Get(ModLoaderId);

    public BoxManifest()
    {
    }

    public BoxManifest(string name, string description, string author, string modLoaderId, string modLoaderVersion,
        IImage icon, ManifestMinecraftVersion version)
    {
        Name = name;
        Description = description;
        Author = author;
        ModLoaderId = modLoaderId;
        ModLoaderVersion = modLoaderVersion;
        Id = IdGenerator.Generate();
        Icon = icon;

        this.version = version;
        Version = version.Id;
    }

    public bool HasModification(string id, string versionId, string platformId)
        => Modifications.FirstOrDefault(m => m.Id == id 
                                             && m.PlatformId == platformId 
                                             && m.VersionId == versionId) != null;
    
    public bool HasModification(string id, string platformId)
        => Modifications.FirstOrDefault(m => m.Id == id 
                                             && m.PlatformId == platformId) != null;

    public BoxStoredModification? GetModification(string id)
        => Modifications.FirstOrDefault(mod => mod.Id == id);

    public void AddModification(string id, string versionId, string platformId, string[] filenames)
    {
        if (HasModification(id, versionId, platformId)) return;
        
        Modifications.Add(new BoxStoredModification
        {
            Id = id,
            PlatformId = platformId,
            VersionId = versionId,
            Filenames = filenames
        });
    }

    public void RemoveModification(string id)
    {
        BoxStoredModification? mod = GetModification(id);
        if (mod == null) return;
        
        mod.Delete();
        Modifications.Remove(mod);
    }

    public async Task<MinecraftVersion> Setup()
    {
        MinecraftVersion mcVersion =
            await MinecraftManager.Manifest.Get(Version).DownloadOrGetLocally(BoxManager.SystemFolder);

        await BoxManager.SetupVersionAsync(mcVersion, downloadAllAfter: false);

        ModLoaderSupport modLoader = ModLoader;
        if (modLoader != null && modLoader is not VanillaModLoaderSupport)
        {
            ModLoaderVersion[]? versions = await modLoader.GetVersionsAsync(Version);
            ModLoaderVersion version = versions.FirstOrDefault(v => v.Name == ModLoaderVersion);

            MinecraftVersion? mlMcVersion = await version.GetMinecraftVersionAsync(Version);

            // Merging
            mlMcVersion = mlMcVersion.Merge(mcVersion);

            // Install & setup this patched version for the modloader
            await BoxManager.SetupVersionAsync(mlMcVersion, customName: $"{modLoader.Name} {version.Name}",
                downloadAllAfter: false);

            return mlMcVersion;
        }

        await DownloadManager.DownloadAll();

        return mcVersion;
    }
}
public class BoxStoredModification
{
    public string Id { get; init; }
    public string VersionId { get; init; }
    public string PlatformId { get; init; }
    public string[] Filenames { get; init; }

    public void Delete()
    {
        foreach (string file in Filenames)
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
}