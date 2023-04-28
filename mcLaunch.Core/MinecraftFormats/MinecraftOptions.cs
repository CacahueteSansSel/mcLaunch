using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace mcLaunch.Core.MinecraftFormats;

public class MinecraftOptions : Dictionary<string, object>
{
    public string Filename { get; }
    
    public MinecraftOptions(string filename)
    {
        Filename = filename;
        
        Load(File.ReadAllLines(filename));
    }

    public string Build()
    {
        string finalStr = "";

        foreach (var kv in this)
        {
            finalStr += $"{kv.Key}:{ObjectToString(kv.Value)}{Environment.NewLine}";
        }

        return finalStr;
    }

    public void Save()
    {
        File.WriteAllText(Filename, Build());
    }

    string ObjectToString(object obj)
    {
        if (obj is float f) return f.ToString(CultureInfo.InvariantCulture);
        if (obj is int i) return i.ToString(CultureInfo.InvariantCulture);
        if (obj is bool b) return b.ToString().ToLower();
        if (obj is object[] arr) return JsonSerializer.Serialize(arr.Select(ObjectToString));
        
        return obj.ToString();
    }

    object ParseValue(string input)
    {
        if (input.StartsWith('[') && input.EndsWith(']'))
        {
            JsonNode json = JsonNode.Parse(input)!;
            if (json is JsonArray array)
            {
                return array.Select(item => ParseValue(item.ToString())).ToArray();
            }
        }

        if (input.StartsWith('"') && input.EndsWith('"'))
        {
            return input.Trim('"');
        }
            
        if (int.TryParse(input, CultureInfo.InvariantCulture, out int intValue))
        {
            return intValue;
        } else if (float.TryParse(input, CultureInfo.InvariantCulture, out float floatValue))
        {
            return floatValue;
        } else if (bool.TryParse(input, out bool boolValue))
        {
            return boolValue;
        }

        return input;
    }
    
    void Load(string[] lines)
    {
        foreach (string line in lines)
        {
            if (!line.Contains(':')) continue;

            string[] tokens = line.Split(':');
            string key = tokens[0].Trim();
            string stringValue = tokens[1].Trim();
            
            Add(key, ParseValue(stringValue));
        }
    }
}