using mcLaunch.Core.Managers;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;

namespace mcLaunch.Core.Mods.Platforms;

public class MultiplexerModPlatform : ModPlatform
{
    List<ModPlatform> _platforms;

    public MultiplexerModPlatform(params ModPlatform[] platforms)
    {
        _platforms = new List<ModPlatform>(platforms);
    }

    public override string Name { get; } = "Multiplexer";

    public override async Task<PaginatedResponse<Modification>> GetModsAsync(int page, Box box, string searchQuery)
    {
        List<Modification> mods = new();

        foreach (ModPlatform platform in _platforms)
        {
            PaginatedResponse<Modification> modsFromPlatform = await platform.GetModsAsync(page, box, searchQuery);

            foreach (Modification mod in modsFromPlatform.Items)
            {
                int similarModCount = mods.Count(m => m.IsSimilar(mod));

                // Avoid to add a mod that we have got from another platform
                // Ensure only one mod per search query
                if (similarModCount == 0) mods.Add(mod);
            }
        }

        return new PaginatedResponse<Modification>(page, mods.Count / 20, mods.ToArray());
    }

    public override async Task<PaginatedResponse<PlatformModpack>> GetModpacksAsync(int page, string searchQuery,
        string minecraftVersion)
    {
        List<PlatformModpack> mods = new();

        foreach (ModPlatform platform in _platforms)
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

    public override async Task<PaginatedResponse<ModDependency>> GetModDependenciesAsync(string id, string modLoaderId,
        string versionId, string minecraftVersionId)
    {
        Modification mod = await GetModAsync(id);
        if (mod == null) return null;

        return await mod.Platform.GetModDependenciesAsync(id, modLoaderId, versionId, minecraftVersionId);
    }

    public override async Task<Modification> GetModAsync(string id)
    {
        foreach (ModPlatform platform in _platforms)
        {
            Modification mod = await platform.GetModAsync(id);
            if (mod != null) return mod;
        }

        return null;
    }

    public override Task<ModVersion[]> GetModVersionsAsync(Modification mod, string modLoaderId,
        string minecraftVersionId)
        => mod.Platform!.GetModVersionsAsync(mod, modLoaderId, minecraftVersionId);

    public override async Task<PlatformModpack> GetModpackAsync(string id)
    {
        foreach (ModPlatform platform in _platforms)
        {
            PlatformModpack modpack = await platform.GetModpackAsync(id);
            if (modpack != null) return modpack;
        }

        return null;
    }

    public override async Task<bool> InstallModAsync(Box targetBox, Modification mod, string versionId,
        bool installOptional)
    {
        ModPlatform? modPlatform = mod.Platform ?? _platforms.FirstOrDefault(p => p.Name == mod.ModPlatformId);

        if (modPlatform == null || !_platforms.Contains(modPlatform)) return false;

        return await modPlatform.InstallModAsync(targetBox, mod, versionId, installOptional);
    }

    public override async Task<ModificationPack> LoadModpackFileAsync(string filename)
    {
        return null;
    }

    public override async Task<Modification> DownloadModInfosAsync(Modification mod)
    {
        ModPlatform modPlatform = mod.Platform;

        if (!_platforms.Contains(modPlatform)) return mod;

        return await modPlatform.DownloadModInfosAsync(mod);
    }

    public override ModPlatform GetModPlatform(string id)
    {
        foreach (ModPlatform platform in _platforms)
        {
            ModPlatform p = platform.GetModPlatform(id);
            if (p != null) return p;
        }

        return null;
    }

    public override async Task<ModVersion?> GetModVersionFromData(Stream stream)
    {
        foreach (ModPlatform platform in _platforms)
        {
            ModVersion? ver = await platform.GetModVersionFromData(stream);
            if (ver != null) return ver;
        }

        return null;
    }
}