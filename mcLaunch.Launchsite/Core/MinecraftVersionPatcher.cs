using System.Runtime.InteropServices;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core;

public static class MinecraftVersionPatcher
{
    public static void Patch(MinecraftVersion version)
    {
        TryPatchCase1(version);
    }

    // Case 1 => Any version that targets lwjgl-platform:2.9.4-nightly-20150209 on macOS ARM64
    private static void TryPatchCase1(MinecraftVersion version)
    {
        if (!OperatingSystem.IsMacOS() || RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            return;

        const string lwjglVersion = "2.9.4-nightly-20150209";
        const string lwjgl = "org.lwjgl.lwjgl:lwjgl:" + lwjglVersion;
        const string lwjglUtil = "org.lwjgl.lwjgl:lwjgl_util:" + lwjglVersion;
        const string lwjglPlatform = "org.lwjgl.lwjgl:lwjgl-platform:" + lwjglVersion;

        if (version.Libraries == null || !version.Libraries.Any(library => library.Name == lwjglPlatform))
            return;

        foreach (MinecraftVersion.ModelLibrary library in version.Libraries)
        {
            if (library.Name is not (lwjgl or lwjglUtil or lwjglPlatform)) continue;

            if (library.Rules != null)
            {
                library.Rules = library.Rules.Select(rule =>
                    rule.Action == "disallow" && rule.Os?.Name == "osx"
                                              && string.IsNullOrWhiteSpace(rule.Os.Arch)
                        ? new MinecraftVersion.ModelLibrary.ModelRule
                        {
                            Action = rule.Action,
                            Os = new MinecraftVersion.ModelLibrary.ModelRule.ModelOS
                            {
                                Name = "osx",
                                Arch = "x64"
                            }
                        }
                        : rule).ToArray();
            }

            if (library.Name != lwjglPlatform) continue;

            library.Downloads ??= new MinecraftVersion.ModelLibrary.ModelDownloads();
            library.Downloads.Classifiers ??= new MinecraftVersion.ModelLibrary.ModelDownloads.ModelClassifiers();

            library.Downloads.Classifiers.NativesOSX = new FileArtifact
            {
                Path = $"org/lwjgl/lwjgl/lwjgl-platform/{lwjglVersion}/lwjgl-platform-{lwjglVersion}-natives-osx-arm64.jar",
                Hash = "eff546c0b319d6ffc7a835652124c18089c67f36",
                Size = 488316,
                Url = "https://github.com/MinecraftMachina/lwjgl/releases/download/2.9.4-20150209-mmachina.2/" +
                      "lwjgl-platform-2.9.4-nightly-20150209-natives-osx.jar"
            };
            library.Natives ??= new MinecraftVersion.ModelLibrary.ModelNatives();
            library.Natives.MacOS = "natives-osx";
        }
    }
}
