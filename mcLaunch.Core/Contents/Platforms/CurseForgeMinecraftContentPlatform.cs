using System.Collections.Concurrent;
using System.Diagnostics;
using CurseForge.Models;
using CurseForge.Models.Files;
using CurseForge.Models.Fingerprints;
using CurseForge.Models.Games;
using CurseForge.Models.Mods;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using CurseForgeClient = CurseForge.CurseForge;
using File = CurseForge.Models.Files.File;

namespace mcLaunch.Core.Contents.Platforms;

public class CurseForgeMinecraftContentPlatform : MinecraftContentPlatform
{
    public static CurseForgeMinecraftContentPlatform Instance { get; private set; }
    public const int MinecraftGameId = 432;
    public const int ModpacksClassId = 4471;
    public const int ModsClassId = 6;
    public const int ResourcePacksClassId = 12;
    public const int ShaderPacksClassId = 6552;
    public const int DataPacksClassId = 6945;
    public const int WorldsClassId = 17;

    CurseForgeClient client;
    ConcurrentDictionary<string, MinecraftContent> contentCache = new();

    public override string Name { get; } = "Curseforge";

    public CurseForgeMinecraftContentPlatform(string apiKey)
    {
        Instance = this;
        client = new CurseForgeClient(apiKey, "mcLaunch/1.0.0");
    }

    uint GetClassIdForType(MinecraftContentType type)
    {
        return type switch
        {
            MinecraftContentType.Modification => ModsClassId,
            MinecraftContentType.ResourcePack => ResourcePacksClassId,
            MinecraftContentType.ShaderPack => ShaderPacksClassId,
            MinecraftContentType.DataPack => DataPacksClassId,
            MinecraftContentType.World => WorldsClassId,
            _ => 0
        };
    }

    MinecraftContentType GetTypeFromClassId(uint classId)
    {
        return classId switch
        {
            ModsClassId => MinecraftContentType.Modification,
            ResourcePacksClassId => MinecraftContentType.ResourcePack,
            ShaderPacksClassId => MinecraftContentType.ShaderPack,
            DataPacksClassId => MinecraftContentType.DataPack,
            WorldsClassId => MinecraftContentType.World,
            _ => 0
        };
    }

    public override async Task<PaginatedResponse<MinecraftContent>> GetContentsAsync(int page, Box box,
        string searchQuery, MinecraftContentType contentType)
    {
        ModLoaderType type = ModLoaderType.Any;

        if (contentType == MinecraftContentType.Modification)
        {
            if (box != null && !Enum.TryParse(box.Manifest.ModLoaderId, true, out type))
                return PaginatedResponse<MinecraftContent>.Empty;
        }
        else type = ModLoaderType.Any;

        CursePaginatedResponse<List<Mod>> resp;

        try
        {
            resp = await client.SearchMods(MinecraftGameId,
                gameVersion: box?.Manifest.Version,
                modLoaderType: (box == null || contentType != MinecraftContentType.Modification) ? null : type,
                sortField: ModsSearchSortField.Popularity,
                sortOrder: SortOrder.Descending,
                pageSize: 10,
                classId: GetClassIdForType(contentType),
                searchFilter: searchQuery,
                index: (uint) (page * 10)
            );
        }
        catch (HttpRequestException)
        {
            return PaginatedResponse<MinecraftContent>.Empty;
        }

        MinecraftContent[] contents = resp.Data.Select(mod =>
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

            MinecraftContent m = new MinecraftContent
            {
                Id = mod.Id.ToString(),
                Type = contentType,
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

        // Download all contents images
        foreach (MinecraftContent content in contents) content.DownloadIconAsync();

        return new PaginatedResponse<MinecraftContent>(page, (int) resp.Pagination.TotalCount, contents);
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

    public override async Task<MinecraftContent> GetContentAsync(string id)
    {
        if (!uint.TryParse(id, out uint intId)) 
            return null;
        if (contentCache.TryGetValue(id, out var cachedContent))
            return cachedContent;
        
        string cacheName = $"content-curseforge-{id}";
        if (CacheManager.HasContent(cacheName))
        {
            // Mods loaded from the cache 
            MinecraftContent? content = CacheManager.LoadContent(cacheName)!;

            if (content != null)
            {
                content.Platform = this;
                return content;
            }
        }

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
            

            MinecraftContent content = new MinecraftContent
            {
                Id = cfMod.Id.ToString(),
                Type = GetTypeFromClassId(cfMod.ClassId ?? ModsClassId), // TODO: Check if cfMod.ClassId is safe to use
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

            contentCache.TryAdd(id, content);
            CacheManager.Store(content, cacheName);
            return content;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public override async Task<ContentVersion[]> GetContentVersionsAsync(MinecraftContent content, string? modLoaderId,
        string? minecraftVersionId)
    {
        if (!uint.TryParse(content.Id, out uint id)) return [];
        
        try
        {
            ModLoaderType? modLoader = string.IsNullOrWhiteSpace(modLoaderId) 
                                       || content.Type != MinecraftContentType.Modification
                ? null
                : Enum.Parse<ModLoaderType>(modLoaderId, true);
            List<ContentVersion> modVersions = new();

            foreach (File file in (await client.GetModFiles(id, minecraftVersionId, modLoader)).Data)
            {
                string? modLoaderName = file.GameVersions.FirstOrDefault(ModLoaderManager.IsModLoaderName)?.ToLower()
                                        ?? "forge";

                modVersions.Add(new ContentVersion(content, file.Id.ToString(), file.DisplayName,
                    file.GameVersions.FirstOrDefault(v => v.Contains('.'))!, modLoaderName));
            }

            return modVersions.ToArray();
        }
        catch (HttpRequestException)
        {
            return [];
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

    public override async Task<PaginatedResponse<ContentDependency>> GetContentDependenciesAsync(string id, string modLoaderId,
        string versionId,
        string minecraftVersionId)
    {
        if (!uint.TryParse(id, out uint intId)) 
            return null;
        
        try
        {
            List<File> files = (await client.GetModFiles(intId, minecraftVersionId,
                modLoaderId == null ? null : Enum.Parse<ModLoaderType>(modLoaderId, true), pageSize: 100)).Data;
            File? file = files.FirstOrDefault(f => f.Id == uint.Parse(versionId));

            return await Task.Run(() =>
            {
                file ??= files.FirstOrDefault(f => f.Id == uint.Parse(versionId));
                if (file == null) return new PaginatedResponse<ContentDependency>(0, 1, []);

                return new PaginatedResponse<ContentDependency>(0, 1, file.Dependencies.Select(dep => new ContentDependency
                {
                    Content = GetContentAsync(dep.ModId.ToString()).GetAwaiter().GetResult(),
                    VersionId = GetContentAsync(dep.ModId.ToString()).GetAwaiter().GetResult()?.LatestVersion,
                    Type = ToRelationType(dep.RelationType)
                }).ToArray());
            });
        }
        catch (HttpRequestException)
        {
            return PaginatedResponse<ContentDependency>.Empty;
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

    async Task<string[]?> InstallFileAsync(Box targetBox, File file, bool installOptional, MinecraftContentType contentType)
    {
        if (!file.GameVersions.Contains(targetBox.Manifest.Version))
        {
            Debug.WriteLine($"Mod {file.DisplayName} is incompatible ! Skipping");
            return null;
        }

        if (file.Dependencies != null && file.Dependencies.Count > 0 && contentType == MinecraftContentType.Modification)
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
                await InstallFileAsync(targetBox, cfMod.LatestFiles[0], false, MinecraftContentType.Modification);
            }
        }

        if (targetBox.Manifest.HasContentStrict(file.ModId.ToString(), Name))
        {
            Debug.WriteLine($"Mod {file.DisplayName} already installed in {targetBox.Manifest.Name}");
            return null;
        }

        DownloadManager.Begin(file.DisplayName);

        List<string> filenames = new();

        string folder = MinecraftContentUtils.GetInstallFolderName(contentType);
        string path = $"{targetBox.Folder.Path}/{folder}/{file.FileName}";
        string url = file.DownloadUrl;

        if (url == null)
        {
            Debug.WriteLine($"Mod {file.DisplayName} cannot be downloaded !");
            DownloadManager.End();

            return null;
        }

        // TODO: This may break things
        if (!System.IO.File.Exists(path)) DownloadManager.Add(url, path, null, EntryAction.Download);

        filenames.Add($"{folder}/{file.FileName}");

        targetBox.Manifest.AddContent(file.ModId.ToString(), contentType, file.Id.ToString(), Name,
            filenames.ToArray());

        DownloadManager.End();

        return filenames.ToArray();
    }

    public override async Task<bool> InstallContentAsync(Box targetBox, MinecraftContent content, string versionId,
        bool installOptional)
    {
        if (content == null) return false;

        CurseResponse<File>? version;

        try
        {
            version = await client.GetModFile(uint.Parse(content.Id), uint.Parse(versionId));
            if (version == null) return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }

        string[]? filenames = null;

        if (!targetBox.HasContentSoft(content))
        {
            filenames = await InstallFileAsync(targetBox, version.Data, installOptional, content.Type);
            
            if (filenames == null)
            {
                await DownloadManager.ProcessAll();
                targetBox.SaveManifest();

                return false;
            }
        }

        await DownloadManager.ProcessAll();

        if (content.Type == MinecraftContentType.DataPack && filenames != null)
            targetBox.InstallDatapack(versionId, filenames[0]);

        targetBox.SaveManifest();
        return true;
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return new CurseForgeModificationPack(filename);
    }

    public override async Task<MinecraftContent> DownloadContentInfosAsync(MinecraftContent content)
    {
        if (!uint.TryParse(content.Id, out uint id)) 
            return null;
        
        Mod cfMod;

        try
        {
            cfMod = (await client.GetMod(id)).Data;
        }
        catch (HttpRequestException)
        {
            return content;
        }

        content.Changelog = (cfMod.LatestFiles == null || cfMod.LatestFiles.FirstOrDefault() == null)
            ? string.Empty
            : (await client.GetModFileChangelog(cfMod.Id, cfMod.LatestFiles.FirstOrDefault().Id)).Data;
        content.LongDescriptionBody = (await client.GetModDescription(id)).Data;

        return content;
    }

    public override MinecraftContentPlatform GetModPlatform(string id)
    {
        if (id == Name) return this;

        return null;
    }

    public override async Task<ContentVersion?> GetContentVersionFromData(Stream stream)
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
        MinecraftContent mod = await GetContentAsync(match.Id.ToString());
        // TODO: Maybe specify another modloader than forge as default
        string modLoaderName = match.File.GameVersions.FirstOrDefault(ModLoaderManager.IsModLoaderName)?.ToLower()
                               ?? "forge";

        return new ContentVersion(mod, match.File.Id.ToString(),
            match.File.DisplayName, match.File.GameVersions.FirstOrDefault(), modLoaderName);
    }
}