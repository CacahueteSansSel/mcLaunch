﻿using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Managers;

namespace mcLaunch.Core.Core;

public class IconCollection
{
    public const int SmallIconSize = 48;
    public const int LargeIconSize = 384;

    private IconCollection(string url, bool isFile)
    {
        Url = url;
        IsLocalFile = isFile;
    }

    public IconCollection(Uri resourceUri)
    {
        ResourceUri = resourceUri;
    }

    private IconCollection()
    {
    }

    public static IconCollection? Default { get; set; } = new() {IsDefaultIcon = true};

    public Uri? ResourceUri { get; }
    public string? Url { get; }
    public bool IsLocalFile { get; }

    public Bitmap? IconSmall { get; private set; }
    public Bitmap? IconLarge { get; private set; }
    public bool IsDefaultIcon { get; private set; }
    public int IconSmallSize { get; private set; } = SmallIconSize;
    public int IconLargeSize { get; private set; } = LargeIconSize;

    public static IconCollection FromUrl(string url)
    {
        return new IconCollection(url, false);
    }

    public static IconCollection FromResources(string path)
    {
        return new IconCollection(new Uri($"avares://mcLaunch/resources/{path}"));
    }

    public static async Task<IconCollection> FromFileAsync(string filename, int largeSize = LargeIconSize,
        int smallSize = SmallIconSize)
    {
        return await new IconCollection(filename, true).WithCustomSizes(largeSize, smallSize).DownloadAllAsync();
    }

    public static async Task<IconCollection> FromBitmapAsync(Bitmap bitmap, int largeSize = LargeIconSize,
        int smallSize = SmallIconSize)
    {
        IconCollection icon = new();

        await Task.Run(() =>
        {
            icon.IconLarge = bitmap.CreateScaledBitmap(new PixelSize(largeSize, largeSize));
            icon.IconSmall = bitmap.CreateScaledBitmap(new PixelSize(smallSize, smallSize));
        });

        return icon;
    }

    public IconCollection WithCustomSizes(int small, int large)
    {
        IconSmallSize = small;
        IconLargeSize = large;

        return this;
    }

    private async Task<Stream> LoadStreamAsync()
    {
        if (ResourceUri != null)
            return AssetLoader.Open(ResourceUri);

        if (Url == null)
            return AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_mod_logo.png"));

        if (IsLocalFile)
        {
            int times = 0;
            Exception exception = null;

            while (times < 4)
            {
                try
                {
                    return new FileStream(Url, FileMode.Open);
                }
                catch (Exception e)
                {
                    exception = e;
                    await Task.Delay(10);
                }

                times++;
            }

            throw new Exception($"Failed to load the icon: {exception}");
        }

        HttpClient client = new HttpClient();

        try
        {
            HttpResponseMessage resp = await client.GetAsync(Url);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadAsStreamAsync();
        }
        catch (Exception e)
        {
            IsDefaultIcon = true;

            return AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_mod_logo.png"));
        }
    }

    public async Task DownloadSmallAsync()
    {
        SHA1 sha = SHA1.Create();
        string cacheName = string.Empty;
        bool isResource = ResourceUri != null;
        if (!isResource && !IsLocalFile)
        {
            string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(Url)));

            cacheName = $"is-{hash}";

            if (CacheManager.HasBitmap(cacheName) && !IsLocalFile)
            {
                await Task.Run(() => { IconSmall = CacheManager.LoadBitmap(cacheName); });

                return;
            }
        }

        Stream imageStream = await LoadStreamAsync();

        IconSmall = await Task.Run(() =>
        {
            try
            {
                return Bitmap.DecodeToWidth(imageStream, SmallIconSize);
            }
            catch (Exception e)
            {
                return null;
            }
        });

        if (!isResource && !IsLocalFile) CacheManager.Store(IconSmall, cacheName);


        if (!isResource || IsLocalFile) await imageStream.DisposeAsync();
    }

    public async Task DownloadLargeAsync()
    {
        SHA1 sha = SHA1.Create();
        string cacheName = string.Empty;
        bool isResource = ResourceUri != null;
        if (!isResource && !IsLocalFile)
        {
            string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(Url)));

            cacheName = $"il-{hash}";

            if (CacheManager.HasBitmap(cacheName) && !IsLocalFile)
            {
                await Task.Run(() => { IconLarge = CacheManager.LoadBitmap(cacheName); });

                return;
            }
        }

        Stream imageStream = await LoadStreamAsync();

        IconLarge = await Task.Run(() =>
        {
            try
            {
                return Bitmap.DecodeToWidth(imageStream, LargeIconSize);
            }
            catch (Exception e)
            {
                return null;
            }
        });

        if (!isResource && !IsLocalFile) CacheManager.Store(IconLarge, cacheName);

        if (!isResource || IsLocalFile) await imageStream.DisposeAsync();
    }

    public async Task<IconCollection> DownloadAllAsync()
    {
        await DownloadSmallAsync();
        await DownloadLargeAsync();

        return this;
    }
}