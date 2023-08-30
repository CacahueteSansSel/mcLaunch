using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Mods.Platforms;

namespace mcLaunch.Core.Mods.Packs;

public class CurseForgeModificationPack : ModificationPack
{
    public override string Name { get; init; }
    public override string Author { get; init; }
    public override string Version { get; init; }
    public override string? Id { get; init; }
    public override string? Description { get; init; }
    public override string MinecraftVersion { get; init; }
    public override string ModloaderId { get; init; }
    public override string ModloaderVersion { get; init; }
    public override SerializedModification[] Modifications { get; set; }
    public override AdditionalFile[] AdditionalFiles { get; set; }

    public CurseForgeModificationPack()
    {
        
    }

    public CurseForgeModificationPack(string filename) : base(filename)
    {
        ZipArchive zip = ZipFile.Open(filename, ZipArchiveMode.Read);

        using Stream manifestStream = zip.GetEntry("manifest.json")!.Open();
        ModelManifest manifest = JsonSerializer.Deserialize<ModelManifest>(manifestStream)!;

        ZipArchiveEntry[] overridesEntries = zip.Entries
            .Where(e => e.FullName.StartsWith("overrides"))
            .ToArray();

        Name = manifest.Name;
        Author = manifest.Author;
        Version = manifest.Version;
        MinecraftVersion = manifest.Minecraft.Version;
        Description = "Imported from a CurseForge modpack";
        
        string[] modloaderVersionTokens = manifest.Minecraft.Modloaders[0].Id.Split('-');
        ModloaderId = modloaderVersionTokens[0];
        ModloaderVersion = modloaderVersionTokens[1];

        Modifications = manifest.Files.Select(file => new SerializedModification
        {
            PlatformId = "Curseforge",
            ModId = file.ProjectId.ToString(),
            VersionId = file.FileId.ToString(),
            IsRequired = file.IsRequired
        }).ToArray();

        AdditionalFiles = overridesEntries.Where(entry => entry.FullName.Contains('.')).Select(entry => new AdditionalFile
        {
            Path = entry.FullName.Replace("overrides/", ""),
            Data = entry.Open().ReadToEndAndClose(entry.Length),
        }).ToArray();
    }
    
    public override async Task InstallModificationAsync(Box targetBox, SerializedModification mod)
    {
        await CurseForgeModPlatform.Instance.InstallModAsync(targetBox, new Modification
        {
            Id = mod.ModId
        }, mod.VersionId, false);
    }

    public override Task ExportAsync(Box box, string filename)
    {
        throw new NotImplementedException();
    }

    public class ModelManifest
    {
        [JsonPropertyName("minecraft")] public ModelMinecraft Minecraft { get; set; }

        [JsonPropertyName("manifestType")] public string ManifestType { get; set; }

        [JsonPropertyName("manifestVersion")] public int ManifestVersion { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("version")] public string Version { get; set; }

        [JsonPropertyName("author")] public string Author { get; set; }

        [JsonPropertyName("files")] public ModelFile[] Files { get; set; }

        public class ModelFile
        {
            [JsonPropertyName("projectID")] public uint ProjectId { get; set; }

            [JsonPropertyName("fileID")] public uint FileId { get; set; }

            [JsonPropertyName("required")] public bool IsRequired { get; set; }
        }

        public class ModelMinecraft
        {
            [JsonPropertyName("version")] public string Version { get; set; }
            [JsonPropertyName("modLoaders")] public ModelModloader[] Modloaders { get; set; }

            public class ModelModloader
            {
                [JsonPropertyName("id")] public string Id { get; set; }

                [JsonPropertyName("primary")] public bool IsPrimary { get; set; }
            }
        }
    }
}