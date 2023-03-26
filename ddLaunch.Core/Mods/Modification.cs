using Avalonia.Media.Imaging;

namespace ddLaunch.Core.Mods;

public class Modification
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string IconPath { get; set; }
    public Bitmap Icon { get; set; }
    public ModPlatform Platform { get; set; }

    public bool IsSimilar(Modification other)
    {
        return Name.Trim() == other.Name.Trim() 
               && Author.Trim() == other.Author.Trim();
    }

    async Task<Stream> LoadIconStreamAsync()
    {
        if (IconPath == null) return null;
        
        HttpClient client = new HttpClient();

        HttpResponseMessage resp = await client.GetAsync(IconPath);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadAsStreamAsync();
    }

    public async Task DownloadIconAsync()
    {
        if (IconPath == null) return;
        
        await using (var imageStream = await LoadIconStreamAsync())
        {
            if (imageStream == null) return;
            
            Icon = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
        }
    }
}