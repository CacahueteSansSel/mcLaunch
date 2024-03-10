using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Tests.Tests;

public class ColdMinecraftVersionInstallTest : TestBase
{
    private AssetsDownloader _assetsDownloader;
    private LibrariesDownloader _librariesDownloader;

    public ColdMinecraftVersionInstallTest()
    {
        AddDependency<InitEnvironmentTest>();
    }

    public override string Name => "Cold Minecraft versions installation";

    public override async Task<TestResult> RunAsync()
    {
        string[] versions =
        [
            MinecraftManager.Manifest.Latest.Release, "1.18.2", "1.16.5", "1.12.2",
            "1.7.10", "1.5.2", "1.4.7", "1.0"
        ];

        _assetsDownloader = new AssetsDownloader(SystemFolder);
        _librariesDownloader = new LibrariesDownloader(SystemFolder);

        foreach (string version in versions)
        {
            TestResult? result = await InstallVersion(version);
            if (result != null) return result;
        }

        return TestResult.Ok;
    }

    private async Task<TestResult> DownloadVersionAssets(MinecraftVersion version)
    {
        DownloadManager.Begin($"{version.Id} assets");
        await _assetsDownloader.DownloadAsync(version!, null);
        DownloadManager.End();

        await DownloadManager.ProcessAll();

        AssetIndex? index = await version.GetAssetIndexAsync();

        foreach (Asset asset in index.ParseAll())
        {
            string path = $"{_assetsDownloader.Path}/objects/{asset.Prefix}/{asset.Hash}";
            if (!File.Exists(path))
                return TestResult.Error($"Asset at '{path}' does not exist");
        }

        if (!File.Exists($"{_assetsDownloader.Path}/indexes/{version.AssetIndex.Id}.json"))
            return TestResult.Error($"AssetIndex {version.AssetIndex.Id} does not exist");

        return TestResult.Ok;
    }

    private async Task<TestResult> DownloadVersionLibraries(MinecraftVersion version)
    {
        DownloadManager.Begin($"{version.Id} libraries");
        await _librariesDownloader.DownloadAsync(version!, null);
        DownloadManager.End();

        await DownloadManager.ProcessAll();

        foreach (var library in version!.Libraries)
        {
            string path = $"{_librariesDownloader.Path}/{library.Downloads.Artifact.Path}";

            if (!File.Exists(path))
                return TestResult.Error($"Library at '{path}' does not exist");
        }

        return TestResult.Ok;
    }

    private async Task<TestResult?> InstallVersion(string id)
    {
        MinecraftVersion? version = await MinecraftManager.GetAsync(id);
        if (version == null) return TestResult.Error($"Version {id} can't be fetched");

        if (!await Test($"Downloading {id} assets", () => DownloadVersionAssets(version)))
            return TestFailure();
        if (!await Test($"Downloading {id} libraries", () => DownloadVersionLibraries(version)))
            return TestFailure();

        return null;
    }
}