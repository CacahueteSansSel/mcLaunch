using mcLaunch.Core.Core;

namespace mcLaunch.Core.Mods;

public class ModVersionContentProvider(ModVersion[] versions, Modification mod) : IVersionContent
{
    public string Name => mod.Name;
    public IEnumerable<IVersion> ContentVersions => versions;
}