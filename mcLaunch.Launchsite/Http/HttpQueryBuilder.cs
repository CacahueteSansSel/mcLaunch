using System.Web;

namespace mcLaunch.Launchsite.Http;

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

        foreach (KeyValuePair<string, string> kv in this) final += $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}&";

        return $"?{final.TrimEnd('&')}";
    }
}