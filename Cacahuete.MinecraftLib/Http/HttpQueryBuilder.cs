using System.Web;

namespace Cacahuete.MinecraftLib.Http;

public class HttpQueryBuilder : Dictionary<string, string>
{
    public HttpQueryBuilder With(string key, string value)
    {
        Add(key, value);

        return this;
    }

    public string Build()
    {
        string final = "";

        foreach (var kv in this) final += $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}&";

        return $"?{final.TrimEnd('&')}";
    }
}