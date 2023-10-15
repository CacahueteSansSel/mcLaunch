using System.Text.RegularExpressions;

namespace mcLaunch.Server.Core;

public static partial class RegularExpressions
{
    [GeneratedRegex("(Minecraft).+", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex MinecraftVersion();

    [GeneratedRegex("(\\d|\\.)+", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex SemanticVersionRegex();

    [GeneratedRegex("(with ).+", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex ModLoaderRegex();

    [GeneratedRegex("(?!: )\\w+ joined the game", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex PlayerConnectRegex();

    [GeneratedRegex("(?!: )\\w+ left the game", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex PlayerDisconnectRegex();
}