using ddLaunch.Core.Boxes;

namespace ddLaunch.Core.Mods.Platforms;

public class MultiplexerModPlatform : ModPlatform
{
    List<ModPlatform> _platforms;

    public MultiplexerModPlatform(params ModPlatform[] platforms)
    {
        _platforms = new List<ModPlatform>(platforms);
    }

    public override string Name { get; } = "Multiplexer";
    
    public override async Task<Modification[]> GetModsAsync(int page, Box box, string searchQuery)
    {
        List<Modification> mods = new();

        foreach (ModPlatform platform in _platforms)
        {
            Modification[] modsFromPlatform = await platform.GetModsAsync(page, box, searchQuery);

            foreach (Modification mod in modsFromPlatform)
            {
                int similarModCount = mods.Count(m => m.IsSimilar(mod));
                
                // Avoid to add a mod that we have got from another platform
                // Ensure only one mod per search query
                if (similarModCount == 0) mods.Add(mod);
            }
        }
        
        return mods.ToArray();
    }

    public override async Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId)
    {
        ModPlatform modPlatform = mod.Platform;

        if (!_platforms.Contains(modPlatform)) return false;

        return await modPlatform.InstallModificationAsync(targetBox, mod, versionId);
    }

    public override async Task<Modification> DownloadAdditionalInfosAsync(Modification mod)
    {
        ModPlatform modPlatform = mod.Platform;

        if (!_platforms.Contains(modPlatform)) return mod;

        return await modPlatform.DownloadAdditionalInfosAsync(mod);
    }
}