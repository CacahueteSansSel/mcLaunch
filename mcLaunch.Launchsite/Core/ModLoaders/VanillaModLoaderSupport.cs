namespace mcLaunch.Launchsite.Core.ModLoaders;

public class VanillaModLoaderSupport : ModLoaderSupport
{
    public override string Id { get; } = "vanilla";
    public override string Name { get; set; } = "Vanilla";
    public override string Type { get; set; } = "vanilla";
    public override ModLoaderVersion LatestVersion { get; set; }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion)
    {
        return
        [
            new ModLoaderVersion
            {
                MinecraftVersion = minecraftVersion,
                Name = minecraftVersion
            }
        ];
    }
}