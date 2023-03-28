namespace ddLaunch.Core.Utilities;

public static class Extensions
{
    public static string Capitalize(this string text)
        => char.ToUpper(text[0]) + text[1..];

    // This ugly hacks must be used to ensure that two mods are similar on different platforms
    public static string NormalizeTitle(this string title)
        => title.Replace("(Fabric)", "").Replace("(fabric)", "").Trim();

    public static string NormalizeUsername(this string username)
        => username.Trim().TrimEnd('_');
}