using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using Markdig;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using ReactiveUI;

namespace mcLaunch.Core.Contents;

public class PlatformModpack : ReactiveObject, IVersionContent
{
    private Bitmap? background;
    private Bitmap? icon;
    private bool isInstalledOnCurrentBox;
    private string? longDescriptionBody;
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

    public string ModPlatformId
    {
        get => Platform?.Name;
        set => Platform = ModPlatformManager.Platform.GetModPlatform(value);
    }

    [JsonIgnore] public MinecraftContentPlatform? Platform { get; set; }

    public string DownloadCountFormatted => !IsDownloadCountValid ? "-" : DownloadCount.Value.ToDisplay();
    public TimeSpan LastUpdatedSpan => !IsLastUpdatedValid ? TimeSpan.Zero : DateTime.Now - LastUpdated.Value;
    public string LastUpdatedFormatted => !IsLastUpdatedValid ? "-" : LastUpdatedSpan.ToDisplay();
    public bool IsDownloadCountValid => DownloadCount.HasValue;
    public bool IsLastUpdatedValid => LastUpdated.HasValue;

    public string? Name { get; set; }
    public IEnumerable<IVersion> ContentVersions => Versions;

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
    {
        return IsSimilar(other.Name, other.Author);
    }

    private async Task<Stream> LoadIconStreamAsync()
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

    private async Task<Stream> LoadBackgroundStreamAsync()
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

    public void TransformLongDescriptionToHtml()
    {
        if (string.IsNullOrWhiteSpace(LongDescriptionBody))
            return;

        LongDescriptionBody = Markdown.ToHtml(LongDescriptionBody,
            new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
    }

    public class ModpackVersion : IVersion
    {
        public string? ModpackFileUrl { get; set; }
        public string? ModpackFileHash { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string? MinecraftVersion { get; set; }
        public string? ModLoader { get; set; }
    }
}