using System.Text.RegularExpressions;

namespace mcLaunch.Server.Core;

public class ChatParser
{
    private Regex _regex = new Regex("<\\w+> .+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

    public bool IsChatMessage(string input)
        => _regex.IsMatch(input);

    public ChatMessage? Parse(string input)
    {
        if (!IsChatMessage(input)) return null;

        string rawMessage = _regex.Match(input).Value
            .Replace("<", "")
            .Replace(">", "")
            .Trim();

        string[] tokens = rawMessage.Split(' ');

        string username = tokens[0];
        string message = string.Join(' ', tokens.Skip(1));

        return new ChatMessage(username, message);
    }
}

public class ChatMessage
{
    public string Username { get; }
    public string Contents { get; private set; }

    public ChatMessage(string username, string contents)
    {
        Username = username;
        Contents = contents;
    }

    public ChatMessage RemoveForbiddenCharacters()
    {
        Contents = Contents
            .Replace("@", "`@`")
            .Replace("/", "`/`");
        
        return this;
    }
}