using System.Text.RegularExpressions;

namespace mcLaunch.Server.Core;

public class UuidStorage
{
    private Dictionary<string, string> dict = new();

    private Regex regex = new Regex("UUID of player \\w+ is .+",
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);

    public int Count => dict.Count;

    public bool IsUuidLine(string input) => regex.IsMatch(input);

    public (string username, string uuid)? Process(string input)
    {
        if (!IsUuidLine(input)) return null;

        string line = regex.Match(input).Value
            .Replace("UUID of player ", " ")
            .Replace(" is ", " ")
            .Trim();

        string[] tokens = line.Split(" ");

        if (dict.ContainsKey(tokens[0])) return (tokens[0], tokens[1]);;
        
        dict.Add(tokens[0], tokens[1]);

        return (tokens[0], tokens[1]);
    }

    public void Remove(string username) => dict.Remove(username);

    public string GetUuid(string username)
        => dict[username];

    public string GetUsername(string uuid)
        => dict.First(kv => kv.Value == uuid).Key;
}