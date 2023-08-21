using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Core.Managers;

public static class DefaultsManager
{
    public static string DefaultsPath { get; private set; }

    public static void Init()
    {
        DefaultsPath = "system/defaults";

        if (!Directory.Exists(DefaultsPath)) 
            Directory.CreateDirectory(DefaultsPath);
    }

    public static void SetDefaultMinecraftOptions(MinecraftOptions options)
    {
        options.SaveTo($"{DefaultsPath}/options.txt");
    }

    public static MinecraftOptions? LoadDefaultMinecraftOptions()
        => File.Exists($"{DefaultsPath}/options.txt") ? new MinecraftOptions($"{DefaultsPath}/options.txt") : null;

    public static void ClearDefaultOptions()
    {
        File.Delete($"{DefaultsPath}/options.txt");
    }
}