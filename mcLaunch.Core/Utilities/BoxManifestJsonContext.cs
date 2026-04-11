using System.Text.Json.Serialization;
using mcLaunch.Core.Boxes;
using mcLaunch.Launchsite.Core;

namespace mcLaunch.Core.Utilities;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BoxManifest))]
[JsonSerializable(typeof(BoxStoredContent))]
[JsonSerializable(typeof(BoxBackup))]
[JsonSerializable(typeof(CommandLineSettings))]
public partial class BoxManifestJsonContext : JsonSerializerContext
{
    
}