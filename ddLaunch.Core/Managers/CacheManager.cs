using Avalonia.Media.Imaging;
using Path = System.IO.Path;

namespace ddLaunch.Core.Managers;

public static class CacheManager
{
    public static string FolderPath { get; private set; }
    
    public static void Init()
    {
        FolderPath = Path.GetFullPath("cache");

        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
    }

    public static void Store(Bitmap bmp, string id)
    {
        bmp.Save($"{FolderPath}/{id}");
    }

    public static Bitmap Load(string id)
        => new Bitmap($"{FolderPath}/{id}");

    public static bool Has(string id)
        => File.Exists($"{FolderPath}/{id}");
}