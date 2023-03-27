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
        FacetCollection collection = new FacetCollection
        {
            {
                Facet.Category(box.Manifest.ModLoaderId.ToLower()), Facet.Version(box.Manifest.Version),
                Facet.ProjectType(ProjectType.Mod)
            }
        };

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
        Project project = await client.Project.GetAsync(modId);
        List<string> versionIds = new();

        foreach (string versionId in project.Versions.Reverse())
        {
            Version version = await client.Version.GetAsync(versionId);

            if (version.GameVersions.Contains(minecraftVersionId) && version.Loaders.Contains(modLoaderId.ToLower()))
            {
                versionIds.Add(versionId);

                // TODO: This bad fix fixes the infinite loop and spamming of the Modrinth API when scanning all versions
                // This should be fixed in the future
                break;
            }
        }

        return versionIds.ToArray();
    }

    public override async Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId)
    {
        Version version = await client.Version.GetAsync(versionId);
        if (version == null) return false;

        if (version.Dependencies != null)
        {
            foreach (Dependency dependency in version.Dependencies)
            {
                if (dependency.DependencyType != DependencyType.Required) continue;

                if (targetBox.Manifest.HasModification(dependency.ProjectId, dependency.VersionId, Name))
                    continue;

                Version dependencyVersion = await client.Version.GetAsync(dependency.VersionId);

                DownloadManager.Begin($"Dependency {dependencyVersion.Name}");

                List<string> filenames = new();
                foreach (File file in dependencyVersion.Files)
                {
                    string path = $"{targetBox.Folder.Path}/mods/{file.FileName}";
                    string url = file.Url;

                    // TODO: This may break things
                    if (System.IO.File.Exists(path)) continue;

                    DownloadManager.Add(url, path, EntryAction.Download);
                    filenames.Add(path);
                }

                targetBox.Manifest.AddModification(dependency.ProjectId, dependency.VersionId, Name,
                    filenames.ToArray());

                DownloadManager.End();
            }
        }

        if (!targetBox.HasModification(mod))
        {
            DownloadManager.Begin($"{mod.Name} {version.Name}");

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

            targetBox.Manifest.AddModification(mod.Id, versionId, Name, filenames.ToArray());

            DownloadManager.End();
        }

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