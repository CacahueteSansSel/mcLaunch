using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Http;

public static class Api
{
    const int RetryCount = 2;
    static ProductInfoHeaderValue? userAgent;

    public static event Action<string> OnNetworkError; 
    public static event Action<string> OnNetworkSuccess; 

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
                OnNetworkError?.Invoke(url);
                return default;
            }
            
            try
            {
                resp = await client.GetAsync(url);
                if (resp.IsSuccessStatusCode) break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{url} => (Exception) {e}");
            }

            t++;
        }

        if (!resp.IsSuccessStatusCode) return default;

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
        if (patchDateTimes) json = json.Replace("+0000", "");
        
        OnNetworkSuccess?.Invoke(url);
    
        return JsonSerializer.Deserialize<T>(json);
    }
    
    public static async Task<T?> GetAsyncAuthBearer<T>(string url, string auth)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth);

        HttpResponseMessage resp = null;
        int t = 0;
        while (true)
        {
            if (t >= RetryCount)
            {
                OnNetworkError?.Invoke(url);
                return default;
            }
            
            try
            {
                resp = await client.GetAsync(url);
                if (resp.IsSuccessStatusCode) break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{url} => (Exception) {e}");
            }

            t++;
        }

        if (!resp.IsSuccessStatusCode) return default;
        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
        
        OnNetworkSuccess?.Invoke(url);
    
        return JsonSerializer.Deserialize<T>(json);
    }
    
    public static async Task<JsonNode?> GetNodeAsync(string url, bool patchDateTimes = false)
    {
        HttpClient client = new HttpClient();

        HttpResponseMessage resp = null;
        int t = 0;
        while (true)
        {
            if (t >= RetryCount)
            {
                OnNetworkError?.Invoke(url);
                return default;
            }
            
            try
            {
                resp = await client.GetAsync(url);
                if (resp.IsSuccessStatusCode) break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{url} => (Exception) {e}");
            }

            t++;
        }

        if (!resp.IsSuccessStatusCode) return default;

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
        if (patchDateTimes) json = json.Replace("+0000", "");
        
        OnNetworkSuccess?.Invoke(url);

        return JsonNode.Parse(json);
    }
    
    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        HttpClient client = new HttpClient();
        
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        string inputJson = JsonSerializer.Serialize(data);
        HttpContent content = new StringContent(inputJson);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        HttpResponseMessage resp = null;
        int t = 0;
        while (true)
        {
            if (t >= RetryCount)
            {
                OnNetworkError?.Invoke(url);
                return default;
            }
            
            try
            {
                resp = await client.PostAsync(url, content);
                if (resp.IsSuccessStatusCode) break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{url} => (Exception) {e}");
            }

            t++;
        }

        if (!resp.IsSuccessStatusCode) return default;

        string json = Encoding.UTF8.GetString(await resp.Content.ReadAsByteArrayAsync());
        
        OnNetworkSuccess?.Invoke(url);
    
        return JsonSerializer.Deserialize<TResponse>(json);
    }
}