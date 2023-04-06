using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cacahuete.MinecraftLib.Core;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Utils;

namespace Cacahuete.MinecraftLib.Models;

public class MinecraftVersion
{
    [JsonPropertyName("arguments")] public ModelArguments? Arguments { get; set; }

    [JsonPropertyName("assetIndex")] public ModelAssetIndex AssetIndex { get; set; }

    [JsonPropertyName("downloads")] public ModelDownloads Downloads { get; set; }

    [JsonPropertyName("javaVersion")] public ModelJavaVersion JavaVersion { get; set; }

    [JsonPropertyName("libraries")] public ModelLibrary[] Libraries { get; set; }

    [JsonPropertyName("logging")] public ModelLogging Logging { get; set; }

    [JsonPropertyName("assets")] public string Assets { get; set; }

    [JsonPropertyName("complianceLevel")] public int? ComplianceLevel { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("time")] public DateTime? Time { get; set; }

    [JsonPropertyName("releaseTime")] public DateTime? ReleaseTime { get; set; }

    [JsonPropertyName("minimumLauncherVersion")]
    public int? MinimumLauncherVersion { get; set; }

    [JsonPropertyName("mainClass")] public string MainClass { get; set; }
    
    // Needed to specify a custom client url (used by modloaders for example)
    [JsonIgnore] public string? CustomClientUrl { get; set; }

    public async Task InstallToAsync(string targetDirectoryPath, bool force = false)
    {
        if (!Directory.Exists(targetDirectoryPath)) Directory.CreateDirectory(targetDirectoryPath);
        if (File.Exists($"{targetDirectoryPath}/{Id}.json") && File.Exists($"{targetDirectoryPath}/{Id}.jar"))
            return;

        await File.WriteAllTextAsync($"{targetDirectoryPath}/{Id}.json", JsonSerializer.Serialize(this));
        await Context.Downloader.DownloadAsync(CustomClientUrl ?? Downloads.Client.Url, $"{targetDirectoryPath}/{Id}.jar");
    }

    public Task<AssetIndex?> GetAssetIndexAsync() => Api.GetAsync<AssetIndex>(AssetIndex.Url);

    /// <summary>
    /// Merges the two <see cref="MinecraftVersion"/> and gives the priority to the callee for non-list elements that cannot
    /// be merged. If this <see cref="MinecraftVersion"/> have null fields, they will be set to the other's fields value instead
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public MinecraftVersion Merge(MinecraftVersion other)
    {
        return new MinecraftVersion
        {
            Arguments = new ModelArguments
            {
                Game = new List<object>((Arguments ?? ModelArguments.Default).Game).AddsOnce(other.Arguments?.Game).ToArray(),
                JVM = new List<object>((Arguments ?? ModelArguments.Default).JVM).AddsOnce(other.Arguments?.JVM).ToArray()
            },
            AssetIndex = this.AssetIndex ?? other.AssetIndex,
            Assets = this.Assets ?? other.Assets,
            ComplianceLevel = this.ComplianceLevel,
            Downloads = this.Downloads ?? other.Downloads,
            Id = this.Id ?? other.Id,
            JavaVersion = this.JavaVersion ?? other.JavaVersion,
            Libraries = new List<ModelLibrary>(this.Libraries).Adds(other.Libraries).ToArray(),
            Logging = this.Logging ?? other.Logging,
            MainClass = this.MainClass ?? other.MainClass,
            MinimumLauncherVersion = this.MinimumLauncherVersion ?? other.MinimumLauncherVersion,
            Type = this.Type ?? other.Type,
            Time = this.Time ?? other.Time,
            ReleaseTime = this.ReleaseTime ?? other.ReleaseTime
        };
    }

    public class ModelLogging
    {
        [JsonPropertyName("client")] public ModelEntry Client { get; set; }

        public class ModelEntry
        {
            [JsonPropertyName("argument")] public string Argument { get; set; }

            [JsonPropertyName("file")] public ModelFile File { get; set; }

            [JsonPropertyName("type")] public string Type { get; set; }

            public class ModelFile
            {
                [JsonPropertyName("id")] public string Id { get; set; }

                [JsonPropertyName("sha1")] public string Hash { get; set; }

                [JsonPropertyName("size")] public ulong Size { get; set; }

                [JsonPropertyName("url")] public string Url { get; set; }
            }
        }
    }

    public class ModelLibrary
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("downloads")] public ModelDownloads Downloads { get; set; }

        [JsonPropertyName("natives")] public ModelNatives Natives { get; set; }

        [JsonPropertyName("rules")] public ModelRule[] Rules { get; set; }

        // Useful for Fabric
        [JsonPropertyName("url")] public string Url { get; set; }

        public bool NeedsToDeduceUrlFromName => Name.Contains('.')
                                                && Name.Contains(':')
                                                && !string.IsNullOrEmpty(Url);

        public string GetFinalJarFilename() => new LibraryName(Name).JarFilename;

        public string? DeduceUrl()
        {
            if (!NeedsToDeduceUrlFromName) return null;

            LibraryName libName = new LibraryName(Name);

            string url = libName.BuildMavenUrl(Url);

            return url;
        }

        public class ModelNatives
        {
            [JsonPropertyName("linux")] public string Linux { get; set; }
            [JsonPropertyName("windows")] public string Windows { get; set; }
            [JsonPropertyName("macos")] public string MacOS { get; set; }
        }

        public class ModelRule
        {
            [JsonPropertyName("action")] public string Action { get; set; }

            [JsonPropertyName("os")] public ModelOS Os { get; set; }

            public class ModelOS
            {
                [JsonPropertyName("name")] public string Name { get; set; }
                [JsonPropertyName("arch")] public string Arch { get; set; }

                [JsonIgnore]
                public bool IsSatisfied => (Name == null || Name == Utilities.GetPlatformIdentifier())
                                           || (Arch == null || Arch == Utilities.GetArchitecture());
            }
        }

        public class ModelDownloads
        {
            [JsonPropertyName("artifact")] public ModelArtifact Artifact { get; set; }
            [JsonPropertyName("classifiers")] public ModelClassifiers Classifiers { get; set; }

            public class ModelArtifact
            {
                [JsonPropertyName("path")] public string Path { get; set; }

                [JsonPropertyName("sha1")] public string Hash { get; set; }

                [JsonPropertyName("size")] public ulong Size { get; set; }

                [JsonPropertyName("url")] public string Url { get; set; }
            }

            public class ModelClassifiers
            {
                [JsonPropertyName("natives-linux")] public ModelArtifact NativesLinux { get; set; }

                [JsonPropertyName("natives-macos")] public ModelArtifact NativesMacOS { get; set; }

                [JsonPropertyName("natives-windows")] public ModelArtifact NativesWindows { get; set; }

                [JsonPropertyName("natives-sources")] public ModelArtifact Sources { get; set; }

                [JsonPropertyName("natives-javadoc")] public ModelArtifact Javadoc { get; set; }
            }
        }
    }

    public class ModelJavaVersion
    {
        [JsonPropertyName("component")] public string Component { get; set; }

        [JsonPropertyName("majorVersion")] public int MajorVersion { get; set; }
    }

    public class ModelDownloads
    {
        [JsonPropertyName("client")] public Download Client { get; set; }

        [JsonPropertyName("client_mappings")] public Download ClientMappings { get; set; }

        [JsonPropertyName("server")] public Download Server { get; set; }

        [JsonPropertyName("server_mappings")] public Download ServerMappings { get; set; }

        public class Download
        {
            [JsonPropertyName("sha1")] public string Hash { get; set; }

            [JsonPropertyName("size")] public ulong Size { get; set; }

            [JsonPropertyName("url")] public string Url { get; set; }
        }
    }

    public class ModelArguments
    {
        [JsonPropertyName("game")] public object[]? Game { get; set; }

        [JsonPropertyName("jvm")] public object[]? JVM { get; set; }

        bool RuleSatisfied(JsonElement ruleJson)
        {
            if (!ruleJson.TryGetProperty("action", out JsonElement actionJson)) return false;
            if (actionJson.GetString() != "allow")
            {
                Console.WriteLine($"Rule not implemented: {actionJson.GetString()}");
                return false;
            }

            if (ruleJson.TryGetProperty("os", out JsonElement osJson))
            {
                if (osJson.TryGetProperty("name", out JsonElement osNameJson)
                    && osNameJson.GetString() != Utilities.GetPlatformIdentifier())
                {
                    return false;
                }

                if (osJson.TryGetProperty("arch", out JsonElement osArchJson)
                    && osArchJson.GetString() != Utilities.GetArchitecture())
                {
                    return false;
                }
            }

            return true;
        }

        string FormatArgument(string? arg, Dictionary<string, string> replacements)
        {
            if (arg == null) return "\"\"";

            bool ignoreWhitespaces = false;

            if (arg.Contains('$'))
            {
                foreach (var kv in replacements)
                {
                    string toReplace = $"${{{kv.Key}}}";
                    string value = kv.Value ?? "";
                    if (arg.Contains(toReplace) && value.Contains(' '))
                    {
                        value = $"\"{value}\"";
                        ignoreWhitespaces = true;
                    }

                    string newValue = string.IsNullOrEmpty(value) ? "\"\"" : value;
                    arg = arg.Replace(toReplace, newValue);
                }
            }

            if ((arg.Contains(" ") && !ignoreWhitespaces) || string.IsNullOrWhiteSpace(arg)) arg = $"\"{arg}\"";

            return arg;
        }

        public string Build(Dictionary<string, string> replacements, string middle = "")
        {
            string final = "";
            List<string> processedArgs = new();

            foreach (object arg in JVM)
            {
                if (arg is not JsonElement elmt) continue;

                if (elmt.ValueKind == JsonValueKind.String)
                {
                    final += FormatArgument(elmt.GetString(), replacements) + " ";
                }

                if (elmt.ValueKind == JsonValueKind.Object
                    && elmt.TryGetProperty("rules", out JsonElement rulesJson)
                    && elmt.TryGetProperty("value", out JsonElement valueJson))
                {
                    bool abort = false;

                    foreach (JsonElement rule in rulesJson.EnumerateArray())
                    {
                        if (!RuleSatisfied(rule))
                        {
                            abort = true;
                            break;
                        }
                    }

                    if (abort) continue;

                    if (valueJson.ValueKind == JsonValueKind.String)
                    {
                        if (processedArgs.Contains(valueJson.GetString())) continue;
                        processedArgs.Add(valueJson.GetString());
                        
                        final += FormatArgument(valueJson.GetString(), replacements) + " ";
                        continue;
                    }

                    foreach (JsonElement value in valueJson.EnumerateArray())
                    {
                        if (processedArgs.Contains(value.GetString())) continue;
                        processedArgs.Add(value.GetString());
                        
                        final += FormatArgument(value.GetString(), replacements) + " ";
                    }
                }
            }

            final += middle + " ";

            foreach (object arg in Game)
            {
                if (arg is not JsonElement elmt) continue;
                if (elmt.ValueKind != JsonValueKind.String) continue;
                
                if (processedArgs.Contains(elmt.GetString())) continue;
                processedArgs.Add(elmt.GetString());

                final += FormatArgument(elmt.GetString(), replacements) + " ";
            }

            return final.Trim();
        }

        public static ModelArguments Default { get; } =
            JsonSerializer.Deserialize<ModelArguments>(File.ReadAllText("system/default_args.json"))!;
    }

    public class ModelAssetIndex
    {
        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("sha1")] public string Hash { get; set; }

        [JsonPropertyName("size")] public ulong Size { get; set; }

        [JsonPropertyName("totalSize")] public ulong TotalSize { get; set; }

        [JsonPropertyName("url")] public string Url { get; set; }
    }
}