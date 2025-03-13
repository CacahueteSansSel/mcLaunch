using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using mcLaunch.Launchsite.Models;
using mcLaunch.Launchsite.Models.Forge.Installer;
using mcLaunch.Launchsite.Utils;

namespace mcLaunch.Launchsite.Core.ModLoaders.Forge;

public class ForgeInstallerFile : IDisposable
{
    public const string DefaultMavenRepositoryUrl = "https://libraries.minecraft.net/";
    private readonly ZipArchive _archive;

    public ForgeInstallerFile(string jarFilename)
    {
        _archive = new ZipArchive(new FileStream(jarFilename, FileMode.Open));

        string profileJson = _archive.ReadAllText("install_profile.json");
        ForgeInstallProfile? installProfile = JsonSerializer.Deserialize<ForgeInstallProfile>(profileJson,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
        if (installProfile == null) return;

        if (installProfile.IsV2) ParseProfileV2(installProfile);
        else ParseProfileV1(installProfile);
    }

    public string Name { get; private set; }
    public string MinecraftVersionId { get; private set; }
    public List<PostProcessor> Processors { get; } = [];
    public List<LibraryEntry> Libraries { get; } = [];
    public string? EmbeddedForgeJarPath { get; private set; }
    public LibraryName? EmbeddedForgeJarLibraryName { get; private set; }
    public MinecraftVersion Version { get; private set; }
    public Dictionary<string, string> DataVariables { get; } = [];
    public bool IsV2 { get; private set; }

    public void Dispose()
    {
        _archive.Dispose();
    }

    public void ExtractLocalLibrary(LibraryEntry library, string targetFilename)
    {
        if (!library.IsLocal) return;

        ExtractFile(library.ArtifactUrl, targetFilename);
    }

    public void ExtractFile(string filename, string targetFilename)
    {
        if (!HasFile(filename)) return;

        byte[] data = _archive.ReadAllBytes(filename);
        File.WriteAllBytes(targetFilename, data);
    }

    public bool HasFile(string filename) => _archive.GetEntry(filename) != null;

    public LibraryEntry? GetProcessorLibrary(PostProcessor processor)
    {
        foreach (LibraryEntry lib in Libraries)
        {
            if (lib.Name == processor.JarName)
                return lib;
        }

        return null;
    }

    private void ParseProfileV1(ForgeInstallProfile profile)
    {
        IsV2 = false;

        Name = profile.Install.Version.Replace("forge", "");
        MinecraftVersionId = profile.Install.Minecraft;
        Version = profile.VersionInfo;

        foreach (MinecraftVersion.ModelLibrary library in Version.Libraries)
        {
            if (string.IsNullOrEmpty(library.Url))
                library.Url = DefaultMavenRepositoryUrl;
        }

        EmbeddedForgeJarPath = profile.Install.FilePath;
        EmbeddedForgeJarLibraryName = new LibraryName(profile.Install.Path);
    }

    private void ParseProfileV2(ForgeInstallProfile profile)
    {
        IsV2 = true;

        Name = profile.Version;
        MinecraftVersionId = profile.Minecraft;
        Version = JsonSerializer.Deserialize<MinecraftVersion>(_archive
            .ReadAllText(profile.Json!.TrimStart('/')))!;

        foreach (ForgeInstallProfile.ModelProcessor? processor in profile.Processors)
        {
            if (processor.Sides != null && !processor.Sides.Contains("client")) continue;

            LibraryName jarName = new(processor.Jar);
            Processors.Add(new PostProcessor(jarName, processor.Classpath, processor.Args));
        }

        foreach (MinecraftVersion.ModelLibrary library in profile.Libraries)
        {
            LibraryName name = new(library.Name);
            bool isLocalLib = string.IsNullOrEmpty(library.Downloads.Artifact.Url);

            Libraries.Add(new LibraryEntry(name,
                library.Downloads.Artifact.Path,
                isLocalLib ? $"maven/{name.MavenFilename}" : library.Downloads.Artifact.Url,
                isLocalLib));
        }

        foreach (KeyValuePair<string, JsonNode?> kv in profile.Data!.AsObject())
        {
            JsonNode values = kv.Value!;
            string clientValue = values["client"]!.AsValue().GetValue<string>();

            DataVariables.Add(kv.Key, clientValue);
        }

        if (profile.Path != null)
        {
            LibraryName forgeName = new(profile.Path);
            EmbeddedForgeJarLibraryName = forgeName;
            EmbeddedForgeJarPath = $"maven/{forgeName.MavenFilename}";
        }
    }

    public record PostProcessor(LibraryName JarName, string[] Classpath, string[] Arguments);

    public record LibraryEntry(LibraryName Name, string ArtifactPath, string ArtifactUrl, bool IsLocal);
}