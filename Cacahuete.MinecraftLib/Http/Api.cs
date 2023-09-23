﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Http;

public static class Api
{
    const int RetryCount = 3;
    static ProductInfoHeaderValue? userAgent;

    public static void SetUserAgent(ProductInfoHeaderValue ua)
    {
        userAgent = ua;
    }
    
    public static async Task<T?> GetAsync<T>(string url, bool patchDateTimes = false)
    {
        HttpClient client = new HttpClient();
        if (userAgent != null) client.DefaultRequestHeaders.UserAgent.Add(userAgent);

        HttpResponseMessage resp = null;
        int t = 0;
        while (true)
        {
            if (t >= RetryCount)
            {
                resp = await client.GetAsync(url);
                break;
            }
            
            try
            {
                resp = await client.GetAsync(url);
                if (resp != null && resp.IsSuccessStatusCode) break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{url} => (Exception) {e}");
            }

            t++;
        }

        if (resp == null)
        {
            Console.WriteLine($"{url} => {(int)resp.StatusCode} {resp.StatusCode}");
        }

        if (!resp.IsSuccessStatusCode) return default;

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

        if (!resp.IsSuccessStatusCode) return default;

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
    
        return JsonSerializer.Deserialize<T>(json);
    }
    
    public static async Task<JsonNode?> GetNodeAsync(string url, bool patchDateTimes = false)
    {
        HttpClient client = new HttpClient();

        HttpResponseMessage resp = await client.GetAsync(url);
        Console.WriteLine($"{url} => {(int)resp.StatusCode} {resp.StatusCode}");

        if (!resp.IsSuccessStatusCode) return default;

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());

        if (patchDateTimes) json = json.Replace("+0000", "");

        return JsonNode.Parse(json);
    }
    
    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        HttpClient client = new HttpClient();
        
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        string inputJson = JsonSerializer.Serialize(data);
        HttpContent content = new StringContent(inputJson);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        HttpResponseMessage resp = await client.PostAsync(url, content);

        if (!resp.IsSuccessStatusCode) return default;

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
    
        return JsonSerializer.Deserialize<TResponse>(json);
    }
}