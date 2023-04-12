using System.Text.Json.Serialization;
using System.Web;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Utilities;
using ReactiveUI;

namespace ddLaunch.Core.Mods;

public class Modification : ReactiveObject
{
    string? longDescriptionBody;
    Bitmap? icon;
    Bitmap? background;
    ModPlatform? platform;
    bool isInstalledOnCurrentBox;
    
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
    public string? InstalledVersion { get; set; }
    public bool IsUpdateRequired { get; set; }

    public bool IsInstalledOnCurrentBox
    {
        get => isInstalledOnCurrentBox;
        set => this.RaiseAndSetIfChanged(ref isInstalledOnCurrentBox, value);
    }

    [JsonIgnore]
    public Bitmap? Icon
    {
        get => icon;
        set => this.RaiseAndSetIfChanged(ref icon, value);
    }

    [JsonIgnore]
    public Bitmap? Background
    {
        get => background;
        set => this.RaiseAndSetIfChanged(ref background, value);
    }

    public string ModPlatformId
    {
        get => Platform?.Name;
        set => Platform = ModPlatformManager.Platform.GetModPlatform(value);
    }

    [JsonIgnore]
    public ModPlatform? Platform
    {
        get => platform;
        set => platform = value;
    }

    public void TransformLongDescriptionToMarkdown()
    {
        if (string.IsNullOrWhiteSpace(LongDescriptionBody)) 
            return;
        
        var converter = new ReverseMarkdown.Converter();
        LongDescriptionBody = HttpUtility.HtmlDecode(converter.Convert(LongDescriptionBody));
    }

    public bool IsSimilar(string name, string author)
    {
        string nameNormalized = Name.NormalizeTitle();
        string authorNormalized = Author.NormalizeUsername();
        string otherNameNormalized = name.NormalizeTitle();
        string otherAuthorNormalized = author.NormalizeUsername();

        return nameNormalized == otherNameNormalized
               && authorNormalized == otherAuthorNormalized;
    }

    public bool IsSimilar(Modification other)
        => IsSimilar(other.Name, other.Author);

    public bool IsSimilar(BoxStoredModification other)
        => IsSimilar(other.Name, other.Author);

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
                Icon = CacheManager.LoadBitmap(cacheName);
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
                Background = CacheManager.LoadBitmap(cacheName);
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

    public async void CommandDownload(Box target)
    {
        // TODO: Version selection

        string[] versions =
            await ModPlatformManager.Platform.GetVersionsForMinecraftVersionAsync(Id,
                target.Manifest.ModLoaderId,
                target.Manifest.Version);

        // TODO: maybe tell the user when the installation failed
        if (versions.Length == 0) return;

        await ModPlatformManager.Platform.InstallModificationAsync(target, this, versions[0]);

        IsInstalledOnCurrentBox = true;
    }
}