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
        FolderPath = Path.GetFullPath("cache");

        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
    }

    public static void Store(Bitmap? bmp, string id)
    {
        if (bmp == null) return;
        
        bmp.Save($"{FolderPath}/{id}");
    }

    public static void Store(Modification? mod, string id)
    {
        if (mod == null) return;
        
        File.WriteAllText($"{FolderPath}/{id}", JsonSerializer.Serialize(mod));
    }

    public static Bitmap LoadBitmap(string id)
        => new Bitmap($"{FolderPath}/{id}");

    public static Modification? LoadModification(string id)
        => JsonSerializer.Deserialize<Modification>(File.ReadAllText($"{FolderPath}/{id}"));

    public static bool Has(string id)
        => File.Exists($"{FolderPath}/{id}");
}