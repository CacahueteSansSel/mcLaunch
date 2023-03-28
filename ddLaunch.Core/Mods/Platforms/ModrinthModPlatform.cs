using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using Modrinth;
using Modrinth.Models;
using Modrinth.Models.Enums.Project;
using Modrinth.Models.Enums.Version;
using File = Modrinth.Models.File;
using Version = Modrinth.Models.Version;

namespace ddLaunch.Core.Mods.Platforms;

public class ModrinthModPlatform : ModPlatform
{
    ModrinthClient client;
    Dictionary<string, Modification> modCache = new();

    public override string Name { get; } = "Modrinth";

    public ModrinthModPlatform()
    {
        UserAgent ua = new UserAgent
        {
            ProjectName = "ddLaunch Minecraft Launcher",
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

    public override async Task<Modification> GetModAsync(string id)
    {
        if (modCache.ContainsKey(id)) return modCache[id];

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
        return mod;
    }

    public override async Task<string[]> GetVersionsForMinecraftVersionAsync(string modId, string modLoaderId,
        string minecraftVersionId)
    {
        Version[] versions = await client.Version.GetProjectVersionListAsync(modId, new[] {modLoaderId},
            new[] {minecraftVersionId});

        return versions.Select(v => v.Id).ToArray();
    }

    async Task InstallVersionAsync(Box targetBox, Version version)
    {
        if (version.Dependencies != null)
        {
            foreach (Dependency dependency in version.Dependencies)
            {
                if (dependency.DependencyType != DependencyType.Required) continue;

                if (targetBox.Manifest.HasModification(dependency.ProjectId, dependency.VersionId, Name))
                    continue;

                string depVersionId = dependency.VersionId;
                Version dependencyVersion;

                if (depVersionId == null)
                {
                    // Grab the latest available version for this dependency
                    Version[] depVersions = await client.Version.GetProjectVersionListAsync(dependency.ProjectId,
                        new[] {targetBox.Manifest.ModLoaderId.ToLower()},
                        new[] {targetBox.Manifest.Version});

                    dependencyVersion = depVersions[0];
                }
                else
                {
                    dependencyVersion = await client.Version.GetAsync(depVersionId);
                }

                await InstallVersionAsync(targetBox, dependencyVersion);
            }
        }
        
        DownloadManager.Begin(version.Name);
        
        List<string> filenames = new();
        foreach (File file in version.Files)
        {
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

    public override async Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId)
    {
        Version version = await client.Version.GetAsync(versionId);
        if (version == null) return false;

        if (!targetBox.HasModification(mod)) await InstallVersionAsync(targetBox, version);

        await DownloadManager.DownloadAll();

        targetBox.SaveManifest();
        return false;
    }

    public override async Task<Modification> DownloadAdditionalInfosAsync(Modification mod)
    {
        if (mod.LongDescriptionBody != null) return mod;

        Project project = await client.Project.GetAsync(mod.Id);

        mod.Versions = project.Versions;
        mod.LatestVersion = mod.Versions.Last();
        mod.LongDescriptionBody = project.Body;
        mod.BackgroundPath = project.FeaturedGallery;

        return mod;
    }
}