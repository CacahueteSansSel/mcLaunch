using mcLaunch.Core.Core;

namespace mcLaunch.Core.Mods;

public class ModVersion(Modification mod, string id, string name, string minecraftVersion) : IVersion
{
    public Modification Mod { get; set; } = mod;
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string ModLoader { get; set; }
    public string MinecraftVersion { get; set; } = minecraftVersion;
}