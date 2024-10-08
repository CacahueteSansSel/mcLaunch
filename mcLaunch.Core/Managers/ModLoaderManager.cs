﻿using mcLaunch.Core.Boxes;
using mcLaunch.Launchsite.Core.ModLoaders;

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
            BoxManager.JvmDownloader.GetAndPrepareJvmExecPath(
                Launchsite.Core.Utilities.GetJavaPlatformIdentifier(),
                "java-runtime-gamma"), BoxManager.SystemFolder.CompletePath));

        // NeoForge
        // TODO: avoid to hardcode the jvm to use for Forge's installer
        All.Add(new NeoForgeModLoaderSupport(
            BoxManager.JvmDownloader.GetAndPrepareJvmExecPath(
                Launchsite.Core.Utilities.GetJavaPlatformIdentifier(),
                "java-runtime-gamma"), BoxManager.SystemFolder.CompletePath));

        // Quilt
        All.Add(new QuiltModLoaderSupport());

        // Babric (Fabric for Minecraft b1.7.3)
        All.Add(new BabricModLoaderSupport());

        // Direct Jar Merging
        All.Add(new DirectJarMergingModLoaderSupport());
    }

    public static bool IsModLoaderName(string name)
    {
        return All.Any(ml => string.Equals(ml.Id, name, StringComparison.CurrentCultureIgnoreCase)
                             || ml.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public static ModLoaderSupport? Get(string id)
    {
        return All.FirstOrDefault(ml => ml.Id == id);
    }
}