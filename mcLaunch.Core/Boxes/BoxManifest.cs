using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using ReactiveUI;

namespace mcLaunch.Core.Boxes;

public class BoxManifest : ReactiveObject
{
    private Bitmap? background;
    private IconCollection? icon;
    private MinecraftVersion setUpVersion;
    private ManifestMinecraftVersion version;

    public BoxManifest()
    {
    }

    public BoxManifest(string name, string description, string author, string modLoaderId, string modLoaderVersion,
        IconCollection icon, ManifestMinecraftVersion version, BoxType type = BoxType.Default)
    {
        Name = name;
        Description = description;
        Author = author;
        ModLoaderId = modLoaderId;
        ModLoaderVersion = modLoaderVersion;
        Id = IdGenerator.Generate();
        Icon = icon;
        LastLaunchTime = DateTime.Now;
        Type = type;

        this.version = version;
        Version = version.Id;
    }

    public int? ManifestVersion { get; set; }
    public string Name { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string ModLoaderId { get; set; }
    public string ModLoaderVersion { get; set; }
    public string DescriptionLine => $"{ModLoaderId.ToUpper()} {Version}";

    [JsonPropertyName("Modifications")] // For compatibility reasons
    public List<BoxStoredContent> Contents { get; set; } = [];

    public List<string> AdditionalModloaderFiles { get; set; } = [];

    [JsonIgnore]
    public IEnumerable<BoxStoredContent> ContentModifications =>
        Contents.Where(content => content.Type == MinecraftContentType.Modification);

    [JsonIgnore]
    public IEnumerable<BoxStoredContent> ContentResourcepacks =>
        Contents.Where(content => content.Type == MinecraftContentType.ResourcePack);

    [JsonIgnore]
    public IEnumerable<BoxStoredContent> ContentDatapacks =>
        Contents.Where(content => content.Type == MinecraftContentType.DataPack);

    [JsonIgnore]
    public IEnumerable<BoxStoredContent> ContentShaders =>
        Contents.Where(content => content.Type == MinecraftContentType.ShaderPack);

    [JsonIgnore]
    public IEnumerable<BoxStoredContent> ContentWorlds =>
        Contents.Where(content => content.Type == MinecraftContentType.World);

    [JsonIgnore] public string ModificationCount => Contents.Count.ToString();

    [JsonIgnore] public string ResourcepacksCount => ContentResourcepacks.Count().ToString();

    [JsonIgnore] public string DatapacksCount => ContentDatapacks.Count().ToString();

    [JsonIgnore] public string ShadersCount => ShadersCount.Count().ToString();

    [JsonIgnore] public string ContentWorldsCount => ContentWorlds.Count().ToString();

    public DateTime LastLaunchTime { get; set; }
    public BoxType Type { get; set; }

    [JsonIgnore] public string FileHash { get; set; }

    [JsonIgnore]
    public IconCollection? Icon
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
    public List<BoxBackup> Backups { get; set; } = [];

    public bool HasContentStrict(string id, string versionId, string platformId)
    {
        return Contents.FirstOrDefault(m => m.Id == id
                                           && m.PlatformId == platformId
                                           && m.VersionId == versionId) != null;
    }

    public bool HasContentStrict(string id, string platformId)
    {
        return Contents.FirstOrDefault(m => m.Id == id
                                           && m.PlatformId == platformId) != null;
    }

    public bool HasContentSoft(MinecraftContent content)
    {
        if (content == null) return false;

        BoxStoredContent? storedContent =
            Contents.FirstOrDefault(m => content.IsSimilar(m) || HasContentStrict(content.Id, content.ModPlatformId));

        return storedContent != null;
    }

    public BoxStoredContent? GetContent(string id)
    {
        return Contents.FirstOrDefault(content => content.Id == id);
    }

    public BoxStoredContent? GetContentByVersion(string versionId)
    {
        return Contents.FirstOrDefault(content => content.VersionId == versionId);
    }

    public BoxStoredContent[] GetContents(MinecraftContentType type)
    {
        return Contents.Where(c => c.Type == type).ToArray();
    }

    public void AddContent(MinecraftContent content, string versionId, string[] filenames)
    {
        if (filenames.Length == 0) return;
        if (HasContentStrict(content.Id, content.ModPlatformId)) return;

        lock (Contents)
        {
            Contents.Add(new BoxStoredContent
            {
                Content = content,
                VersionId = versionId,
                Filenames = filenames
            });
        }
    }

    public void RemoveContent(string id, Box box)
    {
        BoxStoredContent? content = GetContent(id);
        if (content == null) return;

        content.Delete(box.Folder.CompletePath);

        lock (Contents)
        {
            Contents.Remove(content);
        }
    }

    public async Task<bool> RunPostDeserializationChecksAsync()
    {
        bool hadChange = false;

        if (string.IsNullOrWhiteSpace(Author))
        {
            Author = "Unknown";
            hadChange = true;
        }

        await Parallel.ForEachAsync(Contents, async (content, token) =>
        {
            if (content.Content != null || string.IsNullOrEmpty(content.Id))
                return;

            content.Content = await ModPlatformManager.Platform.GetContentAsync(content.Id);
            hadChange = true;
        });
        
        // Remove duplicates
        List<string> seenContents = [];
        List<string> toRemoveContents = [];
        await Parallel.ForEachAsync(Contents, async (content, token) =>
        {
            if (content.Content == null) return;
            
            if (seenContents.Contains(content.Content!.Id))
            {
                toRemoveContents.Add(content.Content!.Id);
                return;
            }
            
            seenContents.Add(content.Content.Id);
        });

        Contents.RemoveAll(content => content.Content == null || toRemoveContents.Contains(content.Content!.Id));

        return hadChange;
    }

    public async Task<Result<MinecraftVersion>> Setup()
    {
        if (setUpVersion != null) return new Result<MinecraftVersion>(setUpVersion);

        MinecraftVersion mcVersion =
            await MinecraftManager.Manifest.Get(Version).DownloadOrGetLocally(BoxManager.SystemFolder);

        if (ModLoader != null) mcVersion = await ModLoader.PostProcessMinecraftVersionAsync(mcVersion);

        await BoxManager.SetupVersionAsync(mcVersion, downloadAllAfter: false);

        ModLoaderSupport modLoader = ModLoader;
        if (modLoader != null && modLoader.Type == "modded")
        {
            ModLoaderVersion[]? versions = await modLoader.GetVersionsAsync(Version);
            ModLoaderVersion version = versions.FirstOrDefault(v => v.Name == ModLoaderVersion);
            if (version == null) version = versions[0];

            Result<MinecraftVersion> versionResult = await version.GetMinecraftVersionAsync(Version);
            if (versionResult.IsError) return versionResult;

            MinecraftVersion modloaderVersion = versionResult.Data!;

            // Merging
            if (modLoader.NeedsMerging)
                modloaderVersion = modloaderVersion.Merge(mcVersion);

            // Install & setup this patched version for the modloader
            await BoxManager.SetupVersionAsync(modloaderVersion, $"{modLoader.Name} {version.Name}",
                false);

            setUpVersion = modloaderVersion;
            return new Result<MinecraftVersion>(setUpVersion);
        }

        setUpVersion = mcVersion;

        await DownloadManager.ProcessAll();

        return new Result<MinecraftVersion>(setUpVersion);
    }

    public override string ToString()
    {
        return $"Manifest {Id} {Name}";
    }
}

public class BoxStoredContent
{
    private string? id, platformId, name, author;
    private MinecraftContentType? type;
    
    public MinecraftContent? Content { get; set; }

    [Obsolete("Use Content instead")]
    public string Id
    {
        get => id ?? Content.Id;
        set => id = value;
    }

    public string VersionId { get; init; }
    [Obsolete("Use Content instead")]
    public string PlatformId 
    {
        get => platformId ?? Content.ModPlatformId;
        set => platformId = value;
    }
    public string[] Filenames { get; set; }
    [Obsolete("Use Content instead")]
    public string Name
    {
        get => name ?? Content.Name;
        set => name = value;
    }
    [Obsolete("Use Content instead")]
    public string Author
    {
        get => author ?? Content.Author;
        set => author = value;
    }
    [Obsolete("Use Content instead")]
    public MinecraftContentType Type
    {
        get => type ?? Content.Type;
        set => type = value;
    }

    public void Delete(string boxRootPath)
    {
        foreach (string file in Filenames)
        {
            if (Path.IsPathFullyQualified(file))
                throw new Exception("Mod filename is absolute : was the manifest updated to Manifest Version 2 ?");

            string path = $"{boxRootPath}/{file.TrimStart('/')}";

            if (File.Exists(path)) File.Delete(path);
        }
    }
}

public class BoxBackup
{
    public BoxBackup()
    {
    }

    public BoxBackup(string name, BoxBackupType type, DateTime creationTime, string filename)
    {
        Name = name;
        Type = type;
        CreationTime = creationTime;
        Filename = filename;
    }

    public string Name { get; set; }
    public BoxBackupType Type { get; set; }
    public DateTime CreationTime { get; set; }
    public string Filename { get; set; }

    [JsonIgnore] public bool IsCompleteBackup => Type == BoxBackupType.Complete;
    [JsonIgnore] public bool IsPartialBackup => Type == BoxBackupType.Partial;
}

public enum BoxBackupType
{
    Complete,
    Partial
}

public enum BoxType
{
    Default,
    Temporary
}