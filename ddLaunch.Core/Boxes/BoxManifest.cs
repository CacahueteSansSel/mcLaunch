using System.Text.Json.Serialization;
using Avalonia.Media;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Utilities;

namespace ddLaunch.Core.Boxes;

public class BoxManifest
{
    ManifestMinecraftVersion version;
    public string Name { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string ModLoaderId { get; set; }
    public string ModLoaderVersion { get; set; }
    public string DescriptionLine => $"{ModLoaderId.ToUpper()} {Version}";
    
    [JsonIgnore]
    public IImage Icon { get; set; }

    [JsonIgnore] public ModLoaderSupport? ModLoader => ModLoaderManager.Get(ModLoaderId);

    public BoxManifest()
    {
        
    }

    public BoxManifest(string name, string description, string author, string modLoaderId, string modLoaderVersion, IImage icon, ManifestMinecraftVersion version)
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

    public async Task<MinecraftVersion> Setup()
    {
        MinecraftVersion mcVersion =
            await MinecraftManager.Manifest.Get(Version).DownloadOrGetLocally(BoxManager.SystemFolder);

        await BoxManager.SetupVersionAsync(mcVersion);

        ModLoaderSupport modLoader = ModLoader;
        if (modLoader != null && modLoader is not VanillaModLoaderSupport)
        {
            ModLoaderVersion[]? versions = await modLoader.GetVersionsAsync(Version);
            ModLoaderVersion version = versions.FirstOrDefault(v => v.Name == ModLoaderVersion);
            
            MinecraftVersion? mlMcVersion = await version.GetMinecraftVersionAsync(Version);
            
            // Merging
            mlMcVersion = mlMcVersion.Merge(mcVersion);

            // Install & setup this patched version for the modloader
            await BoxManager.SetupVersionAsync(mlMcVersion);

            return mlMcVersion;
        }
        
        return mcVersion;
    }
}