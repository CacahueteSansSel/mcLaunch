using System.Collections.Generic;

namespace mcLaunch.Utilities;

public class ArgumentsParser
{
    private Dictionary<string, string> dict = new();

    public ArgumentsParser(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string current = args[i];
            string? next = i + 1 >= args.Length ? null : args[i + 1];

            if (current.StartsWith("-") && next != null && !next.StartsWith("-"))
            {
                dict.Add(current.TrimStart('-'), next);
                i++;
                
                continue;
            }
            
            dict.Add(current.TrimStart('-'), string.Empty);
        }
    }

    public string Get(string key, string defaultValue = null)
        => dict.ContainsKey(key) ? dict[key] : defaultValue;

    public bool Contains(string key)
        => dict.ContainsKey(key);
}