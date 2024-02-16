using Avalonia;
using Avalonia.Controls;
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

        return new TestResult(true, null);
    }

    private async Task<bool> FetchingMinecraftVersions()
    {
        await MinecraftManager.InitAsync();

        return MinecraftManager.Manifest != null && MinecraftManager.ManifestVersions.Length > 0;
    }

    async Task<bool> InitManagers()
    {
        AppBuilder app = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .SetupWithoutStarting();

        App mcl = (App) app.Instance!;
        mcl.InitManagers();

        return true;
    }
}