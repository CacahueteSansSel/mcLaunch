namespace mcLaunch.Core.Mods;

public class ModVersion
{
    public Modification Mod { get; set; }
    public string VersionId { get; set; }

    public ModVersion(Modification mod, string version)
    {
        Mod = mod;
        VersionId = version;
    }
}