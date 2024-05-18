using System.Security.Cryptography;
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

    private IconCollection(string filename)
    {
        Filename = filename;
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
    public string? Filename { get; }

    public Bitmap? IconSmall { get; private set; }
    public Bitmap? IconLarge { get; private set; }
    public bool IsDefaultIcon { get; private set; }
    public int IconSmallSize { get; private set; } = SmallIconSize;
    public int IconLargeSize { get; private set; } = LargeIconSize;

    public static IconCollection FromResources(string path)
    {
        return new IconCollection(new Uri($"avares://mcLaunch/resources/{path}"));
    }

    public static async Task<IconCollection> FromFileAsync(string filename, int largeSize = LargeIconSize,
        int smallSize = SmallIconSize)
    {
        return await new IconCollection(filename)
            .WithCustomSizes(largeSize, smallSize)
            .LoadAsync();
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

        if (Filename == null)
            return AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_mod_logo.png"));

        try
        {
            return new FileStream(Filename, FileMode.Open);
        }
        catch (Exception)
        {
            return AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_mod_logo.png"));
        }
    }

    public async Task LoadSmallAsync()
    {
        await using Stream imageStream = await LoadStreamAsync();

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
    }

    public async Task LoadLargeAsync()
    {
        await using Stream imageStream = await LoadStreamAsync();

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
    }

    public async Task<IconCollection> LoadAsync()
    {
        await LoadSmallAsync();
        await LoadLargeAsync();

        return this;
    }
}