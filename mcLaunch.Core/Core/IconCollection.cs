using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Managers;

namespace mcLaunch.Core.Core;

public class IconCollection
{
    public Stream? DirectStream { get; private set; }
    public string? Url { get; private set; }
    public bool IsLocalFile { get; private set; }
    
    public Bitmap? IconSmall { get; private set; }
    public Bitmap? IconLarge { get; private set; }
    public bool IsDefaultIcon { get; private set; }
    public int IconSmallSize { get; private set; } = 48;
    public int IconLargeSize { get; private set; } = 512;
    
    private IconCollection(string url, bool isFile)
    {
        Url = url;
        IsLocalFile = isFile;
    }

    private IconCollection(Stream stream)
    {
        DirectStream = stream;
    }
    
    private IconCollection()
    {
        
    }

    public static IconCollection FromUrl(string url) => new(url, false);
    public static IconCollection FromStream(Stream stream) => new(stream);
    public static async Task<IconCollection> FromFileAsync(string filename, int largeSize = 512, int smallSize = 48) 
        => await new IconCollection(filename, true).WithCustomSizes(largeSize, smallSize).DownloadAllAsync();

    public static async Task<IconCollection> FromBitmapAsync(Bitmap bitmap, int largeSize = 512, int smallSize = 48)
    {
        IconCollection icon = new();

        await Task.Run(() =>
        {
            icon.IconLarge = bitmap.CreateScaledBitmap(new PixelSize(largeSize, largeSize));
            icon.IconSmall = bitmap.CreateScaledBitmap(new PixelSize(smallSize, smallSize));
        });
        
        return icon;
    }
    public static IconCollection Default() => new();

    public IconCollection WithCustomSizes(int small, int large)
    {
        IconSmallSize = small;
        IconLargeSize = large;

        return this;
    }

    async Task<Stream> LoadStreamAsync()
    {
        if (DirectStream != null)
            return DirectStream;
        
        if (Url == null)
            return AssetLoader.Open(new Uri($"avares://mcLaunch/resources/default_mod_logo.png"));
        
        if (IsLocalFile)
            return new FileStream(Url, FileMode.Open);
        
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
            
            return AssetLoader.Open(new Uri($"avares://mcLaunch/resources/default_mod_logo.png"));
        }
    }

    public async Task DownloadSmallAsync()
    {
        SHA1 sha = SHA1.Create();
        string cacheName = string.Empty;
        bool isStream = DirectStream != null;
        if (!isStream)
        {
            string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(Url)));
            
            cacheName = $"is-{hash}";
        
            if (CacheManager.Has(cacheName) && !IsLocalFile)
            {
                await Task.Run(() =>
                {
                    IconSmall = CacheManager.LoadBitmap(cacheName);
                });

                return;
            }
        }

        await using Stream imageStream = await LoadStreamAsync();
        
        IconSmall = await Task.Run(() =>
        {
            try
            {
                return Bitmap.DecodeToWidth(imageStream, 48);
            }
            catch (Exception e)
            {
                return null;
            }
        });
        
        if (!isStream) CacheManager.Store(IconSmall, cacheName);
    }

    public async Task DownloadLargeAsync()
    {
        SHA1 sha = SHA1.Create();
        string cacheName = string.Empty;
        bool isStream = DirectStream != null;
        if (!isStream)
        {
            string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(Url)));
            
            cacheName = $"il-{hash}";
        
            if (CacheManager.Has(cacheName) && !IsLocalFile)
            {
                await Task.Run(() =>
                {
                    IconLarge = CacheManager.LoadBitmap(cacheName);
                });

                return;
            }
        }

        await using Stream imageStream = await LoadStreamAsync();
        
        IconLarge = await Task.Run(() =>
        {
            try
            {
                return Bitmap.DecodeToWidth(imageStream, 512);
            }
            catch (Exception e)
            {
                return null;
            }
        });
        
        if (!isStream) CacheManager.Store(IconLarge, cacheName);
    }

    public async Task<IconCollection> DownloadAllAsync()
    {
        await DownloadSmallAsync();
        await DownloadLargeAsync();

        return this;
    }
}