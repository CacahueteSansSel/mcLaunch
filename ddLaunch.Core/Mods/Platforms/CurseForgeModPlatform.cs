using System.Diagnostics;
using ddLaunch.Core.Boxes;
using CurseForge.Models;
using CurseForge.Models.Files;
using CurseForge.Models.Games;
using CurseForge.Models.Mods;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Utilities;
using CurseForgeClient = CurseForge.CurseForge;
using File = CurseForge.Models.Files.File;

namespace ddLaunch.Core.Mods.Platforms;

public class CurseForgeModPlatform : ModPlatform
{
    public static CurseForgeModPlatform Instance { get; private set; }
    public const int MinecraftGameId = 432;

    CurseForgeClient client;
    Dictionary<string, Modification> modCache = new();
    
    public override string Name { get; } = "Curseforge";

    public CurseForgeModPlatform(string apiKey)
    {
        Instance = this;
        client = new CurseForgeClient(apiKey, "ddLaunch/1.0.0");
    }
    
    public override async Task<Modification[]> GetModsAsync(int page, Box box, string searchQuery)
    {
        CursePaginatedResponse<List<Mod>> resp = await client.SearchMods(MinecraftGameId,
            gameVersion: box.Manifest.Version,
            modLoaderType: Enum.Parse<ModLoaderType>(box.Manifest.ModLoaderId, true), 
            sortField: ModsSearchSortField.Popularity, 
            pageSize: 10,
            searchFilter: searchQuery, 
            index: (uint)(page * 10)
        );

        Modification[] mods = resp.Data.Select(mod =>
        {
            List<string> minecraftVersions = new();

            foreach (var file in mod.LatestFiles)
            {
                foreach (string ver in file.GameVersions)
                {
                    if (!minecraftVersions.Contains(ver))
                        minecraftVersions.Add(ver);
                }
            }
            
            return new Modification
            {
                Id = mod.Id.ToString(),
                Name = mod.Name,
                ShortDescription = mod.Summary,
                Author = mod.Authors?.FirstOrDefault()?.Name,
                IconPath = mod.Logo.Url,
                MinecraftVersions = minecraftVersions.ToArray(),
                LatestMinecraftVersion = minecraftVersions.Last(),
                BackgroundPath = mod.Screnshots?.FirstOrDefault()?.Url,
                Platform = this
            };
        }).ToArray();
        
        // Download all mods images
        foreach (Modification mod in mods) await mod.DownloadIconAsync();

        return mods;
    }

    public override async Task<Modification> GetModAsync(string id)
    {
        string cacheName = $"mod-{id}";
        if (CacheManager.Has(cacheName)) return CacheManager.LoadModification(cacheName)!;
        if (modCache.ContainsKey(id))
            return modCache[id];

        try
        {
            Mod cfMod = (await client.GetMod(uint.Parse(id))).Data;
            List<string> minecraftVersions = new();

            foreach (var file in cfMod.LatestFiles)
            {
                foreach (string ver in file.GameVersions)
                {
                    if (!minecraftVersions.Contains(ver))
                        minecraftVersions.Add(ver);
                }
            }

            Modification mod = new Modification
            {
                Id = cfMod.Id.ToString(),
                Name = cfMod.Name,
                ShortDescription = cfMod.Summary,
                Author = cfMod.Authors?.FirstOrDefault()?.Name,
                IconPath = cfMod.Logo.Url,
                BackgroundPath = cfMod.Screnshots?.FirstOrDefault()?.Url,
                MinecraftVersions = minecraftVersions.ToArray(),
                LatestMinecraftVersion = minecraftVersions.Last(),
                Versions = cfMod.LatestFiles.Select(f => f.Id.ToString()).ToArray(),
                LatestVersion = cfMod.LatestFiles?.FirstOrDefault().Id.ToString(),
                LongDescriptionBody = (await client.GetModDescription(cfMod.Id)).Data,
                Platform = this
            };

            modCache.Add(id, mod);
            CacheManager.Store(mod, cacheName);
            return mod;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public override async Task<string[]> GetVersionsForMinecraftVersionAsync(string modId, string modLoaderId, string minecraftVersionId)
    {
        Mod cfMod = (await client.GetMod(uint.Parse(modId))).Data;
        List<string> modVersions = new();

        foreach (var file in cfMod.LatestFiles)
        {
            // Mod.GameVersions contains the Minecraft version (1.19.4), the mod loader (Fabric), and the side (Client)
            // We need to check those
            
            if (!file.GameVersions.Contains(modLoaderId.Capitalize()) 
                || !file.GameVersions.Contains("Client")
                || !file.GameVersions.Contains(minecraftVersionId))
                continue;
            
            if (!modVersions.Contains(file.Id.ToString()))
                modVersions.Add(file.Id.ToString());
        }

        return modVersions.ToArray();
    }

    async Task InstallFile(Box targetBox, File file)
    {
        if (!file.GameVersions.Contains(targetBox.Manifest.Version))
        {
            Debug.WriteLine($"Mod {file.DisplayName} is incompatible ! Skipping");
            return;
        }
        
        if (file.Dependencies != null && file.Dependencies.Count > 0)
        {
            foreach (FileDependency dep in file.Dependencies)
            {
                if (dep.RelationType != FileRelationType.RequiredDependency) continue;
                
                Mod cfMod = (await client.GetMod(dep.ModId)).Data;
                await InstallFile(targetBox, cfMod.LatestFiles[0]);
            }
        }

        if (targetBox.Manifest.HasModification(file.ModId.ToString(), Name))
        {
            Debug.WriteLine($"Mod {file.DisplayName} already installed in {targetBox.Manifest.Name}");
            return;
        }
        
        DownloadManager.Begin(file.DisplayName);
        
        List<string> filenames = new();
        
        string path = $"{targetBox.Folder.Path}/mods/{file.FileName}";
        string url = file.DownloadUrl;

        // TODO: This may break things
        if (!System.IO.File.Exists(path)) DownloadManager.Add(url, path, EntryAction.Download);

        filenames.Add(path);

        targetBox.Manifest.AddModification(file.ModId.ToString(), file.Id.ToString(), Name,
            filenames.ToArray());
        
        DownloadManager.End();
    }

    public override async Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId)
    {
        var version = await client.GetModFile(uint.Parse(mod.Id), uint.Parse(versionId));
        if (version == null) return false;

        await InstallFile(targetBox, version.Data);
        
        await DownloadManager.DownloadAll();

        targetBox.SaveManifest();
        return true;
    }

    public override async Task<Modification> DownloadAdditionalInfosAsync(Modification mod)
    {
        // With the CurseForge API, we don't need to download additional infos for a mod (when searching for example)
        
        return mod;
    }

    public override ModPlatform GetModPlatform(string id)
    {
        if (id == Name) return this;

        return null;
    }
}