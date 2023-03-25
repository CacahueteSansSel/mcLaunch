using System.Text;
using System.Text.Json;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Http;

public static class Api
{
    public static async Task<T?> GetAsync<T>(string url, bool patchDateTimes = false)
    {
        HttpClient client = new HttpClient();

        HttpResponseMessage resp = await client.GetAsync(url);
        Console.WriteLine($"{url} => {(int)resp.StatusCode} {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());

        if (patchDateTimes) json = json.Replace("+0000", "");
    
        return JsonSerializer.Deserialize<T>(json);
    }
}