using System.Text.Json;
using Avalonia.Media.Imaging;

namespace mcLaunch.Core.Managers;

public static class SkinsManager
{
    public static string SkinsPath { get; private set; }
    public static ManifestSkin[] Skins { get; private set; }

    public static void Init()
    {
        SkinsPath = AppdataFolderManager.GetValidPath("system/skins");
        
        if (!File.Exists($"{SkinsPath}/manifest.json"))
        {
            File.WriteAllText($"{SkinsPath}/manifest.json", "[]");
        }

        Skins = JsonSerializer.Deserialize<ManifestSkin[]>(File.ReadAllText($"{SkinsPath}/manifest.json"))!;
    }
    
    public static void LoadAllSkins()
    {
        foreach (var skin in Skins)
            skin.LoadBitmap();
    }
    
    public static void AddSkin(string filename, string name)
    {
        string localFilename = $"{SkinsPath}/{Path.GetFileName(filename)}";
        File.Copy(filename, localFilename, true);
        
        var skin = new ManifestSkin
        {
            Filename = localFilename,
            Name = name
        };
        skin.LoadBitmap();
        
        Skins = Skins.Append(skin).ToArray();
        
        File.WriteAllText($"{SkinsPath}/manifest.json", JsonSerializer.Serialize(Skins));
    }
}

public class ManifestSkin
{
    public string Filename { get; set; }
    public string Name { get; set; }
    public Bitmap? Bitmap { get; set; }
    
    public async void LoadBitmap()
    {
        if (Bitmap != null) return;
        
        Bitmap = new Bitmap(Filename);
    }
}