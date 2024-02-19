using System.Text.Json;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Mods;
using Path = System.IO.Path;

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
        
        bmp.Save($"{FolderPath}/bitmaps/{id}.cache", 30);
    }

    public static void Store(Modification? mod, string id)
    {
        if (mod == null) return;

        using FileStream fs = new($"{FolderPath}/mods/{id}.cache", FileMode.Create);
        mod.WriteToStream(fs);
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

    public static Modification? LoadModification(string id)
    {
        if (!File.Exists($"{FolderPath}/mods/{id}.cache")) return null;
        
        try
        {
            using FileStream fs = new($"{FolderPath}/mods/{id}.cache", FileMode.Open);
            return new Modification(fs);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static bool HasBitmap(string id)
        => File.Exists($"{FolderPath}/bitmaps/{id}.cache");

    public static bool HasModification(string id)
        => File.Exists($"{FolderPath}/mods/{id}.cache");
}