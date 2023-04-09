using System.Text.Json.Serialization;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using ddLaunch.Core.Utilities;
using ReactiveUI;

namespace ddLaunch.Core.Boxes;

public class BoxManifest : ReactiveObject
{
    ManifestMinecraftVersion version;
    Bitmap icon;
    Bitmap? background;
    MinecraftVersion setUpVersion;

    public string Name { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string ModLoaderId { get; set; }
    public string ModLoaderVersion { get; set; }
    public string DescriptionLine => $"{ModLoaderId.ToUpper()} {Version}";
    public string ModificationCount => Modifications.Count.ToString();
    public List<BoxStoredModification> Modifications { get; set; } = new();

    [JsonIgnore]
    public Bitmap Icon
    {
        get => icon;
        set => this.RaiseAndSetIfChanged(ref icon, value);
    }

    [JsonIgnore]
    public Bitmap? Background
    {
        get => background;
        set => this.RaiseAndSetIfChanged(ref background, value);
    }

    [JsonIgnore] public ModLoaderSupport? ModLoader => ModLoaderManager.Get(ModLoaderId);

    public BoxManifest()
    {
    }

    public BoxManifest(string name, string description, string author, string modLoaderId, string modLoaderVersion,
        Bitmap icon, ManifestMinecraftVersion version)
    {
        Name = name;
        Description = description;
        Author = author;
        ModLoaderId = modLoaderId;
        ModLoaderVersion = modLoaderVersion;
        Id = IdGenerator.Generate();
        Icon = icon;

        this.version = version;
        Version = version.Id;
    }

    public bool HasModificationStrict(string id, string versionId, string platformId)
        => Modifications.FirstOrDefault(m => m.Id == id
                                             && m.PlatformId == platformId
                                             && m.VersionId == versionId) != null;

    public bool HasModificationStrict(string id, string platformId)
        => Modifications.FirstOrDefault(m => m.Id == id
                                             && m.PlatformId == platformId) != null;

    public bool HasModificationSoft(Modification mod)
        => Modifications.FirstOrDefault(m => mod.IsSimilar(m) || HasModificationStrict(m.Id, m.PlatformId)) != null;

    public BoxStoredModification? GetModification(string id)
        => Modifications.FirstOrDefault(mod => mod.Id == id);

    public void AddModification(string id, string versionId, string platformId, string[] filenames)
    {
        if (HasModificationStrict(id, versionId, platformId)) return;

        Modifications.Add(new BoxStoredModification
        {
            Id = id,
            PlatformId = platformId,
            VersionId = versionId,
            Filenames = filenames
        });
    }

    public void RemoveModification(string id)
    {
        BoxStoredModification? mod = GetModification(id);
        if (mod == null) return;

        mod.Delete();
        Modifications.Remove(mod);
    }

    public void RunPostDeserializationChecks()
    {
        if (string.IsNullOrWhiteSpace(Author)) Author = "Unknown";
    }

    public async Task<MinecraftVersion> Setup()
    {
        if (setUpVersion != null) return setUpVersion;

        MinecraftVersion mcVersion =
            await MinecraftManager.Manifest.Get(Version).DownloadOrGetLocally(BoxManager.SystemFolder);

        await BoxManager.SetupVersionAsync(mcVersion, downloadAllAfter: false);

        ModLoaderSupport modLoader = ModLoader;
        if (modLoader != null && modLoader is not VanillaModLoaderSupport)
        {
            ModLoaderVersion[]? versions = await modLoader.GetVersionsAsync(Version);
            ModLoaderVersion version = versions.FirstOrDefault(v => v.Name == ModLoaderVersion);

            MinecraftVersion? mlMcVersion = await version.GetMinecraftVersionAsync(Version);

            // Merging
            if (modLoader.NeedsMerging)
                mlMcVersion = mlMcVersion.Merge(mcVersion);

            // Install & setup this patched version for the modloader
            await BoxManager.SetupVersionAsync(mlMcVersion, customName: $"{modLoader.Name} {version.Name}",
                downloadAllAfter: false);

            setUpVersion = mlMcVersion;

            return mlMcVersion;
        }

        setUpVersion = mcVersion;

        await DownloadManager.DownloadAll();

        return mcVersion;
    }
}

public class BoxStoredModification
{
    public string Id { get; init; }
    public string VersionId { get; init; }
    public string PlatformId { get; init; }
    public string[] Filenames { get; init; }
    public string Name { get; init; }
    public string Author { get; init; }

    public void Delete()
    {
        foreach (string file in Filenames)
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
}