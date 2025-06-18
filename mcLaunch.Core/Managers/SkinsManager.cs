using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Core;
using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;

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
    
    public static async Task AddSkin(string filename, string name, SkinType type)
    {
        string localFilename = $"{SkinsPath}/{Path.GetFileName(filename)}";
        File.Copy(filename, localFilename, true);
        
        MinecraftProfile? profile = await MinecraftServices.UploadSkinAsync(filename, type);
        
        var skin = new ManifestSkin
        {
            Filename = localFilename,
            Name = name,
            Url = profile?.Skins[0].Url ?? string.Empty,
            Type = type
        };
        skin.LoadBitmap();
        
        Skins = Skins.Append(skin).ToArray();
        
        await File.WriteAllTextAsync($"{SkinsPath}/manifest.json", JsonSerializer.Serialize(Skins));
    }
}

public class ManifestSkin
{
    public string Filename { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public SkinType Type { get; set; }
    [JsonIgnore]
    public Bitmap? Bitmap { get; set; }
    
    public async void LoadBitmap()
    {
        if (Bitmap != null) return;
        
        Bitmap = new Bitmap(Filename);
    }
}