using mcLaunch.Core.Core;

namespace mcLaunch.Core.Contents;

public class ModVersionContentProvider(ContentVersion[] versions, MinecraftContent mod) : IVersionContent
{
    public string Name => mod.Name;
    public IEnumerable<IVersion> ContentVersions => versions;
}