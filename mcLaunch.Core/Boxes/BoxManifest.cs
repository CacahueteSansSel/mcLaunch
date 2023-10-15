using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Utilities;
using ReactiveUI;

namespace mcLaunch.Core.Boxes;

public class BoxManifest : ReactiveObject
{
    ManifestMinecraftVersion version;
    IconCollection? icon;
    Bitmap? background;
    MinecraftVersion setUpVersion;

    public int? ManifestVersion { get; set; }
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
    public DateTime LastLaunchTime { get; set; }
    public BoxType Type { get; set; }
    
    [JsonIgnore]
    public string FileHash { get; set; }

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

    public bool HasModificationStrict(string id, string versionId, string platformId)
        => Modifications.FirstOrDefault(m => m.Id == id
                                             && m.PlatformId == platformId
                                             && m.VersionId == versionId) != null;

    public bool HasModificationStrict(string id, string platformId)
        => Modifications.FirstOrDefault(m => m.Id == id
                                             && m.PlatformId == platformId) != null;

    public bool HasModificationSoft(Modification mod)
    {
        BoxStoredModification? storedMod =
            Modifications.FirstOrDefault(m => mod.IsSimilar(m) || HasModificationStrict(mod.Id, mod.ModPlatformId));

        return storedMod != null;
    }

    public BoxStoredModification? GetModification(string id)
        => Modifications.FirstOrDefault(mod => mod.Id == id);

    public void AddModification(string id, string versionId, string platformId, string[] filenames)
    {
        if (filenames.Length == 0) return;
        if (HasModificationStrict(id, versionId, platformId)) return;

        Modifications.Add(new BoxStoredModification
        {
            Id = id,
            PlatformId = platformId,
            VersionId = versionId,
            Filenames = filenames
        });
    }

    public void RemoveModification(string id, Box box)
    {
        BoxStoredModification? mod = GetModification(id);
        if (mod == null) return;

        mod.Delete(box.Folder.CompletePath);
        Modifications.Remove(mod);
    }

    public async Task<bool> RunPostDeserializationChecks()
    {
        bool hadChange = false;

        if (string.IsNullOrWhiteSpace(Author))
        {
            Author = "Unknown";
            hadChange = true;
        }

        try
        {
            foreach (BoxStoredModification mod in Modifications)
            {
                if (!ManifestVersion.HasValue || ManifestVersion < 2)
                {
                    string[] newArray = mod.Filenames;
                    Regex relativePathRegex = new Regex("(?!\\/minecraft\\/)mods\\/.+");

                    for (int i = 0; i < newArray.Length; i++)
                    {
                        string filename = newArray[i];

                        if (!filename.StartsWith("mods/"))
                        {
                            filename = relativePathRegex.Match(filename).Value;
                            hadChange = true;
                        }

                        newArray[i] = filename;
                    }

                    mod.Filenames = newArray;
                }
            
                if (!string.IsNullOrWhiteSpace(mod.Name) && !string.IsNullOrWhiteSpace(mod.Author)) continue;

                Modification dlMod = await ModPlatformManager.Platform.GetModAsync(mod.Id);
                if (dlMod != null)
                {
                    mod.Name = dlMod.Name;
                    mod.Author = dlMod.Author;
                
                    hadChange = true;
                }
            }
        
            if (hadChange) ManifestVersion = 2;
        }
        catch (Exception e)
        {
            
        }

        return hadChange;
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
            if (version == null) version = versions[0];
            
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
    public string[] Filenames { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }

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

public enum BoxType
{
    Default,
    Temporary
}