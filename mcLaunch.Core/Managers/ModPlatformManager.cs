using mcLaunch.Core.Mods;

namespace mcLaunch.Core.Managers;

public static class ModPlatformManager
{
    public static ModPlatform Platform { get; private set; }

    public static void Init(ModPlatform platform)
    {
        Platform = platform;
    }
}