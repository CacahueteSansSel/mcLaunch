using Avalonia.Media.Imaging;

namespace mcLaunch.Core.Utilities;

public static class BitmapUtilities
{
    public static async Task<Bitmap?> LoadBitmapAsync(string url, int expectedWidth, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
    {
        HttpClient client = new();
        Stream stream;

        try
        {
            HttpResponseMessage resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return null;

            stream = await resp.Content.ReadAsStreamAsync();
        }
        catch (Exception e)
        {
            return null;
        }
        
        return await Task.Run(() => Bitmap.DecodeToWidth(stream, expectedWidth, interpolationMode));
    }
}