using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using ddLaunch.Core.Utilities;

namespace ddLaunch.Core.Mods.Packs;

public class CurseForgeModificationPack : ModificationPack
{
    public override string Name { get; }
    public override string Author { get; }
    public override string Version { get; }
    public override string MinecraftVersion { get; }
    public override string ModloaderId { get; }
    public override string ModloaderVersion { get; }
    public override SerializedModification[] Modifications { get; }
    public override AdditionalFile[] AdditionalFiles { get; }

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