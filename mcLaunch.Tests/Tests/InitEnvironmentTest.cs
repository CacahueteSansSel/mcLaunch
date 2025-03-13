using Avalonia;
using mcLaunch.Core.Managers;

namespace mcLaunch.Tests.Tests;

public class InitEnvironmentTest : TestBase
{
    public override string Name => "Init mcLaunch Environment";

    public override async Task<TestResult> RunAsync()
    {
        if (!await Test("Initializing managers", InitManagers))
            return TestFailure();
        if (!await Test("Fetching Minecraft versions", FetchingMinecraftVersions))
            return TestFailure();

        return TestResult.Ok;
    }

    private async Task<TestResult> FetchingMinecraftVersions()
    {
        await MinecraftManager.InitAsync();

        if (MinecraftManager.Manifest == null)
            return TestResult.Error("Minecraft versions manifest is null");
        if (MinecraftManager.ManifestVersions.Length == 0)
            return TestResult.Error("Minecraft versions list is empty");

        return TestResult.Ok;
    }

    private async Task<TestResult> InitManagers()
    {
        AppBuilder app = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .SetupWithoutStarting();

        App mcl = (App)app.Instance!;
        mcl.InitManagers();

        return TestResult.Ok;
    }
}