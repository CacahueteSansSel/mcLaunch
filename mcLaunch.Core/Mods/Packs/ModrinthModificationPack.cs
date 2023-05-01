using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Mods.Platforms;
using mcLaunch.Core.Utilities;
using Version = Modrinth.Models.Version;

namespace mcLaunch.Core.Mods.Packs;

public class ModrinthModificationPack : ModificationPack
{
    private ModelModrinthIndex manifest;
    private ZipArchiveEntry[] overridesEntries;
    public override string Name { get; }
    public override string Author { get; }
    public override string Version { get; }
    public override string? Id { get; }
    public override string? Description { get; }
    public override string MinecraftVersion { get; }
    public override string ModloaderId { get; }
    public override string ModloaderVersion { get; }
    public override SerializedModification[] Modifications { get; set; }
    public override AdditionalFile[] AdditionalFiles { get; set; }

    public ModrinthModificationPack(string filename)
    {
        ZipArchive zip = ZipFile.Open(filename, ZipArchiveMode.Read);

        using Stream manifestStream = zip.GetEntry("modrinth.index.json")!.Open();
        manifest = JsonSerializer.Deserialize<ModelModrinthIndex>(manifestStream)!;

        overridesEntries = zip.Entries
            .Where(e => e.FullName.StartsWith("overrides") || e.FullName.StartsWith("client-overrides"))
            .ToArray();

        Name = manifest.Name;
        Author = "Unknown";
        Version = manifest.VersionId;
        Description = manifest.Summary;
        MinecraftVersion = manifest.Dependencies.Minecraft;

        if (!string.IsNullOrEmpty(manifest.Dependencies.FabricLoader))
        {
            ModloaderId = "fabric";
            ModloaderVersion = manifest.Dependencies.FabricLoader;
        } else if (!string.IsNullOrEmpty(manifest.Dependencies.Forge))
        {
            ModloaderId = "forge";
            ModloaderVersion = manifest.Dependencies.Forge;
        } else if (!string.IsNullOrEmpty(manifest.Dependencies.QuiltLoader))
        {
            ModloaderId = "quilt";
            ModloaderVersion = manifest.Dependencies.QuiltLoader;
        }
    }

    public async Task SetupAsync()
    {
        List<SerializedModification> mods = new();
        foreach (var file in manifest.Files)
        {
            string firstDownload = file.Downloads[0];
            bool canDeduceModInfos = firstDownload.StartsWith("https://cdn.modrinth.com");

            if (canDeduceModInfos)
            {
                string[] urlTokens = firstDownload
                    .Replace("https://", "")
                    .Split('/');

                if (urlTokens[1] == "data" && urlTokens[3] == "versions")
                {
                    string id = HttpUtility.UrlDecode(urlTokens[2]);
                    string version = HttpUtility.UrlDecode(urlTokens[4]);
                    Regex versionNumberRegex = new Regex("\\.|-");
                    Version? ver = null;

                    if (versionNumberRegex.IsMatch(version))
                    {
                        try
                        {
                            ver = await ModrinthModPlatform.Instance.Client.Version.GetByVersionNumberAsync(id, version);
                        }
                        catch (Exception e)
                        {
                            Version[] versions =
                                await ModrinthModPlatform.Instance.Client.Version.GetProjectVersionListAsync(id,
                                    gameVersions: new[] {MinecraftVersion}, loaders: new[] {ModloaderId});

                            ver = versions.FirstOrDefault(v => v.GameVersions.Contains(MinecraftVersion));

                            if (ver == null)
                            {
                                Debug.WriteLine($"Failed to download mod {file.Downloads[0]} {file.Path} : failed to find any version compatible with {MinecraftVersion}");
                                continue;
                            }
                        }

                        version = ver.Id;
                    }

                    if (ver == null)
                    {
                        ver = await ModrinthModPlatform.Instance.Client.Version.GetAsync(version);
                    }

                    Regex rootMcVerRegex = new Regex("\\d+\\.\\d+");
                    string rootMcVer = rootMcVerRegex.Match(MinecraftVersion).Value;

                    if (!ver.GameVersions.Contains(MinecraftVersion) && !ver.GameVersions.Contains(rootMcVer))
                    {
                        Version[] versions = await ModrinthModPlatform.Instance.Client.Version.GetProjectVersionListAsync(id,
                            gameVersions: new[] {MinecraftVersion}, loaders: new[] {ModloaderId});

                        ver = versions.FirstOrDefault(v => v.GameVersions.Contains(MinecraftVersion));

                        if (ver == null)
                        {
                            Debug.WriteLine($"Failed to download mod {file.Downloads[0]} {file.Path} : failed to find any version compatible with {MinecraftVersion}");
                            continue;
                        }
                    }

                    mods.Add(new SerializedModification
                    {
                        IsRequired = true,
                        ModId = id,
                        PlatformId = "modrinth",
                        VersionId = version
                    });

                    continue;
                }
            }

            // TODO: Do something when the mod isn't recognized
        }

        Modifications = mods.ToArray();

        AdditionalFiles = overridesEntries.Where(entry => entry.FullName.Contains('.')).Select(entry =>
            new AdditionalFile
            {
                Path = entry.FullName.Replace("overrides/", "").Replace("client-overrides/", ""),
                Data = entry.Open().ReadToEndAndClose(entry.Length),
            }).ToArray();
    }

    public override async Task InstallModificationAsync(Box targetBox, SerializedModification mod)
    {
        await ModrinthModPlatform.Instance.InstallModAsync(targetBox, new Modification
        {
            Id = mod.ModId,
            Platform = ModrinthModPlatform.Instance
        }, mod.VersionId, false);
    }

    public override Task ExportAsync(Box box, string filename)
    {
        throw new NotSupportedException();
    }

    public class ModelModrinthIndex
    {
        [JsonPropertyName("formatVersion")] public int FormatVersion { get; set; }
        [JsonPropertyName("game")] public string Game { get; set; }
        [JsonPropertyName("versionId")] public string VersionId { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("summary")] public string Summary { get; set; }
        [JsonPropertyName("files")] public ModelFile[] Files { get; set; }
        [JsonPropertyName("dependencies")] public ModelDependencies Dependencies { get; set; }

        public class ModelDependencies
        {
            [JsonPropertyName("minecraft")] public string Minecraft { get; set; }
            [JsonPropertyName("forge")] public string Forge { get; set; }
            [JsonPropertyName("fabric-loader")] public string FabricLoader { get; set; }
            [JsonPropertyName("quilt-loader")] public string QuiltLoader { get; set; }
        }

        public class ModelFile
        {
            [JsonPropertyName("path")] public string Path { get; set; }
            [JsonPropertyName("hashes")] public ModelHashes Hashes { get; set; }
            [JsonPropertyName("env")] public ModelEnv Env { get; set; }
            [JsonPropertyName("downloads")] public string[] Downloads { get; set; }
            [JsonPropertyName("fileSize")] public uint FileSize { get; set; }

            public class ModelHashes
            {
                [JsonPropertyName("sha1")] public string Sha1 { get; set; }
                [JsonPropertyName("sha512")] public string Sha512 { get; set; }
            }

            public class ModelEnv
            {
                [JsonPropertyName("server")] public string Server { get; set; }
                [JsonPropertyName("client")] public string Client { get; set; }
            }
        }
    }
}