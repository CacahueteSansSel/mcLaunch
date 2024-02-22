using System.Collections.Concurrent;
using System.Security.Cryptography;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using Modrinth;
using Modrinth.Exceptions;
using Modrinth.Helpers;
using Modrinth.Models;
using Modrinth.Models.Enums.Project;
using Modrinth.Models.Enums.Version;
using File = Modrinth.Models.File;
using Version = Modrinth.Models.Version;

namespace mcLaunch.Core.Contents.Platforms;

public class ModrinthMinecraftContentPlatform : MinecraftContentPlatform
{
    public static ModrinthMinecraftContentPlatform Instance { get; private set; }

    ModrinthClient client;
    ConcurrentDictionary<string, MinecraftContent> contentCache = new();

    public override string Name { get; } = "Modrinth";
    public ModrinthClient Client => client;

    public ModrinthMinecraftContentPlatform()
    {
        Instance = this;

        UserAgent ua = new UserAgent
        {
            ProjectName = "mcLaunch Minecraft Launcher",
            ProjectVersion = "1.0.0",
            Contact = "https://github.com/CacahueteSansSel/mcLaunch"
        };

        client = new ModrinthClient(new ModrinthClientConfig
        {
            UserAgent = ua.ToString()
        });
    }

    public override async Task<PaginatedResponse<MinecraftContent>> GetContentsAsync(int page, Box box,
        string searchQuery, MinecraftContentType contentType)
    {
        FacetCollection collection = new();

        if (box != null)
        {
            if (contentType == MinecraftContentType.Modification) 
                collection.Add(Facet.Category(box.Manifest.ModLoaderId.ToLower()));
            collection.Add(Facet.Version(box.Manifest.Version));
        }

        switch (contentType)
        {
            case MinecraftContentType.Modification:
                collection.Add(Facet.ProjectType(ProjectType.Mod));
                break;
            case MinecraftContentType.ResourcePack:
                collection.Add(Facet.ProjectType(ProjectType.Resourcepack));
                break;
            case MinecraftContentType.ShaderPack:
                collection.Add(Facet.ProjectType(ProjectType.Shader));
                break;
            case MinecraftContentType.DataPack:
                collection.Add(Facet.ProjectType(ProjectType.Datapack));
                break;
            case MinecraftContentType.World:
                // TODO: Find a way to search for worlds
                break;
        }

        SearchResponse search;

        try
        {
            search = await client.Project.SearchAsync(searchQuery, facets: collection, 
                limit: 10, offset: (ulong)(page * 10));
        }
        catch (ModrinthApiException e)
        {
            return PaginatedResponse<MinecraftContent>.Empty;
        }
        catch (TaskCanceledException e)
        {
            return PaginatedResponse<MinecraftContent>.Empty;
        }
        catch (HttpRequestException e)
        {
            return PaginatedResponse<MinecraftContent>.Empty;
        }

        MinecraftContent[] contents = search.Hits.Select(hit => new MinecraftContent
        {
            Id = hit.ProjectId,
            Type = contentType,
            Name = hit.Title,
            License = hit.License,
            ShortDescription = hit.Description,
            Author = hit.Author,
            Url = hit.GetDirectUrl(),
            IconUrl = hit.IconUrl,
            MinecraftVersions = hit.Versions,
            BackgroundPath = hit.Gallery?.FirstOrDefault(),
            LatestMinecraftVersion = hit.LatestVersion,
            DownloadCount = hit.Downloads,
            LastUpdated = hit.DateModified,
            Platform = this
        }).ToArray();

        // Download all content images
        foreach (MinecraftContent content in contents) content.DownloadIconAsync();

        return new PaginatedResponse<MinecraftContent>(page, search.TotalHits / search.Limit, contents);
    }

    public override async Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion)
    {
        FacetCollection collection = new();

        collection.Add(Facet.ProjectType(ProjectType.Modpack));

        SearchResponse search;

        try
        {
            search = await client.Project.SearchAsync(searchQuery, facets: collection, 
                limit: 10, offset: (ulong)(page * 10));
        }
        catch (ModrinthApiException e)
        {
            return PaginatedResponse<PlatformModpack>.Empty;
        }
        catch (TaskCanceledException e)
        {
            return PaginatedResponse<PlatformModpack>.Empty;
        }
        catch (HttpRequestException e)
        {
            return PaginatedResponse<PlatformModpack>.Empty;
        }

        PlatformModpack[] modpacks = search.Hits.Select(hit => new PlatformModpack
        {
            Id = hit.ProjectId,
            Name = hit.Title,
            ShortDescription = hit.Description,
            Author = hit.Author,
            Url = hit.GetDirectUrl(),
            IconPath = hit.IconUrl,
            MinecraftVersions = hit.Versions,
            BackgroundPath = hit.Gallery?.FirstOrDefault(),
            LatestMinecraftVersion = hit.Versions.Last(),
            DownloadCount = hit.Downloads,
            LastUpdated = hit.DateModified,
            Color = (uint)hit.Color.Value.ToArgb(),
            Platform = this
        }).ToArray();

        // Download all modpack images
        foreach (PlatformModpack pack in modpacks) pack.DownloadIconAsync();

        return new PaginatedResponse<PlatformModpack>(page, search.TotalHits / search.Limit, modpacks);
    }

    public override async Task<PaginatedResponse<ContentDependency>> GetContentDependenciesAsync(string id, string modLoaderId,
        string versionId,
        string minecraftVersionId)
    {
        try
        {
            Version[] versions = await client.Version.GetProjectVersionListAsync(id, modLoaderId == null ? null : [modLoaderId],
                new[] {minecraftVersionId});
            Version? version = versions.FirstOrDefault(v => v.Id == versionId);

            return new PaginatedResponse<ContentDependency>(0, 1, await Task.Run(() =>
            {
                return version.Dependencies.Where(dep => dep.ProjectId != null).Select(dep => new ContentDependency
                {
                    Content = GetContentAsync(dep.ProjectId).GetAwaiter().GetResult(),
                    VersionId = dep.VersionId,
                    Type = (DependencyRelationType) dep.DependencyType
                }).ToArray();
            }));
        }
        catch (ModrinthApiException e)
        {
            return PaginatedResponse<ContentDependency>.Empty;
        }
        catch (TaskCanceledException e)
        {
            return PaginatedResponse<ContentDependency>.Empty;
        }
        catch (HttpRequestException e)
        {
            return PaginatedResponse<ContentDependency>.Empty;
        }
    }

    MinecraftContentType GetContentTypeFromProjectType(ProjectType type)
    {
        return type switch
        {
            ProjectType.Mod => MinecraftContentType.Modification,
            ProjectType.Resourcepack => MinecraftContentType.ResourcePack,
            ProjectType.Shader => MinecraftContentType.ShaderPack,
            ProjectType.Datapack => MinecraftContentType.DataPack,
            _ => MinecraftContentType.Modification
        };
    }

    public override async Task<MinecraftContent> GetContentAsync(string id)
    {
        if (contentCache.TryGetValue(id, out var cachedMod))
            return cachedMod;
        
        string cacheName = $"content-modrinth-{id}";
        if (CacheManager.HasContent(cacheName))
        {
            // Mods loaded from the cache 
            MinecraftContent? mod = CacheManager.LoadContent(cacheName)!;

            if (mod != null)
            {
                mod.Platform = this;
                return mod;
            }
        }

        try
        {
            Project project = await client.Project.GetAsync(id);
            TeamMember[] team = await client.Team.GetAsync(project.Team);

            MinecraftContent content = new MinecraftContent
            {
                Id = project.Id,
                Type = GetContentTypeFromProjectType(project.ProjectType),
                Name = project.Title,
                ShortDescription = project.Description,
                Author = team.Length == 0 ? "No author" : team.Last().User.Username,
                License = project.License?.Name,
                Url = project.GetDirectUrl(),
                IconUrl = project.IconUrl,
                BackgroundPath = project.FeaturedGallery,
                MinecraftVersions = project.GameVersions,
                LatestMinecraftVersion = project.GameVersions.Last(),
                Versions = project.Versions,
                LatestVersion = project.Versions.Last(),
                LongDescriptionBody = project.Body,
                DownloadCount = project.Downloads,
                LastUpdated = project.Updated,
                Platform = this
            };

            content.TransformLongDescriptionToHtml();

            contentCache.TryAdd(id, content);
            CacheManager.Store(content, cacheName);
            
            return content;
        }
        catch (ModrinthApiException e)
        {
            return null;
        }
        catch (TaskCanceledException e)
        {
            return null;
        }
        catch (HttpRequestException e)
        {
            return null;
        }
    }

    public override async Task<ContentVersion[]> GetContentVersionsAsync(MinecraftContent content, string? modLoaderId,
        string? minecraftVersionId)
    {
        Version[] versions;

        try
        {
            versions = await client.Version.GetProjectVersionListAsync(content.Id,
                modLoaderId == null || content.Type != MinecraftContentType.Modification ? null : [modLoaderId],
                minecraftVersionId == null ? null : [minecraftVersionId]);
        }
        catch (ModrinthApiException e)
        {
            return Array.Empty<ContentVersion>();
        }
        catch (TaskCanceledException e)
        {
            return Array.Empty<ContentVersion>();
        }
        catch (HttpRequestException e)
        {
            return Array.Empty<ContentVersion>();
        }

        return versions.Select(v => new ContentVersion(content, v.Id, v.Name,
            v.GameVersions.FirstOrDefault(), v.Loaders.FirstOrDefault())).ToArray();
    }

    public override async Task<PlatformModpack> GetModpackAsync(string id)
    {
        try
        {
            Project project = await client.Project.GetAsync(id);
            TeamMember[] team = await client.Team.GetAsync(project.Team);

            Version[] projectVersions = await client.Version.GetProjectVersionListAsync(id);
            PlatformModpack.ModpackVersion[] versions = projectVersions.Select(pv => new PlatformModpack.ModpackVersion
            {
                Id = pv.Id,
                Name = pv.Name,
                MinecraftVersion = pv.GameVersions[0],
                ModLoader = pv.Loaders[0],
                ModpackFileUrl = pv.Files[0].Url,
                ModpackFileHash = pv.Files[0].Hashes.Sha1
            }).ToArray();

            PlatformModpack pack = new PlatformModpack
            {
                Id = project.Id,
                Name = project.Title,
                ShortDescription = project.Description,
                Author = team.Length == 0 ? "No author" : team.Last().User.Username,
                Url = project.GetDirectUrl(),
                IconPath = project.IconUrl,
                BackgroundPath = project.FeaturedGallery ?? (project.Gallery != null && project.Gallery.Length > 0
                    ? project.Gallery[0].Url
                    : null),
                MinecraftVersions = project.GameVersions,
                LatestMinecraftVersion = project.GameVersions.Last(),
                Versions = versions,
                LatestVersion = versions[0],
                LongDescriptionBody = project.Body,
                DownloadCount = project.Downloads,
                LastUpdated = project.Updated,
                Color = (uint)project.Color.Value.ToArgb(),
                Platform = this
            };

            pack.TransformLongDescriptionToHtml();

            return pack;
        }
        catch (ModrinthApiException e)
        {
            return null;
        }
        catch (TaskCanceledException e)
        {
            return null;
        }
        catch (HttpRequestException e)
        {
            return null;
        }
    }

    async Task InstallVersionAsync(Box targetBox, Version version, bool installOptional,
        MinecraftContentType contentType)
    {
        if (version.Dependencies != null && contentType == MinecraftContentType.Modification)
        {
            foreach (Dependency dependency in version.Dependencies)
            {
                if (installOptional)
                {
                    if (dependency.DependencyType != DependencyType.Required &&
                        dependency.DependencyType != DependencyType.Optional) continue;
                }
                else
                {
                    if (dependency.DependencyType != DependencyType.Required) continue;
                }

                if (targetBox.Manifest.HasContentStrict(dependency.ProjectId, Name))
                    continue;

                string depVersionId = dependency.VersionId;
                Version dependencyVersion;

                if (depVersionId == null)
                {
                    // Grab the latest available version for this dependency
                    Version[] depVersions = await client.Version.GetProjectVersionListAsync(dependency.ProjectId,
                        new[] { targetBox.Manifest.ModLoaderId.ToLower() },
                        new[] { targetBox.Manifest.Version });

                    if (depVersions.Length == 0) continue;

                    dependencyVersion = depVersions[0];
                }
                else
                {
                    dependencyVersion = await client.Version.GetAsync(depVersionId);
                }

                await InstallVersionAsync(targetBox, dependencyVersion, false, contentType);
            }
        }

        DownloadManager.Begin(version.Name);

        List<string> filenames = new();
        foreach (File file in version.Files)
        {
            // Ignore any non-primary file(s)
            if (!file.Primary && version.Files.Length > 1) continue;

            string folder = MinecraftContentUtils.GetInstallFolderName(contentType);
            string path = $"{targetBox.Folder.Path}/{folder}/{file.FileName}";
            string url = file.Url;

            DownloadManager.Add(url, path, file.Hashes.Sha1, EntryAction.Download);
            filenames.Add($"{folder}/{file.FileName}");
        }

        targetBox.Manifest.AddContent(version.ProjectId, contentType, version.Id, Name,
            filenames.ToArray());

        DownloadManager.End();
    }

    public override async Task<bool> InstallContentAsync(Box targetBox, MinecraftContent content, string versionId,
        bool installOptional)
    {
        Version version;
        
        try
        {
            version = await client.Version.GetAsync(versionId);
            if (version == null) return false;
        }
        catch (ModrinthApiException e)
        {
            return false;
        }
        catch (TaskCanceledException e)
        {
            return false;
        }
        catch (HttpRequestException e)
        {
            return false;
        }

        if (!targetBox.HasContentSoft(content)) 
            await InstallVersionAsync(targetBox, version, installOptional, content.Type);

        await DownloadManager.ProcessAll();

        targetBox.SaveManifest();
        return true;
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return await new ModrinthModificationPack(filename).SetupAsync();
    }

    public override async Task<MinecraftContent> DownloadContentInfosAsync(MinecraftContent content)
    {
        if (content.LongDescriptionBody != null && content.Changelog != null) return content;

        try
        {
            Project project = await client.Project.GetAsync(content.Id);
            Version latest = await client.Version.GetAsync(project.Versions[0]);

            content.Versions = project.Versions;
            content.LatestVersion = content.Versions.Last();
            content.LongDescriptionBody = project.Body;
            content.BackgroundPath = project.FeaturedGallery;
            content.Changelog = latest.Changelog;

            content.TransformLongDescriptionToHtml();
        }
        catch (ModrinthApiException e)
        {
            
        }
        catch (TaskCanceledException e)
        {
            
        }
        catch (HttpRequestException e)
        {
            
        }

        return content;
    }

    public override MinecraftContentPlatform GetModPlatform(string id)
    {
        if (id == Name) return this;

        return null;
    }

    public override async Task<ContentVersion?> GetContentVersionFromData(Stream stream)
    {
        SHA1 sha = SHA1.Create();
        string hash = Convert.ToHexString(await sha.ComputeHashAsync(stream));

        try
        {
            Version version = await client.VersionFile.GetVersionByHashAsync(hash);
            if (version == null) return null;

            return new ContentVersion(await GetContentAsync(version.ProjectId), version.Id,
                version.Name, version.GameVersions.FirstOrDefault(), version.Loaders.FirstOrDefault());
        }
        catch (ModrinthApiException e)
        {
            return null;
        }
        catch (TaskCanceledException e)
        {
            return null;
        }
        catch (HttpRequestException e)
        {
            return null;
        }
    }
}