using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace mcLaunch.Core.MinecraftFormats;

public class MinecraftOptions : Dictionary<string, object>
{
    private static readonly string[] excludedOptions =
    {
        "version",
        "resourcePacks",
        "incompatibleResourcePacks",
        "lastServer",
        "soundDevice",
        "tutorialStep",
        "joinedFirstServer"
    };

    public MinecraftOptions(string filename)
    {
        Filename = filename;

        Load(File.ReadAllLines(filename));
    }

    public string Filename { get; }

    public bool CanOptionBeChanged(string key) => !key.StartsWith("key_") && !excludedOptions.Contains(key);

    public string Build()
    {
        string finalStr = "";

        foreach (KeyValuePair<string, object> kv in this)
            finalStr += $"{kv.Key}:{ObjectToString(kv.Value)}{Environment.NewLine}";

        return finalStr;
    }

    public void Save()
    {
        File.WriteAllText(Filename, Build());
    }

    public void SaveTo(string filename)
    {
        File.WriteAllText(filename, Build());
    }

    private string ObjectToString(object obj)
    {
        if (obj is double d) return d.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
        if (obj is float f) return f.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
        if (obj is int i) return i.ToString(CultureInfo.InvariantCulture);
        if (obj is bool b) return b.ToString().ToLower();
        if (obj is object[] arr) return JsonSerializer.Serialize(arr.Select(ObjectToString));

        return obj.ToString();
    }

    private object ParseValue(string input)
    {
        if (input.StartsWith('[') && input.EndsWith(']'))
        {
            JsonNode json = JsonNode.Parse(input)!;
            if (json is JsonArray array) return array.Select(item => ParseValue(item.ToString())).ToArray();
        }

        if (input.StartsWith('"') && input.EndsWith('"')) return input.Trim('"');

        if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int intValue)) return intValue;
        if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
            return floatValue;
        if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            return doubleValue;
        if (bool.TryParse(input, out bool boolValue)) return boolValue;

        return input;
    }

    private void Load(string[] lines)
    {
        foreach (string line in lines)
        {
            if (!line.Contains(':')) continue;

            string[] tokens = line.Split(':');
            string key = tokens[0].Trim();
            string stringValue = tokens[1].Trim();

            TryAdd(key, ParseValue(stringValue));
        }
    }
}