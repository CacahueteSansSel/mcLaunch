using System.Text.Json;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core;

public class AssetsDownloader
{
    public string Path { get; }

    public AssetsDownloader(MinecraftFolder folder, string path = "assets")
    {
        Path = $"{folder.Path}/{path}";
    }

    public AssetsDownloader(string customPath)
    {
        Path = customPath;
    }

    public async Task DownloadAsync(MinecraftVersion version, Action<float> percentCallback, bool force = false)
    {
        AssetIndex? index = await version.GetAssetIndexAsync();
        string objectsFolder = $"{Path}/objects";
        string indexesFolder = $"{Path}/indexes";

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

            if (File.Exists(assetPath) && !force) continue;

            await Context.Downloader.DownloadAsync(asset.Url, assetPath);

            cur++;
            percentCallback?.Invoke(percent);
        }

        percentCallback?.Invoke(1);
    }
}