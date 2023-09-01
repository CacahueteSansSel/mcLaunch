using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Web;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Markdig;
using mcLaunch.Core.Utilities;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using ReactiveUI;

namespace mcLaunch.Core.Mods;

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
    public string? Changelog { get; set; }
    public string? Url { get; set; }

    public string? LongDescriptionBody
    {
        get => longDescriptionBody;
        set => this.RaiseAndSetIfChanged(ref longDescriptionBody, value);
    }
    public string? IconUrl { get; set; }
    public string? BackgroundPath { get; set; }
    public string[] Versions { get; set; }
    public string[] MinecraftVersions { get; set; }
    public string? LatestVersion { get; set; }
    public string? LatestMinecraftVersion { get; set; }
    public string? InstalledVersion { get; set; }
    public bool IsUpdateRequired { get; set; }
    public string? Filename { get; set; }
    public bool IsFilenameEmpty => string.IsNullOrWhiteSpace(Filename);
    public int? DownloadCount { get; set; }
    public DateTime? LastUpdated { get; set; }

    public bool IsInstalledOnCurrentBox
    {
        get => isInstalledOnCurrentBox;
        set => this.RaiseAndSetIfChanged(ref isInstalledOnCurrentBox, value);
    }
    
    public bool IsInstalledOnCurrentBoxUi { get; set; }
    public bool IsInvalid { get; set; }

    [JsonIgnore]
    public IconCollection Icon { get; set; }

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

    public string DownloadCountFormatted => !IsDownloadCountValid ? "-" : DownloadCount.Value.ToDisplay();
    public TimeSpan LastUpdatedSpan => !IsLastUpdatedValid ? TimeSpan.Zero : DateTime.Now - LastUpdated.Value;
    public string LastUpdatedFormatted => !IsLastUpdatedValid ? "-" : LastUpdatedSpan.ToDisplay();
    public bool IsDownloadCountValid => DownloadCount.HasValue;
    public bool IsLastUpdatedValid => LastUpdated.HasValue;

    public void TransformLongDescriptionToHtml()
    {
        if (string.IsNullOrWhiteSpace(LongDescriptionBody)) 
            return;

        LongDescriptionBody = Markdown.ToHtml(LongDescriptionBody, 
            new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
    }

    public bool IsSimilar(string name, string author)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(author)) 
            return false;
        
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

    public async Task DownloadIconAsync()
    {
        Icon = new IconCollection(IconUrl);
        await Icon.DownloadAllAsync();
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
            await ModPlatformManager.Platform.GetModVersionList(Id,
                target.Manifest.ModLoaderId,
                target.Manifest.Version);

        // TODO: maybe tell the user when the installation failed
        if (versions.Length == 0) return;

        await ModPlatformManager.Platform.InstallModAsync(target, this, versions[0], false);

        IsInstalledOnCurrentBox = true;
    }
}