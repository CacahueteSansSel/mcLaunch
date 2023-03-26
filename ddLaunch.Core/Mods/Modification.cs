using Avalonia.Media.Imaging;
using ddLaunch.Core.Managers;
using ReactiveUI;

namespace ddLaunch.Core.Mods;

public class Modification : ReactiveObject
{
    string? longDescriptionBody;
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
    public string[] Versions { get; set; }
    public string[] MinecraftVersions { get; set; }
    public string? LatestVersion { get; set; }
    public string? LatestMinecraftVersion { get; set; }
    public Bitmap Icon { get; set; }
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

        HttpResponseMessage resp = await client.GetAsync(IconPath);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadAsStreamAsync();
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
            
            Icon = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            
            CacheManager.Store(Icon, cacheName);
        }
    }
}