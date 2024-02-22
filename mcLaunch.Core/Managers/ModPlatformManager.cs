using mcLaunch.Core.Contents;

namespace mcLaunch.Core.Managers;

public static class ModPlatformManager
{
    public static MinecraftContentPlatform Platform { get; private set; }

    public static void Init(MinecraftContentPlatform platform)
    {
        Platform = platform;
    }
}