using System.Text.Json.Serialization;

namespace Cacahuete.MinecraftLib.Models.Fabric;

public class FabricLoaderManifest
{
    [JsonPropertyName("loader")] public ModelLoader Loader { get; set; }

    [JsonPropertyName("intermediary")] public ModelIntermediary Intermediary { get; set; }

    [JsonPropertyName("launcherMeta")] public ModelLauncherMeta LauncherMeta { get; set; }

    public class ModelLauncherMeta
    {
        [JsonPropertyName("version")] public int Version { get; set; }

        [JsonPropertyName("libraries")] public ModelLibraries Libraries { get; set; }

        public class ModelLibraries
        {
            [JsonPropertyName("client")] public ModelLibrary[] Client { get; set; }

            [JsonPropertyName("common")] public ModelLibrary[] Common { get; set; }

            [JsonPropertyName("server")] public ModelLibrary[] Server { get; set; }
        }

        public class ModelLibrary
        {
            [JsonPropertyName("name")] public string Name { get; set; }

            [JsonPropertyName("url")] public string Url { get; set; }
        }
    }

    public class ModelIntermediary
    {
        [JsonPropertyName("maven")] public string Maven { get; set; }

        [JsonPropertyName("version")] public string Version { get; set; }

        [JsonPropertyName("stable")] public bool Stable { get; set; }
    }

    public class ModelLoader
    {
        [JsonPropertyName("separator")] public string Separator { get; set; }

        [JsonPropertyName("build")] public int Build { get; set; }

        [JsonPropertyName("maven")] public string Maven { get; set; }

        [JsonPropertyName("version")] public string Version { get; set; }

        [JsonPropertyName("stable")] public bool Stable { get; set; }
    }
}