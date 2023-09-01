using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Managers;

namespace mcLaunch.Core.Core;

public class IconCollection
{
    public string Url { get; private set; }
    
    public Bitmap? IconSmall { get; private set; }
    public Bitmap? IconLarge { get; private set; }
    public bool IsDefaultIcon { get; private set; }
    
    public IconCollection(string url)
    {
        Url = url;
    }

    async Task<Stream> LoadStreamAsync()
    {
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
        string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(Url)));
        string cacheName = $"is-{hash}";
        
        if (CacheManager.Has(cacheName))
        {
            await Task.Run(() =>
            {
                IconSmall = CacheManager.LoadBitmap(cacheName);
            });

            return;
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
        
        CacheManager.Store(IconSmall, cacheName);
    }

    public async Task DownloadLargeAsync()
    {
        SHA1 sha = SHA1.Create();
        string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(Url)));
        string cacheName = $"il-{hash}";
        
        if (CacheManager.Has(cacheName))
        {
            await Task.Run(() =>
            {
                IconLarge = CacheManager.LoadBitmap(cacheName);
            });

            return;
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
        
        CacheManager.Store(IconLarge, cacheName);
    }

    public async Task DownloadAllAsync()
    {
        await DownloadSmallAsync();
        await DownloadLargeAsync();
    }
}