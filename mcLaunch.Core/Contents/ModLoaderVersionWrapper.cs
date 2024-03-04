using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Core.Core;

namespace mcLaunch.Core.Contents;

public class ModLoaderSupportWrapper : IVersionContent
{
    private readonly ModLoaderSupport modLoader;

    public ModLoaderSupportWrapper(ModLoaderSupport modLoader)
    {
        this.modLoader = modLoader;
    }

    public string Name => modLoader.Name;
    public IEnumerable<IVersion> ContentVersions { get; private set; }

    public async Task FetchVersionsAsync(string minecraftVersion)
    {
        ModLoaderVersion[]? versions = await modLoader.GetVersionsAsync(minecraftVersion);
        if (versions == null) return;

        ContentVersions = versions.Select(version => new ModLoaderVersionWrapper(version, modLoader.Name)).ToArray();
    }
}

public class ModLoaderVersionWrapper : IVersion
{
    private readonly ModLoaderVersion mlVersion;
    private string modLoaderName;

    public ModLoaderVersionWrapper(ModLoaderVersion mlVersion, string modLoaderName)
    {
        this.mlVersion = mlVersion;
        this.modLoaderName = modLoaderName;
    }

    public string Id => mlVersion.Name.ToLower();
    public string Name => mlVersion.Name;
    public string ModLoader { get; }
    public string MinecraftVersion => mlVersion.MinecraftVersion;
}