namespace mcLaunch.Core.Core;

public interface IVersionContent
{
    public string Name { get; }
    public IEnumerable<IVersion> ContentVersions { get; }
}

public interface IVersion
{
    public string Id { get; }
    public string Name { get; }
    public string ModLoader { get; }
    public string MinecraftVersion { get; }
}