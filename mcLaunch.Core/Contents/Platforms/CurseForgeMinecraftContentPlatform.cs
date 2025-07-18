﻿using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;
using CurseForge.Models;
using CurseForge.Models.Files;
using CurseForge.Models.Fingerprints;
using CurseForge.Models.Mods;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using CurseForgeClient = CurseForge.CurseForge;
using File = CurseForge.Models.Files.File;

namespace mcLaunch.Core.Contents.Platforms;

public class CurseForgeMinecraftContentPlatform : MinecraftContentPlatform
{
    public const int MinecraftGameId = 432;
    public const int ModpacksClassId = 4471;
    public const int ModsClassId = 6;
    public const int ResourcePacksClassId = 12;
    public const int ShaderPacksClassId = 6552;
    public const int DataPacksClassId = 6945;
    public const int WorldsClassId = 17;
    public const ModLoaderType NeoforgeModLoaderType = (ModLoaderType)6;

    private readonly CurseForgeClient client;
    private readonly ConcurrentDictionary<string, MinecraftContent> contentCache = new();

    public CurseForgeMinecraftContentPlatform(string apiKey)
    {
        Instance = this;
        client = new CurseForgeClient(apiKey, "mcLaunch/1.0.0");
    }

    public static CurseForgeMinecraftContentPlatform Instance { get; private set; }

    public override string Name { get; } = "Curseforge";

    private uint GetClassIdForType(MinecraftContentType type)
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

    private MinecraftContentType GetTypeFromClassId(uint classId)
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
    
    private bool TryParseModLoaderType(string text, out ModLoaderType type)
    {
        if (text.ToLower() == "neoforge")
        {
            type = NeoforgeModLoaderType;
            return true;
        }

        if (Enum.TryParse(text, true, out type))
            return true;

        return false;
    }

    public override async Task<PaginatedResponse<MinecraftContent>> GetContentsAsync(int page, Box box,
        string searchQuery, MinecraftContentType contentType)
    {
        ModLoaderType type = ModLoaderType.Any;

        if (contentType == MinecraftContentType.Modification)
        {
            if (box != null && !TryParseModLoaderType(box.Manifest.ModLoaderId, out type))
                return PaginatedResponse<MinecraftContent>.Empty;
        }
        else type = ModLoaderType.Any;

        CursePaginatedResponse<List<Mod>> resp;

        try
        {
            resp = await client.SearchMods(MinecraftGameId,
                gameVersion: box?.Manifest.Version,
                modLoaderType: box == null || contentType != MinecraftContentType.Modification ? null : type,
                sortField: ModsSearchSortField.Popularity,
                sortOrder: SortOrder.Descending,
                pageSize: 10,
                classId: GetClassIdForType(contentType),
                searchFilter: searchQuery,
                index: (uint)(page * 10)
            );
        }
        catch (HttpRequestException)
        {
            return PaginatedResponse<MinecraftContent>.Empty;
        }

        MinecraftContent[] contents = resp.Data.Select(mod =>
        {
            List<string> minecraftVersions = new();

            foreach (File? file in mod.LatestFiles)
            foreach (string ver in file.GameVersions)
            {
                if (!minecraftVersions.Contains(ver) && ver.Contains('.'))
                    minecraftVersions.Add(ver);
            }

            MinecraftContent m = new()
            {
                Id = mod.Id.ToString(),
                Slug = mod.Slug,
                Url = $"https://www.curseforge.com/minecraft/mc-mods/{mod.Slug}",
                Type = contentType,
                Name = mod.Name,
                ShortDescription = mod.Summary,
                Author = mod.Authors?.FirstOrDefault()?.Name,
                IconUrl = mod.Logo?.Url,
                MinecraftVersions = minecraftVersions.ToArray(),
                LatestMinecraftVersion = minecraftVersions.LastOrDefault(),
                BackgroundPath = mod.Screnshots?.FirstOrDefault()?.Url,
                DownloadCount = (int)mod.DownloadCount,
                LastUpdated = mod.DateModified.DateTime,
                Platform = this
            };

            return m;
        }).ToArray();

        return new PaginatedResponse<MinecraftContent>(page, (int)resp.Pagination.TotalCount, contents);
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
                index: (uint)(page * 10)
            );
        }
        catch (HttpRequestException)
        {
            return PaginatedResponse<PlatformModpack>.Empty;
        }

        PlatformModpack[] modpacks = resp.Data.Select(modpack => new PlatformModpack
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
            DownloadCount = (int)modpack.DownloadCount,
            LastUpdated = modpack.DateModified.DateTime,
            Platform = this
        }).ToArray();

        return new PaginatedResponse<PlatformModpack>(page, (int)resp.Pagination.TotalCount, modpacks);
    }

    public override async Task<MinecraftContent> GetContentAsync(string id)
    {
        if (!uint.TryParse(id, out uint intId))
            return null;
        if (contentCache.TryGetValue(id, out MinecraftContent? cachedContent))
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

            foreach (File? file in cfMod.LatestFiles)
            foreach (string ver in file.GameVersions)
            {
                if (!minecraftVersions.Contains(ver) && ver.Contains('.'))
                    minecraftVersions.Add(ver);
            }


            MinecraftContent content = new()
            {
                Id = cfMod.Id.ToString(),
                Slug = cfMod.Slug,
                Url = $"https://www.curseforge.com/minecraft/mc-mods/{cfMod.Slug}",
                Type = GetTypeFromClassId(cfMod.ClassId ?? ModsClassId), // TODO: Check if cfMod.ClassId is safe to use
                Name = cfMod.Name,
                ShortDescription = cfMod.Summary,
                Author = cfMod.Authors?.FirstOrDefault()?.Name,
                IconUrl = cfMod.Logo?.Url,
                BackgroundPath = cfMod.Screnshots?.FirstOrDefault()?.Url,
                MinecraftVersions = minecraftVersions.ToArray(),
                LatestMinecraftVersion = minecraftVersions.LastOrDefault(),
                Versions = cfMod.LatestFiles.Select(f => f.Id.ToString()).ToArray(),
                LatestVersion = cfMod.LatestFiles?.FirstOrDefault()?.Id.ToString(),
                LongDescriptionBody = (await client.GetModDescription(cfMod.Id)).Data,
                DownloadCount = (int)cfMod.DownloadCount,
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

    public override async Task<MinecraftContent?> GetContentByAppLaunchUriAsync(Uri uri)
    {
        if (uri.Scheme != "curseforge" || uri.Host != "install")
            return null;

        NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
        string? contentId = queryParams.Get("addonId");
        if (contentId == null)
            return null;

        return await GetContentAsync(contentId);
    }

    private ModLoaderType ParseModLoaderType(string input)
    {
        if (input.ToLower() == "neoforge")
            return (ModLoaderType)6;

        return Enum.Parse<ModLoaderType>(input, true);
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
                : ParseModLoaderType(modLoaderId);
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

        foreach (File? file in cfMod.LatestFiles)
        foreach (string ver in file.GameVersions)
        {
            if (!minecraftVersions.Contains(ver) && ver.Contains('.'))
                minecraftVersions.Add(ver);
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

        PlatformModpack mod = new()
        {
            Id = cfMod.Id.ToString(),
            Name = cfMod.Name,
            ShortDescription = cfMod.Summary,
            Author = cfMod.Authors?.FirstOrDefault()?.Name,
            IconPath = cfMod.Logo.Url,
            BackgroundPath = cfMod.Screnshots?.FirstOrDefault()?.Url ?? cfMod.Logo.Url,
            MinecraftVersions = minecraftVersions.ToArray(),
            LatestMinecraftVersion = minecraftVersions.LastOrDefault(),
            Versions = versions,
            LatestVersion = versions[0],
            LongDescriptionBody = (await client.GetModDescription(cfMod.Id)).Data,
            DownloadCount = (int)cfMod.DownloadCount,
            LastUpdated = cfMod.DateModified.DateTime,
            Platform = this
        };

        return mod;
    }

    public override async Task<PaginatedResponse<ContentDependency>> GetContentDependenciesAsync(string id,
        string modLoaderId,
        string versionId,
        string minecraftVersionId)
    {
        if (!uint.TryParse(id, out uint intId))
            return null;

        try
        {
            List<File> files = (await client.GetModFiles(intId, minecraftVersionId,
                modLoaderId == null ? null : TryParseModLoaderType(modLoaderId, out ModLoaderType type) ? type : null, pageSize: 100)).Data;
            File? file = files.FirstOrDefault(f => f.Id == uint.Parse(versionId));

            return await Task.Run(() =>
            {
                file ??= files.FirstOrDefault(f => f.Id == uint.Parse(versionId));
                if (file == null) return new PaginatedResponse<ContentDependency>(0, 1, []);

                return new PaginatedResponse<ContentDependency>(0, 1, file.Dependencies.Select(dep =>
                    new ContentDependency
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

    private DependencyRelationType ToRelationType(FileRelationType fileRelationType)
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

    private async Task<string[]?> InstallFileAsync(Box targetBox, File file, bool installOptional,
        MinecraftContentType contentType)
    {
        if (!file.GameVersions.Contains(targetBox.Manifest.Version))
        {
            Debug.WriteLine($"Mod {file.DisplayName} is incompatible ! Skipping");
            return null;
        }

        if (file.Dependencies != null
            && file.Dependencies.Count > 0
            && contentType == MinecraftContentType.Modification)
        {
            foreach (FileDependency dep in file.Dependencies)
            {
                if (dep.ModId == file.ModId)
                {
                    // For some reason, some mods references themselves in dependencies (see Thermal Integration on modrinth)
                    // We absolutely want to avoid that, because this will cause an infinite loop !

                    continue;
                }

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
                File? correctFile =
                    cfMod.LatestFiles.FirstOrDefault(f => f.GameVersions.Contains(targetBox.Manifest.Version));
                if (correctFile != null)
                    await InstallFileAsync(targetBox, correctFile, false, MinecraftContentType.Modification);
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
        string filename = string.IsNullOrWhiteSpace(file.FileName) ? Path.GetFileName(file.DownloadUrl) : file.FileName;
        string path = $"{targetBox.Folder.Path}/{folder}/{filename}";
        string url = file.DownloadUrl;

        if (url == null)
        {
            Debug.WriteLine($"Mod {file.DisplayName} cannot be downloaded !");
            DownloadManager.End();

            return null;
        }

        // TODO: This may break things
        if (!System.IO.File.Exists(path)) DownloadManager.Add(url, path, null, EntryAction.Download);

        filenames.Add($"{folder}/{filename}");

        targetBox.Manifest.AddContent(await GetContentAsync(file.ModId.ToString()), file.Id.ToString(),
            filenames.ToArray());

        DownloadManager.End();

        return filenames.ToArray();
    }

    private string DeduceCurseforgeFileUrl(File file)
    {
        string id = file.Id.ToString();

        return $"https://mediafilez.forgecdn.net/files/{id[..4]}/{id.Substring(4, 3)}/{file.FileName}";
    }

    public override async Task<bool> InstallContentAsync(Box targetBox, MinecraftContent content, string versionId,
        bool installOptional, bool processDownload)
    {
        if (content == null) return false;

        CurseResponse<File>? version;

        try
        {
            version = await client.GetModFile(uint.Parse(content.Id), uint.Parse(versionId));
            if (string.IsNullOrWhiteSpace(version.Data.DownloadUrl))
                version.Data.DownloadUrl = DeduceCurseforgeFileUrl(version.Data);
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
                if (processDownload) await DownloadManager.ProcessAll();
                await targetBox.SaveManifestAsync();

                return false;
            }
        }

        bool isDatapackToInstall = content.Type == MinecraftContentType.DataPack && filenames != null;

        if (processDownload || isDatapackToInstall)
            await DownloadManager.ProcessAll();
        if (isDatapackToInstall)
            targetBox.InstallDatapack(versionId, filenames[0]);

        await targetBox.SaveManifestAsync();
        return true;
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        if (!System.IO.File.Exists(filename)) return null;

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

        content.Changelog = cfMod.LatestFiles == null || cfMod.LatestFiles.FirstOrDefault() == null
            ? string.Empty
            : (await client.GetModFileChangelog(cfMod.Id, cfMod.LatestFiles.FirstOrDefault().Id)).Data;
        content.LongDescriptionBody = (await client.GetModDescription(id)).Data;
        content.Url = $"https://www.curseforge.com/minecraft/mc-mods/{cfMod.Slug}";

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