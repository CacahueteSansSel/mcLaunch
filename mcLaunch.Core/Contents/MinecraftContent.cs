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
using mcLaunch.Core.Contents.Platforms;
using ReactiveUI;

namespace mcLaunch.Core.Contents;

public class MinecraftContent : ReactiveObject
{
    public static MinecraftContent CreateIdOnly(string id) => new() {Id = id};

    string? longDescriptionBody;
    IconCollection icon;
    Bitmap? background;
    MinecraftContentPlatform? platform;
    bool isInstalledOnCurrentBox;

    public string? Name { get; set; }
    public string Id { get; set; }
    public string Author { get; set; }
    public MinecraftContentType Type { get; set; }
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
    public string? License { get; set; }

    [JsonIgnore]
    public bool IsOpenSource => License != null
                                && License.ToLower() != "all rights reserved"
                                && License.ToLower() != "arr"
                                && License != "LicenseRef-All-Rights-Reserved";

    [JsonIgnore]
    public bool IsInstalledOnCurrentBox
    {
        get => isInstalledOnCurrentBox;
        set => this.RaiseAndSetIfChanged(ref isInstalledOnCurrentBox, value);
    }

    public bool IsInstalledOnCurrentBoxUi { get; set; }
    public bool IsInvalid { get; set; }

    [JsonIgnore]
    public IconCollection Icon
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
    public MinecraftContentPlatform? Platform
    {
        get => platform;
        set => platform = value;
    }

    public string DownloadCountFormatted => !IsDownloadCountValid ? "-" : DownloadCount.Value.ToDisplay();
    public TimeSpan LastUpdatedSpan => !IsLastUpdatedValid ? TimeSpan.Zero : DateTime.Now - LastUpdated.Value;
    public string LastUpdatedFormatted => !IsLastUpdatedValid ? "-" : LastUpdatedSpan.ToDisplay();
    public bool IsDownloadCountValid => DownloadCount.HasValue;
    public bool IsLastUpdatedValid => LastUpdated.HasValue;

    public MinecraftContent(Stream inputStream)
    {
        BinaryReader rd = new(inputStream);

        Name = rd.ReadNullableString();
        Id = rd.ReadString();
        Author = rd.ReadString();
        ShortDescription = rd.ReadNullableString();
        Changelog = rd.ReadNullableString();
        Url = rd.ReadNullableString();
        IconUrl = rd.ReadNullableString();
        LatestVersion = rd.ReadNullableString();
        LatestMinecraftVersion = rd.ReadNullableString();
        DownloadCount = rd.ReadInt32();
        if (DownloadCount == 0) DownloadCount = null;
        LastUpdated = DateTime.FromBinary(rd.ReadInt64());
        License = rd.ReadNullableString();
        string? platformId = rd.ReadNullableString();
    }

    public MinecraftContent()
    {
        
    }

    public void WriteToStream(Stream stream)
    {
        BinaryWriter wr = new(stream);
        
        wr.WriteNullableString(Name);
        wr.Write(Id);
        wr.Write(Author);
        wr.WriteNullableString(ShortDescription);
        wr.WriteNullableString(Changelog);
        wr.WriteNullableString(Url);
        wr.WriteNullableString(IconUrl);
        wr.WriteNullableString(LatestVersion);
        wr.WriteNullableString(LatestMinecraftVersion);
        wr.Write(DownloadCount ?? 0);
        wr.Write(LastUpdated?.ToBinary() ?? 0);
        wr.WriteNullableString(License);
        wr.WriteNullableString(Platform?.Name);
    }

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

    public bool IsSimilar(MinecraftContent other)
        => IsSimilar(other.Name, other.Author);

    public bool IsSimilar(BoxStoredModification other)
        => IsSimilar(other.Name, other.Author);

    public bool MatchesQuery(string query)
        => Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)
           || Author.Contains(query, StringComparison.InvariantCultureIgnoreCase)
           || ModPlatformId.Contains(query, StringComparison.InvariantCultureIgnoreCase);

    public string GetLicenseDisplayName() => License ?? "Unknown";

    public void SetDefaultIcon()
    {
        Icon = IconCollection.Default!;
    }

    public async Task DownloadIconAsync()
    {
        if (string.IsNullOrWhiteSpace(IconUrl))
        {
            SetDefaultIcon();
            return;
        }

        Icon = IconCollection.FromUrl(IconUrl);
        await Icon.DownloadAllAsync();

        if (Icon.IconLarge == null && Icon.IconSmall == null)
            SetDefaultIcon();
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
        if (CacheManager.HasBitmap(cacheName))
        {
            await Task.Run(() => { Background = CacheManager.LoadBitmap(cacheName); });

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

        ContentVersion[] versions =
            await ModPlatformManager.Platform.GetContentVersionsAsync(this,
                target.Manifest.ModLoaderId,
                target.Manifest.Version);

        // TODO: maybe tell the user when the installation failed
        if (versions.Length == 0) return;

        await ModPlatformManager.Platform.InstallContentAsync(target, this, versions[0].Id, false);

        IsInstalledOnCurrentBox = true;
    }
}

public enum MinecraftContentType
{
    Modification,
    ResourcePack,
    ShaderPack,
    DataPack,
    World
}