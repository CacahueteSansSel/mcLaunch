using Cacahuete.MinecraftLib.Core.ModLoaders;
using ddLaunch.Core.Boxes;

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
        
        // Forge
        // TODO: avoid to hardcode the jvm to use for Forge's installer
        All.Add(new ForgeModLoaderSupport(BoxManager.SystemFolder.GetJVM("java-runtime-gamma"), BoxManager.SystemFolder.CompletePath));
    }

    public static ModLoaderSupport? Get(string id) => All.FirstOrDefault(ml => ml.Id == id);
}