using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    
    public static async Task<T?> GetAsyncAuthBearer<T>(string url, string auth)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth);

        HttpResponseMessage resp = await client.GetAsync(url);
        Console.WriteLine($"{url} => {(int)resp.StatusCode} {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
    
        return JsonSerializer.Deserialize<T>(json);
    }
    
    public static async Task<JsonNode?> GetNodeAsync(string url, bool patchDateTimes = false)
    {
        HttpClient client = new HttpClient();

        HttpResponseMessage resp = await client.GetAsync(url);
        Console.WriteLine($"{url} => {(int)resp.StatusCode} {resp.StatusCode}");
        resp.EnsureSuccessStatusCode();

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());

        if (patchDateTimes) json = json.Replace("+0000", "");

        return JsonNode.Parse(json);
    }
    
    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        HttpClient client = new HttpClient();
        
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        string inputJson = JsonSerializer.Serialize(data);
        HttpContent content = new StringContent(inputJson, new MediaTypeHeaderValue("application/json"));
        HttpResponseMessage resp = await client.PostAsync(url, content);
        resp.EnsureSuccessStatusCode();

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
    
        return JsonSerializer.Deserialize<TResponse>(json);
    }
}