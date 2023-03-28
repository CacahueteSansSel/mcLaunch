using Avalonia.Media.Imaging;
using ddLaunch.Core.Managers;
using ReactiveUI;

namespace ddLaunch.Core.Mods;

public class Modification : ReactiveObject
{
    string? longDescriptionBody;
    Bitmap? background;
    public string? Name { get; set; }
    public string Id { get; set; }
    public string Author { get; set; }
    public string? ShortDescription { get; set; }

    public string? LongDescriptionBody
    {
        get => longDescriptionBody;
        set => this.RaiseAndSetIfChanged(ref longDescriptionBody, value);
    }
    public string? IconPath { get; set; }
    public string? BackgroundPath { get; set; }
    public string[] Versions { get; set; }
    public string[] MinecraftVersions { get; set; }
    public string? LatestVersion { get; set; }
    public string? LatestMinecraftVersion { get; set; }
    public Bitmap? Icon { get; set; }

    public Bitmap? Background
    {
        get => background;
        set => this.RaiseAndSetIfChanged(ref background, value);
    }
    public ModPlatform Platform { get; set; }

    public bool IsSimilar(Modification other)
    {
        return Name.Trim() == other.Name.Trim() 
               && Author.Trim() == other.Author.Trim();
    }

    async Task<Stream> LoadIconStreamAsync()
    {
        if (IconPath == null) return null;
        
        HttpClient client = new HttpClient();

        try
        {
            HttpResponseMessage resp = await client.GetAsync(IconPath);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadAsStreamAsync();
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task DownloadIconAsync()
    {
        string cacheName = $"icon-mod-{Platform.Name}-{Id}";
        if (CacheManager.Has(cacheName))
        {
            await Task.Run(() =>
            {
                Icon = CacheManager.Load(cacheName);
            });

            return;
        }

        if (IconPath == null) return;
        
        await using (var imageStream = await LoadIconStreamAsync())
        {
            if (imageStream == null) return;
            
            Icon = await Task.Run(() =>
            {
                try
                {
                    return Bitmap.DecodeToWidth(imageStream, 400);
                }
                catch (Exception e)
                {
                    return null;
                }
            });
            
            CacheManager.Store(Icon, cacheName);
        }
    }

    async Task<Stream> LoadBackgroundStreamAsync()
    {
        if (BackgroundPath == null) return null;
        
        HttpClient client = new HttpClient();

        try
        {
            HttpResponseMessage resp = await client.GetAsync(BackgroundPath);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadAsStreamAsync();
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task DownloadBackgroundAsync()
    {
        string cacheName = $"bkg-mod-{Platform.Name}-{Id}";
        if (CacheManager.Has(cacheName))
        {
            await Task.Run(() =>
            {
                Background = CacheManager.Load(cacheName);
            });

            return;
        }

        if (BackgroundPath == null) return;
        
        await using (var imageStream = await LoadBackgroundStreamAsync())
        {
            if (imageStream == null) return;
            
            Background = await Task.Run(() =>
            {
                try
                {
                    return Bitmap.DecodeToWidth(imageStream, 1280);
                }
                catch (Exception e)
                {
                    return null;
                }
            });
            
            CacheManager.Store(Background, cacheName);
        }
    }
}