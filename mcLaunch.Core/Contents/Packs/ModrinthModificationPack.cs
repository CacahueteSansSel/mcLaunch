using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Utilities;
using mcLaunch.Launchsite.Core.ModLoaders;
using File = Modrinth.Models.File;
using Version = Modrinth.Models.Version;

namespace mcLaunch.Core.Contents.Packs;

public class ModrinthModificationPack : ModificationPack
{
    private readonly ModelModrinthIndex manifest;
    private readonly ZipArchiveEntry[] overridesEntries;

    public ModrinthModificationPack()
    {
    }

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
        }
        else if (!string.IsNullOrEmpty(manifest.Dependencies.Forge))
        {
            ModloaderId = "forge";
            ModloaderVersion = manifest.Dependencies.Forge;
        }
        else if (!string.IsNullOrEmpty(manifest.Dependencies.QuiltLoader))
        {
            ModloaderId = "quilt";
            ModloaderVersion = manifest.Dependencies.QuiltLoader;
        }
    }

    public override string Name { get; init; }
    public override string Author { get; init; }
    public override string Version { get; init; }
    public override string? Id { get; init; }
    public override string? Description { get; init; }
    public override string MinecraftVersion { get; init; }
    public override string ModloaderId { get; init; }
    public override string ModloaderVersion { get; init; }
    public override SerializedMinecraftContent[] Modifications { get; set; }
    public override AdditionalFile[] AdditionalFiles { get; set; }

    public async Task<ModrinthModificationPack> SetupAsync()
    {
        List<SerializedMinecraftContent> mods = new();
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
                            ver = await ModrinthMinecraftContentPlatform.Instance.Client.Version
                                .GetByVersionNumberAsync(id, version);
                        }
                        catch (Exception e)
                        {
                            Version[] versions =
                                await ModrinthMinecraftContentPlatform.Instance.Client.Version
                                    .GetProjectVersionListAsync(id,
                                        gameVersions: new[] {MinecraftVersion}, loaders: new[] {ModloaderId});

                            ver = versions.FirstOrDefault(v => v.GameVersions.Contains(MinecraftVersion));

                            if (ver == null)
                            {
                                Debug.WriteLine(
                                    $"Failed to download mod {file.Downloads[0]} {file.Path} : failed to find any version compatible with {MinecraftVersion}");
                                continue;
                            }
                        }

                        version = ver.Id;
                    }

                    if (ver == null)
                        try
                        {
                            ver = await ModrinthMinecraftContentPlatform.Instance.Client.Version.GetAsync(version);
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                ContentVersion[] versions =
                                    await ModrinthMinecraftContentPlatform.Instance.GetContentVersionsAsync(
                                        MinecraftContent.CreateIdOnly(id), ModloaderId, MinecraftVersion);

                                ver = await ModrinthMinecraftContentPlatform.Instance.Client.Version.GetAsync(
                                    versions[0].Id);
                            }
                            catch
                            {
                                // The mod failed to download
                                continue;
                            }
                        }

                    Regex rootMcVerRegex = new Regex("\\d+\\.\\d+");
                    string rootMcVer = rootMcVerRegex.Match(MinecraftVersion).Value;

                    if (!ver.GameVersions.Contains(MinecraftVersion) && !ver.GameVersions.Contains(rootMcVer))
                    {
                        Version[] versions = await ModrinthMinecraftContentPlatform.Instance.Client.Version
                            .GetProjectVersionListAsync(id,
                                gameVersions: new[] {MinecraftVersion}, loaders: new[] {ModloaderId});

                        ver = versions.FirstOrDefault(v => v.GameVersions.Contains(MinecraftVersion));

                        if (ver == null)
                        {
                            Debug.WriteLine(
                                $"Failed to download mod {file.Downloads[0]} {file.Path} : failed to find any version compatible with {MinecraftVersion}");
                            continue;
                        }
                    }

                    mods.Add(new SerializedMinecraftContent
                    {
                        IsRequired = true,
                        ModId = id,
                        PlatformId = "modrinth",
                        VersionId = version
                    });
                }
            }

            // TODO: Do something when the mod isn't recognized
        }

        Modifications = mods.ToArray();

        AdditionalFiles = overridesEntries.Where(entry => entry.FullName.Contains('.')).Select(entry =>
            new AdditionalFile
            {
                Path = entry.FullName.Replace("overrides/", "").Replace("client-overrides/", ""),
                Data = entry.Open().ReadToEndAndClose(entry.Length)
            }).ToArray();

        return this;
    }

    public override async Task InstallModificationAsync(Box targetBox, SerializedMinecraftContent mod)
    {
        MinecraftContent content = await ModrinthMinecraftContentPlatform.Instance.GetContentAsync(mod.ModId);
        if (content == null) return;

        await ModrinthMinecraftContentPlatform.Instance.InstallContentAsync(targetBox, new MinecraftContent
        {
            Id = mod.ModId,
            Platform = ModrinthMinecraftContentPlatform.Instance,
            Type = content.Type
        }, mod.VersionId, false, false);
    }

    public override async Task ExportAsync(Box box, string filename, string[]? includedFiles)
    {
        using FileStream fs = new(filename, FileMode.Create);
        using ZipArchive zip = new(fs, ZipArchiveMode.Create);

        ModelModrinthIndex index = new()
        {
            FormatVersion = 1,
            Game = "minecraft",
            VersionId = "1.0.0",
            Name = box.Manifest.Name,
            Summary = box.Manifest.Description
        };

        if (box.ModLoader is ForgeModLoaderSupport)
            index.Dependencies = new ModelModrinthIndex.ModelDependencies
            {
                Forge = box.Manifest.ModLoaderVersion
            };
        else if (box.ModLoader is FabricModLoaderSupport)
            index.Dependencies = new ModelModrinthIndex.ModelDependencies
            {
                FabricLoader = box.Manifest.ModLoaderVersion
            };
        else if (box.ModLoader is QuiltModLoaderSupport)
            index.Dependencies = new ModelModrinthIndex.ModelDependencies
            {
                QuiltLoader = box.Manifest.ModLoaderVersion
            };
        else
            index.Dependencies = new ModelModrinthIndex.ModelDependencies();

        index.Dependencies.Minecraft = box.Manifest.Version;

        List<ModelModrinthIndex.ModelFile> files = new();
        foreach (BoxStoredContent mod in box.Manifest.Contents)
        {
            if (mod.PlatformId.ToLower() != "modrinth")
            {
                // Include any non-Modrinth mod to the overrides
                ZipArchiveEntry overrideEntry = zip.CreateEntry($"overrides/{mod.Filenames[0]}");
                await using Stream entryStream = overrideEntry.Open();
                using FileStream modFileStream =
                    new FileStream(box.Folder.CompletePath + $"/{mod.Filenames[0]}", FileMode.Open);

                await modFileStream.CopyToAsync(entryStream);

                continue;
            }

            Version modVersion = await ModrinthMinecraftContentPlatform.Instance.Client.Version.GetAsync(mod.VersionId);
            File? primaryVersionFile = modVersion.Files.FirstOrDefault(f => f.Primary);

            if (primaryVersionFile == null)
            {
                // TODO: inform user that this mod was ignored
                continue;
            }

            ModelModrinthIndex.ModelFile fileModel = new()
            {
                Path = mod.Filenames[0],
                Downloads = [primaryVersionFile.Url],
                Hashes = new ModelModrinthIndex.ModelFile.ModelHashes
                {
                    Sha1 = primaryVersionFile.Hashes.Sha1,
                    Sha512 = primaryVersionFile.Hashes.Sha512
                },
                FileSize = (uint) primaryVersionFile.Size
            };

            files.Add(fileModel);
        }

        index.Files = files.ToArray();

        if (includedFiles != null)
        {
            foreach (string file in includedFiles)
            {
                string completePath = $"{box.Path}/minecraft/{file}";
                if (!System.IO.File.Exists(completePath)) continue;

                ZipArchiveEntry overrideEntry = zip.CreateEntry($"overrides/{file}");
                await using Stream entryStream = overrideEntry.Open();
                using FileStream modFileStream = new FileStream(completePath, FileMode.Open);

                await modFileStream.CopyToAsync(entryStream);
            }
        }

        foreach (string modFile in box.GetUnlistedMods())
        {
            string completePath = $"{box.Path}/minecraft/{modFile}";
            if (!System.IO.File.Exists(completePath)) continue;

            ZipArchiveEntry overrideEntry = zip.CreateEntry($"overrides/{modFile}");
            await using Stream entryStream = overrideEntry.Open();
            using FileStream modFileStream = new FileStream(completePath, FileMode.Open);

            await modFileStream.CopyToAsync(entryStream);
        }

        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        ZipArchiveEntry entry = zip.CreateEntry("modrinth.index.json");
        await using Stream manifestStream = entry.Open();
        await JsonSerializer.SerializeAsync(manifestStream, index, options);
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