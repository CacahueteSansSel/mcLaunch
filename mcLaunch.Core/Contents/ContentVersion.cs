using mcLaunch.Core.Core;

namespace mcLaunch.Core.Contents;

public class ContentVersion(
    MinecraftContent content,
    string id,
    string name,
    string minecraftVersion,
    string? modLoader) : IVersion
{
    public MinecraftContent Content { get; set; } = content;
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string? ModLoader { get; set; } = modLoader;
    public string MinecraftVersion { get; set; } = minecraftVersion;
}