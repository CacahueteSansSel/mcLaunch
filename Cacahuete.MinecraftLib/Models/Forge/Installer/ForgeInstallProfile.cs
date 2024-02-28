using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.Forge.Installer;

public class ForgeInstallProfile
{
    [JsonIgnore] public bool IsV2 => Json != null;
    
    public int? Spec { get; set; }
    public string Profile { get; set; }
    public string Version { get; set; }
    public string? Path { get; set; }
    public string Minecraft { get; set; }
    [JsonPropertyName("serverJarPath")]
    public string? ServerJarPath { get; set; }
    public ModelProcessor[] Processors { get; set; }
    public MinecraftVersion.ModelLibrary[] Libraries { get; set; }
    public string? Icon { get; set; }
    public string? Json { get; set; }
    public string? Logo { get; set; }
    [JsonPropertyName("mirrorList")]
    public string? MirrorList { get; set; }
    public JsonNode? Data { get; set; }
    
    public ModelOldInstall Install { get; set; }
    [JsonPropertyName("versionInfo")]
    public MinecraftVersion VersionInfo { get; set; }

    public class ModelProcessor
    {
        public string[]? Sides { get; set; }
        public string Jar { get; set; }
        public string[] Classpath { get; set; }
        public string[] Args { get; set; }
    }

    public class ModelDataEntry
    {
        public string Client { get; set; }
        public string Server { get; set; }
    }

    public class ModelOldInstall
    {
        [JsonPropertyName("profileName")]
        public string ProfileName { get; set; }
        public string Target { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        [JsonPropertyName("filePath")]
        public string FilePath { get; set; }
        public string Minecraft { get; set; }
        [JsonPropertyName("mirrorList")]
        public string MirrorList { get; set; }
        public string Logo { get; set; }
        [JsonPropertyName("modList")]
        public string ModList { get; set; }
    }
}