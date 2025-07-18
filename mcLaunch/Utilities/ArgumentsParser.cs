using System.Collections.Generic;
using System.Linq;

namespace mcLaunch.Utilities;

public class ArgumentsParser
{
    private readonly Dictionary<string, string> dict = new();

    public Dictionary<string, string> Dictionary => dict;

    public ArgumentsParser(string commandLine)
    {
        ProcessArgs(commandLine.QuotesSplit(' '));
    }

    public ArgumentsParser(string[] args)
    {
        ProcessArgs(args);
    }

    void ProcessArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string current = args[i];

            if (current.Contains('='))
            {
                string[] tokens = current.Split('=');
                dict.Add(tokens[0].TrimStart('-').Trim(), tokens[1].Trim());
                
                continue;
            }
            
            string? next = i + 1 >= args.Length ? null : args[i + 1];

            if (current.StartsWith('-') && next != null && !next.StartsWith('-'))
            {
                dict.Add(current.TrimStart('-'), next);
                i++;

                continue;
            }

            dict.Add(current.TrimStart('-'), string.Empty);
        }
    }

    public int Count => dict.Count;

    public string? GetOrDefault(int index) => index >= Count ? null : dict.ElementAt(index).Key;
    public string? Get(string key, string? defaultValue = null) => dict.TryGetValue(key, out string? value) ? value : defaultValue;

    public bool Contains(string key) => dict.ContainsKey(key);
}