using mcLaunch.Core.Managers;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;

namespace mcLaunch.Core.Contents.Platforms;

public class MultiplexerMinecraftContentPlatform : MinecraftContentPlatform
{
    List<MinecraftContentPlatform> _platforms;

    public MultiplexerMinecraftContentPlatform(params MinecraftContentPlatform[] platforms)
    {
        _platforms = new List<MinecraftContentPlatform>(platforms);
    }

    public override string Name { get; } = "Multiplexer";

    public override async Task<PaginatedResponse<MinecraftContent>> GetContentsAsync(int page, Box box, string searchQuery)
    {
        List<MinecraftContent> mods = new();

        foreach (MinecraftContentPlatform platform in _platforms)
        {
            PaginatedResponse<MinecraftContent> modsFromPlatform = await platform.GetContentsAsync(page, box, searchQuery);

            foreach (MinecraftContent mod in modsFromPlatform.Items)
            {
                int similarModCount = mods.Count(m => m.IsSimilar(mod));

                // Avoid to add a mod that we have got from another platform
                // Ensure only one mod per search query
                if (similarModCount == 0) mods.Add(mod);
            }
        }

        return new PaginatedResponse<MinecraftContent>(page, mods.Count / 20, mods.ToArray());
    }

    public override async Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion)
    {
        List<PlatformModpack> mods = new();

        foreach (MinecraftContentPlatform platform in _platforms)
        {
            PaginatedResponse<PlatformModpack> modpacksFromPlatform =
                await platform.GetModpacksAsync(page, searchQuery, minecraftVersion);

            foreach (PlatformModpack mod in modpacksFromPlatform.Items)
            {
                int similarModCount = mods.Count(m => m.IsSimilar(mod));

                // Avoid to add a modpack that we have got from another platform
                // Ensure only one modpack per search query
                // Spoiler: it doesn't work at all
                if (similarModCount == 0) mods.Add(mod);
            }
        }

        return new PaginatedResponse<PlatformModpack>(page, mods.Count / 20, mods.ToArray());
    }

    public override async Task<PaginatedResponse<ContentDependency>> GetContentDependenciesAsync(string id, string modLoaderId,
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
            MinecraftContent mod = await platform.GetContentAsync(id);
            if (mod != null) return mod;
        }

        return null;
    }

    public override Task<ContentVersion[]> GetContentVersionsAsync(MinecraftContent content, string? modLoaderId,
        string? minecraftVersionId)
        => content.Platform!.GetContentVersionsAsync(content, modLoaderId, minecraftVersionId);

    public override async Task<PlatformModpack> GetModpackAsync(string id)
    {
        foreach (MinecraftContentPlatform platform in _platforms)
        {
            PlatformModpack modpack = await platform.GetModpackAsync(id);
            if (modpack != null) return modpack;
        }

        return null;
    }

    public override async Task<bool> InstallContentAsync(Box targetBox, MinecraftContent mod, string versionId,
        bool installOptional)
    {
        MinecraftContentPlatform? modPlatform = mod.Platform ?? _platforms.FirstOrDefault(p => p.Name == mod.ModPlatformId);

        if (modPlatform == null || !_platforms.Contains(modPlatform)) return false;

        return await modPlatform.InstallContentAsync(targetBox, mod, versionId, installOptional);
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return null;
    }

    public override async Task<MinecraftContent> DownloadContentInfosAsync(MinecraftContent mod)
    {
        MinecraftContentPlatform minecraftContentPlatform = mod.Platform;

        if (!_platforms.Contains(minecraftContentPlatform)) return mod;

        return await minecraftContentPlatform.DownloadContentInfosAsync(mod);
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