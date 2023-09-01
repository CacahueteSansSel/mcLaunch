using System.Diagnostics;
using CurseForge.Models;
using CurseForge.Models.Files;
using CurseForge.Models.Games;
using CurseForge.Models.Mods;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods.Packs;
using CurseForgeClient = CurseForge.CurseForge;
using File = CurseForge.Models.Files.File;

namespace mcLaunch.Core.Mods.Platforms;

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
        client = new CurseForgeClient(apiKey, "mcLaunch/1.0.0");
    }

    public override async Task<Modification[]> GetModsAsync(int page, Box box, string searchQuery)
    {
        CursePaginatedResponse<List<Mod>> resp = await client.SearchMods(MinecraftGameId,
            gameVersion: box == null ? null : box.Manifest.Version,
            modLoaderType: box == null ? null : Enum.Parse<ModLoaderType>(box.Manifest.ModLoaderId, true),
            sortField: ModsSearchSortField.Popularity,
            sortOrder: SortOrder.Descending,
            pageSize: 10,
            searchFilter: searchQuery,
            index: (uint) (page * 10)
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

            Modification m = new Modification
            {
                Id = mod.Id.ToString(),
                Name = mod.Name,
                ShortDescription = mod.Summary,
                Author = mod.Authors?.FirstOrDefault()?.Name,
                IconUrl = mod.Logo?.Url,
                MinecraftVersions = minecraftVersions.ToArray(),
                LatestMinecraftVersion = minecraftVersions.Last(),
                BackgroundPath = mod.Screnshots?.FirstOrDefault()?.Url,
                DownloadCount = (int)mod.DownloadCount,
                LastUpdated = mod.DateModified.DateTime,
                Platform = this
            };

            return m;
        }).ToArray();

        // Download all mods images
        foreach (Modification mod in mods) mod.DownloadIconAsync();

        return mods;
    }

    public override async Task<PlatformModpack[]> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion)
    {
        return Array.Empty<PlatformModpack>();
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
                IconUrl = cfMod.Logo.Url,
                BackgroundPath = cfMod.Screnshots?.FirstOrDefault()?.Url,
                MinecraftVersions = minecraftVersions.ToArray(),
                LatestMinecraftVersion = minecraftVersions.Last(),
                Versions = cfMod.LatestFiles.Select(f => f.Id.ToString()).ToArray(),
                LatestVersion = cfMod.LatestFiles?.FirstOrDefault().Id.ToString(),
                LongDescriptionBody = (await client.GetModDescription(cfMod.Id)).Data,
                DownloadCount = (int)cfMod.DownloadCount,
                LastUpdated = cfMod.DateModified.DateTime,
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

    public override async Task<PlatformModpack> GetModpackAsync(string id)
    {
        return null;
    }

    public override async Task<string[]> GetModVersionList(string modId, string modLoaderId,
        string minecraftVersionId)
    {
        //Mod cfMod = (await client.GetMod(uint.Parse(modId))).Data;
        List<string> modVersions = new();

        foreach (File file in (await client.GetModFiles(uint.Parse(modId), minecraftVersionId,
                     Enum.Parse<ModLoaderType>(modLoaderId, true), pageSize: 100)).Data)
        {
            modVersions.Add(file.Id.ToString());
        }

        return modVersions.ToArray();
    }

    public override async Task<ModDependency[]> GetModDependenciesAsync(string id, string modLoaderId, string versionId,
        string minecraftVersionId)
    {
        List<File> files = (await client.GetModFiles(uint.Parse(id), minecraftVersionId,
            Enum.Parse<ModLoaderType>(modLoaderId, true), pageSize: 100)).Data;
        File? file = files.FirstOrDefault(f => f.Id == uint.Parse(versionId));

        return await Task.Run(() =>
        {
            return file.Dependencies.Select(dep => new ModDependency
            {
                Mod = GetModAsync(dep.ModId.ToString()).GetAwaiter().GetResult(),
                VersionId = GetModAsync(dep.ModId.ToString()).GetAwaiter().GetResult().LatestVersion,
                Type = ToRelationType(dep.RelationType)
            }).ToArray();
        });
    }

    DependencyRelationType ToRelationType(FileRelationType fileRelationType)
    {
        switch (fileRelationType)
        {
            case FileRelationType.OptionalDependency:
                return DependencyRelationType.Optional;
            case FileRelationType.RequiredDependency:
                return DependencyRelationType.Required;
            case FileRelationType.Incompatible:
                return DependencyRelationType.Incompatible;
            default:
                return DependencyRelationType.Unknown;
        }
    }

    async Task InstallFile(Box targetBox, File file, bool installOptional)
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
                if (installOptional)
                {
                    if (dep.RelationType != FileRelationType.RequiredDependency &&
                        dep.RelationType != FileRelationType.OptionalDependency) continue;
                }
                else
                {
                    if (dep.RelationType != FileRelationType.RequiredDependency) continue;
                }

                Mod cfMod = (await client.GetMod(dep.ModId)).Data;
                await InstallFile(targetBox, cfMod.LatestFiles[0], false);
            }
        }

        if (targetBox.Manifest.HasModificationStrict(file.ModId.ToString(), Name))
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

        filenames.Add($"mods/{file.FileName}");

        targetBox.Manifest.AddModification(file.ModId.ToString(), file.Id.ToString(), Name,
            filenames.ToArray());

        DownloadManager.End();
    }

    public override async Task<bool> InstallModAsync(Box targetBox, Modification mod, string versionId,
        bool installOptional)
    {
        var version = await client.GetModFile(uint.Parse(mod.Id), uint.Parse(versionId));
        if (version == null) return false;

        if (!targetBox.HasModificationSoft(mod)) await InstallFile(targetBox, version.Data, installOptional);

        await DownloadManager.DownloadAll();

        targetBox.SaveManifest();
        return true;
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return new CurseForgeModificationPack(filename);
    }

    public override async Task<Modification> DownloadModInfosAsync(Modification mod)
    {
        Mod cfMod = (await client.GetMod(uint.Parse(mod.Id))).Data;

        mod.Changelog = (cfMod.LatestFiles == null || cfMod.LatestFiles.FirstOrDefault() == null)
            ? string.Empty
            : (await client.GetModFileChangelog(cfMod.Id, cfMod.LatestFiles.FirstOrDefault().Id)).Data;
        mod.LongDescriptionBody = (await client.GetModDescription(uint.Parse(mod.Id))).Data;

        return mod;
    }

    public override ModPlatform GetModPlatform(string id)
    {
        if (id == Name) return this;

        return null;
    }

    public override async Task<ModVersion?> GetModVersionFromSha1(string hash)
    {
        return null;
    }
}