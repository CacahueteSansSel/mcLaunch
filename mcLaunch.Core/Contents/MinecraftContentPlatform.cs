using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;

namespace mcLaunch.Core.Contents;

public abstract class MinecraftContentPlatform
{
    public enum DependencyRelationType
    {
        Required,
        Optional,
        Incompatible,
        Unknown
    }

    public abstract string Name { get; }
    public Bitmap Icon { get; private set; }

    public abstract Task<PaginatedResponse<MinecraftContent>> GetContentsAsync(int page, Box box, string searchQuery,
        MinecraftContentType contentType);

    public abstract Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion);

    public abstract Task<PaginatedResponse<ContentDependency>> GetContentDependenciesAsync(string id,
        string modLoaderId, string versionId, string minecraftVersionId);

    public abstract Task<MinecraftContent> GetContentAsync(string id);

    public abstract Task<ContentVersion[]> GetContentVersionsAsync(MinecraftContent content, string? modLoaderId,
        string? minecraftVersionId);

    public abstract Task<PlatformModpack> GetModpackAsync(string id);

    public abstract Task<bool> InstallContentAsync(Box targetBox, MinecraftContent content, string versionId,
        bool installOptional);

    public abstract Task<ModificationPack> LoadModpackFileAsync(string filename);

    public abstract Task<MinecraftContent> DownloadContentInfosAsync(MinecraftContent content);
    public abstract MinecraftContentPlatform GetModPlatform(string id);
    public abstract Task<ContentVersion?> GetContentVersionFromData(Stream stream);

    public MinecraftContentPlatform WithIcon(string name)
    {
        Icon = new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{name}.png")));

        return this;
    }

    public class ContentDependency
    {
        public MinecraftContent Content { get; init; }
        public string VersionId { get; init; }
        public DependencyRelationType Type { get; init; }
    }
}