using System.Text.Json.Serialization;
using System.Web;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Cacahuete.MinecraftLib.Models;
using Markdig;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using ReactiveUI;

namespace mcLaunch.Core.Mods;

public class PlatformModpack : ReactiveObject, IVersionContent
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
    public IEnumerable<IVersion> ContentVersions => Versions;

    public string? LongDescriptionBody
    {
        get => longDescriptionBody;
        set => this.RaiseAndSetIfChanged(ref longDescriptionBody, value);
    }
    public string? IconPath { get; set; }
    public string? BackgroundPath { get; set; }
    public ModpackVersion[] Versions { get; set; }
    public string[] MinecraftVersions { get; set; }
    public ModpackVersion? LatestVersion { get; set; }
    public string? LatestMinecraftVersion { get; set; }
    public bool IsUpdateRequired { get; set; }
    public int? DownloadCount { get; set; }
    public DateTime? LastUpdated { get; set; }
    public uint Color { get; set; }

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

    public string DownloadCountFormatted => !IsDownloadCountValid ? "-" : DownloadCount.Value.ToDisplay();
    public TimeSpan LastUpdatedSpan => !IsLastUpdatedValid ? TimeSpan.Zero : DateTime.Now - LastUpdated.Value;
    public string LastUpdatedFormatted => !IsLastUpdatedValid ? "-" : LastUpdatedSpan.ToDisplay();
    public bool IsDownloadCountValid => DownloadCount.HasValue;
    public bool IsLastUpdatedValid => LastUpdated.HasValue;

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

    public bool IsSimilar(PlatformModpack other)
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

    public string[] FetchAndSortSupportedMinecraftVersions()
    {
        if (Versions == null) return Array.Empty<string>();
        
        List<Version> mcVersions = new();

        foreach (ModpackVersion? modpackVersion in Versions)
        {
            if (modpackVersion == null || !Version.TryParse(modpackVersion.MinecraftVersion, out Version version)) 
                continue;
            
            if (!mcVersions.Contains(version)) mcVersions.Add(version);
        }
        
        mcVersions.Sort();
        return mcVersions.Select(v => v.ToString()).ToArray();
    }

    public string[] FetchModLoaders()
    {
        List<string> modloadersNames = new();

        if (Versions == null) return [];
        
        foreach (ModpackVersion? modpackVersion in Versions)
        {
            if (modpackVersion == null || string.IsNullOrEmpty(modpackVersion.ModLoader)) continue;

            string capModLoaderName = modpackVersion.ModLoader.Capitalize();
            
            if (!modloadersNames.Contains(capModLoaderName))
                modloadersNames.Add(capModLoaderName);
        }

        return modloadersNames.ToArray();
    }

    public class ModpackVersion : IVersion
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MinecraftVersion { get; set; }
        public string ModpackFileUrl { get; set; }
        public string? ModpackFileHash { get; set; }
        public string ModLoader { get; set; }
    }

    public void TransformLongDescriptionToHtml()
    {
        if (string.IsNullOrWhiteSpace(LongDescriptionBody)) 
            return;

        LongDescriptionBody = Markdown.ToHtml(LongDescriptionBody, 
            new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
    }
}