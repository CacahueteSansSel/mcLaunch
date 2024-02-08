using System.Collections.Concurrent;
using System.Security.Cryptography;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods.Packs;
using Modrinth;
using Modrinth.Helpers;
using Modrinth.Models;
using Modrinth.Models.Enums.Project;
using Modrinth.Models.Enums.Version;
using File = Modrinth.Models.File;
using Version = Modrinth.Models.Version;

namespace mcLaunch.Core.Mods.Platforms;

public class ModrinthModPlatform : ModPlatform
{
    public static ModrinthModPlatform Instance { get; private set; }

    ModrinthClient client;
    ConcurrentDictionary<string, Modification> modCache = new();

    public override string Name { get; } = "Modrinth";
    public ModrinthClient Client => client;

    public ModrinthModPlatform()
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

    public override async Task<PaginatedResponse<Modification>> GetModsAsync(int page, Box box, string searchQuery)
    {
        FacetCollection collection = new();

        if (box != null)
        {
            collection.Add(Facet.Category(box.Manifest.ModLoaderId.ToLower()));
            collection.Add(Facet.Version(box.Manifest.Version));
        }

        collection.Add(Facet.ProjectType(ProjectType.Mod));

        SearchResponse search =
            await client.Project.SearchAsync(searchQuery, facets: collection, limit: 10, offset: (ulong)(page * 10));

        Modification[] mods = search.Hits.Select(hit => new Modification
        {
            Id = hit.ProjectId,
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

        // Download all mods images
        foreach (Modification mod in mods) mod.DownloadIconAsync();

        return new PaginatedResponse<Modification>(page, search.TotalHits / search.Limit, mods);
    }

    public override async Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion)
    {
        FacetCollection collection = new();

        collection.Add(Facet.ProjectType(ProjectType.Modpack));

        SearchResponse search =
            await client.Project.SearchAsync(searchQuery, facets: collection, limit: 10, offset: (ulong)(page * 10));

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
        // TODO: fix that causing slow loading process
        foreach (PlatformModpack pack in modpacks) await pack.DownloadIconAsync();

        return new PaginatedResponse<PlatformModpack>(page, search.TotalHits / search.Limit, modpacks);
    }

    public override async Task<PaginatedResponse<ModDependency>> GetModDependenciesAsync(string id, string modLoaderId,
        string versionId,
        string minecraftVersionId)
    {
        Version[] versions = await client.Version.GetProjectVersionListAsync(id, new[] { modLoaderId },
            new[] { minecraftVersionId });
        Version? version = versions.FirstOrDefault(v => v.Id == versionId);

        return new PaginatedResponse<ModDependency>(0, 1, await Task.Run(() =>
        {
            return version.Dependencies.Where(dep => dep.ProjectId != null).Select(dep => new ModDependency
            {
                Mod = GetModAsync(dep.ProjectId).GetAwaiter().GetResult(),
                VersionId = dep.VersionId,
                Type = (DependencyRelationType)dep.DependencyType
            }).ToArray();
        }));
    }

    public override async Task<Modification> GetModAsync(string id)
    {
        string cacheName = $"mod-{id}";
        if (CacheManager.Has(cacheName)) return CacheManager.LoadModification(cacheName)!;
        if (modCache.ContainsKey(id))
            return modCache[id];

        try
        {
            Project project = await client.Project.GetAsync(id);
            TeamMember[] team = await client.Team.GetAsync(project.Team);

            Modification mod = new Modification
            {
                Id = project.Id,
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

            mod.TransformLongDescriptionToHtml();

            modCache.TryAdd(id, mod);
            CacheManager.Store(mod, cacheName);
            return mod;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public override async Task<ModVersion[]> GetModVersionsAsync(Modification mod, string modLoaderId,
        string minecraftVersionId)
    {
        Version[] versions = await client.Version.GetProjectVersionListAsync(mod.Id,
            modLoaderId == null ? null : [modLoaderId],
            minecraftVersionId == null ? null : [minecraftVersionId]);

        return versions.Select(v => new ModVersion(mod, v.Id, v.Name,
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
        catch (Exception e)
        {
            return null;
        }
    }

    async Task InstallVersionAsync(Box targetBox, Version version, bool installOptional)
    {
        if (version.Dependencies != null)
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

                if (targetBox.Manifest.HasModificationStrict(dependency.ProjectId, Name))
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

                await InstallVersionAsync(targetBox, dependencyVersion, false);
            }
        }

        DownloadManager.Begin(version.Name);

        List<string> filenames = new();
        foreach (File file in version.Files)
        {
            // Ignore any non-primary file(s)
            if (!file.Primary && version.Files.Length > 1) continue;

            string path = $"{targetBox.Folder.Path}/mods/{file.FileName}";
            string url = file.Url;

            DownloadManager.Add(url, path, file.Hashes.Sha1, EntryAction.Download);
            filenames.Add($"mods/{file.FileName}");
        }

        targetBox.Manifest.AddModification(version.ProjectId, version.Id, Name,
            filenames.ToArray());

        DownloadManager.End();
    }

    public override async Task<bool> InstallModAsync(Box targetBox, Modification mod, string versionId,
        bool installOptional)
    {
        Version version = await client.Version.GetAsync(versionId);
        if (version == null) return false;

        if (!targetBox.HasModificationSoft(mod)) await InstallVersionAsync(targetBox, version, installOptional);

        await DownloadManager.ProcessAll();

        targetBox.SaveManifest();
        return true;
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return await new ModrinthModificationPack(filename).SetupAsync();
    }

    public override async Task<Modification> DownloadModInfosAsync(Modification mod)
    {
        if (mod.LongDescriptionBody != null && mod.Changelog != null) return mod;

        Project project = await client.Project.GetAsync(mod.Id);
        Version latest = await client.Version.GetAsync(project.Versions[0]);

        mod.Versions = project.Versions;
        mod.LatestVersion = mod.Versions.Last();
        mod.LongDescriptionBody = project.Body;
        mod.BackgroundPath = project.FeaturedGallery;
        mod.Changelog = latest.Changelog;

        mod.TransformLongDescriptionToHtml();

        return mod;
    }

    public override ModPlatform GetModPlatform(string id)
    {
        if (id == Name) return this;

        return null;
    }

    public override async Task<ModVersion?> GetModVersionFromData(Stream stream)
    {
        SHA1 sha = SHA1.Create();
        string hash = Convert.ToHexString(await sha.ComputeHashAsync(stream));

        try
        {
            Version version = await client.VersionFile.GetVersionByHashAsync(hash);
            if (version == null) return null;

            return new ModVersion(await GetModAsync(version.ProjectId), version.Id,
                version.Name, version.GameVersions.FirstOrDefault(), version.Loaders.FirstOrDefault());
        }
        catch (Exception e)
        {
            return null;
        }
    }
}