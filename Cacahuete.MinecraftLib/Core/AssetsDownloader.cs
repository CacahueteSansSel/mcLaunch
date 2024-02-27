using System.Text.Json;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core;

public class AssetsDownloader
{
    public AssetsDownloader(MinecraftFolder folder, string path = "assets") : this($"{folder.Path}/{path}")
    {
    }

    public AssetsDownloader(string customPath)
    {
        Path = customPath;
    }

    public string Path { get; }
    public string? VirtualPath { get; private set; }

    private async Task DownloadVirtualAsync(AssetIndex index, MinecraftVersion version, Action<float> percentCallback)
    {
        VirtualPath = $"{Path}/virtual/{version.AssetIndex.Id}";
        if (!Directory.Exists(VirtualPath)) Directory.CreateDirectory(VirtualPath);

        Asset[] assets = index.ParseAll();
        int cur = 0;
        foreach (Asset asset in assets)
        {
            float percent = cur / (float) assets.Length;

            string[] assetPathParts = asset.Name.Split('/');
            string directoriesPath = string.Join('/', assetPathParts.Take(assetPathParts.Length - 1));

            if (!string.IsNullOrWhiteSpace(directoriesPath) && !Directory.Exists(directoriesPath))
                Directory.CreateDirectory(directoriesPath);

            string assetPath = $"{VirtualPath}/{asset.Name}";

            await Context.Downloader.DownloadAsync(asset.Url, assetPath, asset.Hash);

            cur++;
            percentCallback?.Invoke(percent);
        }

        percentCallback?.Invoke(1);
    }

    public async Task DownloadAsync(MinecraftVersion version, Action<float> percentCallback)
    {
        AssetIndex? index = await version.GetAssetIndexAsync();
        string objectsFolder = $"{Path}/objects";
        string indexesFolder = $"{Path}/indexes";

        if (index != null && index.MapToResources)
        {
            await DownloadVirtualAsync(index, version, percentCallback);
            return;
        }

        VirtualPath = null;

        if (!Directory.Exists(objectsFolder)) Directory.CreateDirectory(objectsFolder);
        if (!Directory.Exists(indexesFolder)) Directory.CreateDirectory(indexesFolder);

        await File.WriteAllTextAsync($"{indexesFolder}/{version.AssetIndex.Id}.json",
            JsonSerializer.Serialize(index!));

        Asset[] assets = index.ParseAll();
        int cur = 0;
        foreach (Asset asset in assets)
        {
            float percent = cur / (float) assets.Length;

            string prefixFolder = $"{objectsFolder}/{asset.Prefix}";
            if (!Directory.Exists(prefixFolder)) Directory.CreateDirectory(prefixFolder);

            string assetPath = $"{prefixFolder}/{asset.Hash}";

            await Context.Downloader.DownloadAsync(asset.Url, assetPath, null);

            cur++;
            percentCallback?.Invoke(percent);
        }

        percentCallback?.Invoke(1);
    }
}