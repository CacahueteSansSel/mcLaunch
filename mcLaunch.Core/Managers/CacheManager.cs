using Avalonia.Media.Imaging;
using mcLaunch.Core.Contents;

namespace mcLaunch.Core.Managers;

public static class CacheManager
{
    public static string FolderPath { get; private set; }

    public static void Init()
    {
        FolderPath = AppdataFolderManager.GetValidPath("cache");

        Directory.CreateDirectory(FolderPath);
        Directory.CreateDirectory($"{FolderPath}/bitmaps");
        Directory.CreateDirectory($"{FolderPath}/mods");
    }

    public static void Store(Bitmap? bmp, string id)
    {
        if (bmp == null) return;

        try
        {
            bmp.Save($"{FolderPath}/bitmaps/{id}.cache", 30);
        }
        catch
        {
            // ignored
        }
    }

    public static void Store(MinecraftContent? mod, string id)
    {
        if (mod == null) return;

        try
        {
            using FileStream fs = new($"{FolderPath}/mods/{id}.cache", FileMode.Create);
            mod.WriteToStream(fs);
        }
        catch
        {
            // ignored
        }
    }

    public static Bitmap? LoadBitmap(string id)
    {
        if (!File.Exists($"{FolderPath}/bitmaps/{id}.cache")) return null;

        try
        {
            return new Bitmap($"{FolderPath}/bitmaps/{id}.cache");
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static MinecraftContent? LoadContent(string id)
    {
        if (!File.Exists($"{FolderPath}/contents/{id}.cache")) return null;

        try
        {
            using FileStream fs = new($"{FolderPath}/contents/{id}.cache", FileMode.Open);
            return new MinecraftContent(fs);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static bool HasBitmap(string id)
    {
        return File.Exists($"{FolderPath}/bitmaps/{id}.cache");
    }

    public static bool HasContent(string id)
    {
        return File.Exists($"{FolderPath}/contents/{id}.cache");
    }
}