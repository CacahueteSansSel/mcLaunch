using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;

namespace mcLaunch.Core.Contents.Platforms;

public class MultiplexerMinecraftContentPlatform : MinecraftContentPlatform
{
    private readonly List<MinecraftContentPlatform> _platforms;

    public MultiplexerMinecraftContentPlatform(params MinecraftContentPlatform[] platforms)
    {
        _platforms = new List<MinecraftContentPlatform>(platforms);
    }

    public override string Name { get; } = "Multiplexer";

    public override async Task<PaginatedResponse<MinecraftContent>> GetContentsAsync(int page, Box box,
        string searchQuery, MinecraftContentType contentType)
    {
        List<MinecraftContent> contents = new();

        foreach (MinecraftContentPlatform platform in _platforms)
        {
            PaginatedResponse<MinecraftContent> modsFromPlatform =
                await platform.GetContentsAsync(page, box, searchQuery, contentType);

            foreach (MinecraftContent mod in modsFromPlatform.Items)
            {
                int similarModCount = contents.Count(m => m.IsSimilar(mod));

                // Avoid to add a mod that we have got from another platform
                // Ensure only one mod per search query
                if (similarModCount == 0) contents.Add(mod);
            }
        }

        return new PaginatedResponse<MinecraftContent>(page, contents.Count / 20, contents.ToArray());
    }

    public override async Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion)
    {
        List<PlatformModpack> modpacks = new();

        foreach (MinecraftContentPlatform platform in _platforms)
        {
            PaginatedResponse<PlatformModpack> modpacksFromPlatform =
                await platform.GetModpacksAsync(page, searchQuery, minecraftVersion);

            foreach (PlatformModpack mod in modpacksFromPlatform.Items)
            {
                int similarModCount = modpacks.Count(m => m.IsSimilar(mod));

                // Avoid to add a modpack that we have got from another platform
                // Ensure only one modpack per search query
                // Spoiler: it doesn't work at all
                if (similarModCount == 0) modpacks.Add(mod);
            }
        }

        return new PaginatedResponse<PlatformModpack>(page, modpacks.Count / 20, modpacks.ToArray());
    }

    public override async Task<PaginatedResponse<ContentDependency>> GetContentDependenciesAsync(string id,
        string modLoaderId,
        string versionId, string minecraftVersionId)
    {
        MinecraftContent mod = await GetContentAsync(id);
        if (mod == null) return null;

        return await mod.Platform.GetContentDependenciesAsync(id, modLoaderId, versionId, minecraftVersionId);
    }

    public override async Task<MinecraftContent> GetContentAsync(string id)
    {
        foreach (MinecraftContentPlatform platform in _platforms)
        {
            MinecraftContent content = await platform.GetContentAsync(id);
            if (content != null) return content;
        }

        return null;
    }

    public override Task<ContentVersion[]> GetContentVersionsAsync(MinecraftContent content, string? modLoaderId,
        string? minecraftVersionId)
    {
        return content.Platform!.GetContentVersionsAsync(content, modLoaderId, minecraftVersionId);
    }

    public override async Task<PlatformModpack> GetModpackAsync(string id)
    {
        foreach (MinecraftContentPlatform platform in _platforms)
        {
            PlatformModpack modpack = await platform.GetModpackAsync(id);
            if (modpack != null) return modpack;
        }

        return null;
    }

    public override async Task<bool> InstallContentAsync(Box targetBox, MinecraftContent content, string versionId,
        bool installOptional)
    {
        MinecraftContentPlatform? platform =
            content.Platform ?? _platforms.FirstOrDefault(p => p.Name == content.ModPlatformId);

        if (platform == null || !_platforms.Contains(platform)) return false;

        return await platform.InstallContentAsync(targetBox, content, versionId, installOptional);
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return null;
    }

    public override async Task<MinecraftContent> DownloadContentInfosAsync(MinecraftContent content)
    {
        MinecraftContentPlatform minecraftContentPlatform = content.Platform;

        if (!_platforms.Contains(minecraftContentPlatform)) return content;

        return await minecraftContentPlatform.DownloadContentInfosAsync(content);
    }

    public override MinecraftContentPlatform GetModPlatform(string id)
    {
        foreach (MinecraftContentPlatform platform in _platforms)
        {
            MinecraftContentPlatform p = platform.GetModPlatform(id);
            if (p != null) return p;
        }

        return null;
    }

    public override async Task<ContentVersion?> GetContentVersionFromData(Stream stream)
    {
        foreach (MinecraftContentPlatform platform in _platforms)
        {
            ContentVersion? ver = await platform.GetContentVersionFromData(stream);
            if (ver != null) return ver;
        }

        return null;
    }
}