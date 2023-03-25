using Cacahuete.MinecraftLib.Core.ModLoaders;

namespace ddLaunch.Core.Managers;

public static class ModLoaderManager
{
    public static List<ModLoaderSupport> All { get; } = new();

    public static void Init()
    {
        // Vanilla (unmodded original Minecraft)
        All.Add(new VanillaModLoaderSupport());
        
        // Fabric
        All.Add(new FabricModLoaderSupport());
    }

    public static ModLoaderSupport? Get(string id) => All.FirstOrDefault(ml => ml.Id == id);
}