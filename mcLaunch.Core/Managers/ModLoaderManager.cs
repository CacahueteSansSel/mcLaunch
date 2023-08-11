using Cacahuete.MinecraftLib.Core.ModLoaders;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Core.Managers;

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
        All.Add(new ForgeModLoaderSupport(
            BoxManager.JVMDownloader.GetJVMPath(Cacahuete.MinecraftLib.Core.Utilities.GetJavaPlatformIdentifier(),
                "java-runtime-gamma"), BoxManager.SystemFolder.CompletePath));

        // Quilt
        All.Add(new QuiltModLoaderSupport());
    }

    public static ModLoaderSupport? Get(string id) => All.FirstOrDefault(ml => ml.Id == id);
}