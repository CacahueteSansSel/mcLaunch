using System.Diagnostics;
using CurseForge.Models;
using CurseForge.Models.Files;
using CurseForge.Models.Fingerprints;
using CurseForge.Models.Games;
using CurseForge.Models.Mods;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods.Packs;
using CurseForgeClient = CurseForge.CurseForge;
using File = CurseForge.Models.Files.File;

namespace mcLaunch.Core.Mods.Platforms;

public class CurseForgeModPlatform : ModPlatform
{
    public static CurseForgeModPlatform Instance { get; private set; }
    public const int MinecraftGameId = 432;
    public const int ModpacksClassId = 4471;
    public const int ModsClassId = 6;

    CurseForgeClient client;
    Dictionary<string, Modification> modCache = new();

    public override string Name { get; } = "Curseforge";

    public CurseForgeModPlatform(string apiKey)
    {
        Instance = this;
        client = new CurseForgeClient(apiKey, "mcLaunch/1.0.0");
    }

    public override async Task<PaginatedResponse<Modification>> GetModsAsync(int page, Box box, string searchQuery)
    {
        ModLoaderType type = ModLoaderType.Any;
        if (box != null && !Enum.TryParse(box.Manifest.ModLoaderId, true, out type))
            return PaginatedResponse<Modification>.Empty;

        CursePaginatedResponse<List<Mod>> resp;

        try
        {
            resp = await client.SearchMods(MinecraftGameId,
                gameVersion: box?.Manifest.Version,
                modLoaderType: box == null ? null : type,
                sortField: ModsSearchSortField.Popularity,
                sortOrder: SortOrder.Descending,
                pageSize: 10,
                classId: ModsClassId,
                searchFilter: searchQuery,
                index: (uint) (page * 10)
            );
        }
        catch (HttpRequestException)
        {
            return PaginatedResponse<Modification>.Empty;
        }

        Modification[] mods = resp.Data.Select(mod =>
        {
            List<string> minecraftVersions = new();

            foreach (var file in mod.LatestFiles)
            {
                foreach (string ver in file.GameVersions)
                {
                    if (!minecraftVersions.Contains(ver) && ver.Contains('.'))
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
                DownloadCount = (int) mod.DownloadCount,
                LastUpdated = mod.DateModified.DateTime,
                Platform = this
            };

            return m;
        }).ToArray();

        // Download all mods images
        foreach (Modification mod in mods) mod.DownloadIconAsync();

        return new PaginatedResponse<Modification>(page, (int) resp.Pagination.TotalCount, mods);
    }

    public override async Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion)
    {
        CursePaginatedResponse<List<Mod>> resp;

        try
        {
            resp = await client.SearchMods(MinecraftGameId,
                gameVersion: minecraftVersion,
                sortField: ModsSearchSortField.Popularity,
                sortOrder: SortOrder.Descending,
                pageSize: 10,
                classId: ModpacksClassId,
                searchFilter: searchQuery,
                index: (uint) (page * 10)
            );
        }
        catch (HttpRequestException)
        {
            return PaginatedResponse<PlatformModpack>.Empty;
        }

        PlatformModpack[] modpacks = resp.Data.Select(modpack => new PlatformModpack()
        {
            Id = modpack.Id.ToString(),
            Name = modpack.Name,
            ShortDescription = modpack.Summary,
            Author = modpack.Authors[0].Name,
            Url = null,
            IconPath = modpack.Logo.Url,
            MinecraftVersions = modpack.LatestFiles.SelectMany(f => f.GameVersions).ToArray(),
            BackgroundPath = modpack.Screnshots?.FirstOrDefault()?.Url ?? modpack.Logo.Url,
            LatestMinecraftVersion = modpack.LatestFiles.Count == 0 ? null : modpack.LatestFiles[0].GameVersions[0],
            DownloadCount = (int) modpack.DownloadCount,
            LastUpdated = modpack.DateModified.DateTime,
            Platform = this
        }).ToArray();

        // Download all modpack images
        // TODO: fix that causing slow loading process
        foreach (PlatformModpack pack in modpacks) await pack.DownloadIconAsync();

        return new PaginatedResponse<PlatformModpack>(page, (int) resp.Pagination.TotalCount, modpacks);
    }

    public override async Task<Modification> GetModAsync(string id)
    {
        string cacheName = $"mod-{id}";
        if (CacheManager.Has(cacheName)) return CacheManager.LoadModification(cacheName)!;
        if (modCache.ContainsKey(id))
            return modCache[id];

        try
        {
            if (!uint.TryParse(id, out uint value)) return null;
            Mod cfMod = (await client.GetMod(value)).Data;
            List<string> minecraftVersions = new();

            foreach (var file in cfMod.LatestFiles)
            {
                foreach (string ver in file.GameVersions)
                {
                    if (!minecraftVersions.Contains(ver) && ver.Contains('.'))
                        minecraftVersions.Add(ver);
                }
            }

            Modification mod = new Modification
            {
                Id = cfMod.Id.ToString(),
                Name = cfMod.Name,
                ShortDescription = cfMod.Summary,
                Author = cfMod.Authors?.FirstOrDefault()?.Name,
                IconUrl = cfMod.Logo?.Url,
                BackgroundPath = cfMod.Screnshots?.FirstOrDefault()?.Url,
                MinecraftVersions = minecraftVersions.ToArray(),
                LatestMinecraftVersion = minecraftVersions.Last(),
                Versions = cfMod.LatestFiles.Select(f => f.Id.ToString()).ToArray(),
                LatestVersion = cfMod.LatestFiles?.FirstOrDefault().Id.ToString(),
                LongDescriptionBody = (await client.GetModDescription(cfMod.Id)).Data,
                DownloadCount = (int) cfMod.DownloadCount,
                LastUpdated = cfMod.DateModified.DateTime,
                Platform = this
            };

            modCache.TryAdd(id, mod);
            CacheManager.Store(mod, cacheName);
            return mod;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public override async Task<ModVersion[]> GetModVersionsAsync(Modification mod, string? modLoaderId,
        string? minecraftVersionId)
    {
        try
        {
            ModLoaderType? modLoader = string.IsNullOrWhiteSpace(modLoaderId)
                ? null
                : Enum.Parse<ModLoaderType>(modLoaderId, true);
            List<ModVersion> modVersions = new();

            foreach (File file in (await client.GetModFiles(uint.Parse(mod.Id), minecraftVersionId,
                         modLoader)).Data)
            {
                string? modLoaderName = file.GameVersions.FirstOrDefault(ModLoaderManager.IsModLoaderName)?.ToLower()
                                        ?? "forge";

                modVersions.Add(new ModVersion(mod, file.Id.ToString(), file.DisplayName,
                    file.GameVersions.FirstOrDefault(v => v.Contains('.'))!, modLoaderName));
            }

            return modVersions.ToArray();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<ModVersion>();
        }
    }

    public override async Task<PlatformModpack> GetModpackAsync(string id)
    {
        if (!uint.TryParse(id, out uint value)) return null;

        Mod cfMod;

        try
        {
            cfMod = (await client.GetMod(value)).Data;
        }
        catch (HttpRequestException e)
        {
            return null;
        }
        
        List<string> minecraftVersions = new();

        foreach (var file in cfMod.LatestFiles)
        {
            foreach (string ver in file.GameVersions)
            {
                if (!minecraftVersions.Contains(ver) && ver.Contains('.'))
                    minecraftVersions.Add(ver);
            }
        }

        PlatformModpack.ModpackVersion[] versions = cfMod.LatestFiles.Select(pv => new PlatformModpack.ModpackVersion
        {
            Id = pv.Id.ToString(),
            Name = pv.DisplayName,
            MinecraftVersion = pv.GameVersions.FirstOrDefault(),
            ModLoader = cfMod.LatestFileIndexes?.FirstOrDefault(i => i.FileId == pv.Id)?.ModLoader.ToString().ToLower(),
            ModpackFileUrl = pv.DownloadUrl,
            ModpackFileHash = pv.Hashes.FirstOrDefault()?.Value
        }).ToArray();

        PlatformModpack mod = new PlatformModpack
        {
            Id = cfMod.Id.ToString(),
            Name = cfMod.Name,
            ShortDescription = cfMod.Summary,
            Author = cfMod.Authors?.FirstOrDefault()?.Name,
            IconPath = cfMod.Logo.Url,
            BackgroundPath = cfMod.Screnshots?.FirstOrDefault()?.Url ?? cfMod.Logo.Url,
            MinecraftVersions = minecraftVersions.ToArray(),
            LatestMinecraftVersion = minecraftVersions.Last(),
            Versions = versions,
            LatestVersion = versions[0],
            LongDescriptionBody = (await client.GetModDescription(cfMod.Id)).Data,
            DownloadCount = (int) cfMod.DownloadCount,
            LastUpdated = cfMod.DateModified.DateTime,
            Platform = this
        };

        return mod;
    }

    public override async Task<PaginatedResponse<ModDependency>> GetModDependenciesAsync(string id, string modLoaderId,
        string versionId,
        string minecraftVersionId)
    {
        try
        {
            List<File> files = (await client.GetModFiles(uint.Parse(id), minecraftVersionId,
                Enum.Parse<ModLoaderType>(modLoaderId, true), pageSize: 100)).Data;
            File? file = files.FirstOrDefault(f => f.Id == uint.Parse(versionId));

            return await Task.Run(() =>
            {
                file ??= files.FirstOrDefault(f => f.Id == uint.Parse(versionId));
                if (file == null) return new PaginatedResponse<ModDependency>(0, 1, []);

                return new PaginatedResponse<ModDependency>(0, 1, file.Dependencies.Select(dep => new ModDependency
                {
                    Mod = GetModAsync(dep.ModId.ToString()).GetAwaiter().GetResult(),
                    VersionId = GetModAsync(dep.ModId.ToString()).GetAwaiter().GetResult()?.LatestVersion,
                    Type = ToRelationType(dep.RelationType)
                }).ToArray());
            });
        }
        catch (HttpRequestException)
        {
            return PaginatedResponse<ModDependency>.Empty;
        }
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

    async Task<bool> InstallFile(Box targetBox, File file, bool installOptional)
    {
        if (!file.GameVersions.Contains(targetBox.Manifest.Version))
        {
            Debug.WriteLine($"Mod {file.DisplayName} is incompatible ! Skipping");
            return false;
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
            return false;
        }

        DownloadManager.Begin(file.DisplayName);

        List<string> filenames = new();

        string path = $"{targetBox.Folder.Path}/mods/{file.FileName}";
        string url = file.DownloadUrl;

        if (url == null)
        {
            Debug.WriteLine($"Mod {file.DisplayName} cannot be downloaded !");
            DownloadManager.End();

            return false;
        }

        // TODO: This may break things
        if (!System.IO.File.Exists(path)) DownloadManager.Add(url, path, null, EntryAction.Download);

        filenames.Add($"mods/{file.FileName}");

        targetBox.Manifest.AddModification(file.ModId.ToString(), file.Id.ToString(), Name,
            filenames.ToArray());

        DownloadManager.End();

        return true;
    }

    public override async Task<bool> InstallModAsync(Box targetBox, Modification mod, string versionId,
        bool installOptional)
    {
        if (mod == null) return false;

        CurseResponse<File>? version;

        try
        {
            version = await client.GetModFile(uint.Parse(mod.Id), uint.Parse(versionId));
            if (version == null) return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }

        if (!targetBox.HasModificationSoft(mod))
        {
            if (!await InstallFile(targetBox, version.Data, installOptional))
            {
                await DownloadManager.ProcessAll();
                targetBox.SaveManifest();

                return false;
            }
        }

        await DownloadManager.ProcessAll();

        targetBox.SaveManifest();
        return true;
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return new CurseForgeModificationPack(filename);
    }

    public override async Task<Modification> DownloadModInfosAsync(Modification mod)
    {
        Mod cfMod;

        try
        {
            cfMod = (await client.GetMod(uint.Parse(mod.Id))).Data;
        }
        catch (HttpRequestException)
        {
            return mod;
        }

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

    public override async Task<ModVersion?> GetModVersionFromData(Stream stream)
    {
        byte[] data = stream.ReadToEndAndClose();
        uint hash = MurmurHash2.HashNormal(data);

        CurseResponse<FingerprintsMatchesResult>? resp;

        try
        {
            resp = await client.GetFingerprintsMatches([hash]);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        if (resp.Data.ExactMatches.Count == 0) return null;

        FingerprintMatch match = resp.Data.ExactMatches[0];
        Modification mod = await GetModAsync(match.Id.ToString());
        // TODO: Maybe specify another modloader than forge as default
        string modLoaderName = match.File.GameVersions.FirstOrDefault(ModLoaderManager.IsModLoaderName)?.ToLower()
                               ?? "forge";

        return new ModVersion(mod, match.File.Id.ToString(),
            match.File.DisplayName, match.File.GameVersions.FirstOrDefault(), modLoaderName);
    }
}