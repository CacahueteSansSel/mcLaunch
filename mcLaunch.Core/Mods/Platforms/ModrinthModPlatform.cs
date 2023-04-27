using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using Modrinth;
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
    Dictionary<string, Modification> modCache = new();

    public override string Name { get; } = "Modrinth";
    public ModrinthClient Client => client;

    public ModrinthModPlatform()
    {
        Instance = this;
        
        UserAgent ua = new UserAgent
        {
            ProjectName = "mcLaunch Minecraft Launcher",
            ProjectVersion = "1.0.0"
        };

        client = new ModrinthClient(new ModrinthClientConfig
        {
            UserAgent = ua.ToString()
        });
    }

    public override async Task<Modification[]> GetModsAsync(int page, Box box, string searchQuery)
    {
        FacetCollection collection = new();

        collection.Add(Facet.Category(box.Manifest.ModLoaderId.ToLower()));
        collection.Add(Facet.Version(box.Manifest.Version));
        collection.Add(Facet.ProjectType(ProjectType.Mod));

        SearchResponse search =
            await client.Project.SearchAsync(searchQuery, facets: collection, limit: 10, offset: (ulong) (page * 10));

        Modification[] mods = search.Hits.Select(hit => new Modification
        {
            Id = hit.ProjectId,
            Name = hit.Title,
            ShortDescription = hit.Description,
            Author = hit.Author,
            IconPath = hit.IconUrl,
            MinecraftVersions = hit.Versions,
            BackgroundPath = hit.Gallery?.FirstOrDefault(),
            LatestMinecraftVersion = hit.LatestVersion,
            Platform = this
        }).ToArray();

        // Download all mods images
        foreach (Modification mod in mods) await mod.DownloadIconAsync();

        return mods;
    }

    public override async Task<ModDependency[]> GetModDependenciesAsync(string id, string modLoaderId, string versionId,
        string minecraftVersionId)
    {
        Version[] versions = await client.Version.GetProjectVersionListAsync(id, new[] {modLoaderId},
            new[] {minecraftVersionId});
        Version? version = versions.FirstOrDefault(v => v.Id == versionId);

        return await Task.Run(() =>
        {
            return version.Dependencies.Select(dep => new ModDependency
            {
                Mod = GetModAsync(dep.ProjectId).GetAwaiter().GetResult(),
                Type = (DependencyRelationType) dep.DependencyType
            }).ToArray();
        });
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
                Author = team.Last().User.Username,
                IconPath = project.IconUrl,
                BackgroundPath = project.FeaturedGallery,
                MinecraftVersions = project.GameVersions,
                LatestMinecraftVersion = project.GameVersions.Last(),
                Versions = project.Versions,
                LatestVersion = project.Versions.Last(),
                LongDescriptionBody = project.Body,
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

    public override async Task<string[]> GetVersionsForMinecraftVersionAsync(string modId, string modLoaderId,
        string minecraftVersionId)
    {
        Version[] versions = await client.Version.GetProjectVersionListAsync(modId, new[] {modLoaderId},
            new[] {minecraftVersionId});

        return versions.Select(v => v.Id).ToArray();
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
                        new[] {targetBox.Manifest.ModLoaderId.ToLower()},
                        new[] {targetBox.Manifest.Version});

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
            // Ignore the sources jars to avoid problems
            if (file.FileName.Contains("-sources")) continue;
            
            string path = $"{targetBox.Folder.Path}/mods/{file.FileName}";
            string url = file.Url;

            // TODO: This may break things
            if (System.IO.File.Exists(path)) continue;

            DownloadManager.Add(url, path, EntryAction.Download);
            filenames.Add(path);
        }

        targetBox.Manifest.AddModification(version.ProjectId, version.Id, Name,
            filenames.ToArray());

        DownloadManager.End();
    }

    public override async Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId,
        bool installOptional)
    {
        Version version = await client.Version.GetAsync(versionId);
        if (version == null) return false;

        if (!targetBox.HasModification(mod)) await InstallVersionAsync(targetBox, version, installOptional);

        await DownloadManager.DownloadAll();

        targetBox.SaveManifest();
        return false;
    }

    public override async Task<Modification> DownloadAdditionalInfosAsync(Modification mod)
    {
        if (mod.LongDescriptionBody != null) return mod;

        Project project = await client.Project.GetAsync(mod.Id);
        Version latest = await client.Version.GetAsync(project.Versions[0]);

        mod.Versions = project.Versions;
        mod.LatestVersion = mod.Versions.Last();
        mod.LongDescriptionBody = project.Body;
        mod.BackgroundPath = project.FeaturedGallery;
        mod.Changelog = latest.Changelog;

        return mod;
    }

    public override ModPlatform GetModPlatform(string id)
    {
        if (id == Name) return this;

        return null;
    }
}