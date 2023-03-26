using ddLaunch.Core.Boxes;
using Modrinth;
using Modrinth.Models;

namespace ddLaunch.Core.Mods.Platforms;

public class ModrinthModPlatform : ModPlatform
{
    ModrinthClient client;
    
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
            {Facet.Category(box.Manifest.ModLoaderId.ToLower()), Facet.Version(box.Manifest.Version)}
        };

        SearchResponse search =
            await client.Project.SearchAsync(searchQuery, facets: collection, limit: 10, offset: (ulong)(page * 10));

        Modification[] mods = search.Hits.Select(hit => new Modification
        {
            Id = hit.ProjectId,
            Name = hit.Title,
            ShortDescription = hit.Description,
            Author = hit.Author,
            IconPath = hit.IconUrl,
            Versions = hit.Versions,
            LatestVersion = hit.LatestVersion,
            Platform = this
        }).ToArray();

        // Download all mods images
        foreach (Modification mod in mods) await mod.DownloadIconAsync();

        return mods;
    }

    public override async Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId)
    {
        return false;
    }

    public override async Task<Modification> DownloadAdditionalInfosAsync(Modification mod)
    {
        if (mod.LongDescriptionBody != null) return mod;
        
        Project project = await client.Project.GetAsync(mod.Id);

        mod.LongDescriptionBody = project.Body;

        return mod;
    }
}