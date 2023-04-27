namespace mcLaunch.Core.Utilities;

public static class Extensions
{
    public static string Capitalize(this string text)
        => char.ToUpper(text[0]) + text[1..];

    // This ugly hacks must be used to ensure that two mods are similar on different platforms
    public static string NormalizeTitle(this string title)
        => title
            .Replace("(Fabric)", "")
            .Replace("(fabric)", "")
            .Replace("(Forge)", "")
            .Replace("(forge)", "")
            .Replace("(Forge/Fabric)", "")
            .Replace("(forge/fabric)", "")
            .Replace("(Fabric/Forge)", "")
            .Replace("(fabric/forge)", "")
            .Trim();

    public static string NormalizeUsername(this string username)
        => username.Trim().TrimEnd('_');

    public static byte[] ReadToEndAndClose(this Stream stream, long? size = null)
    {
        MemoryStream arrayStream = new();
        stream.CopyTo(arrayStream);

        try
        {
            stream.Close();
        }
        catch (Exception e)
        {
            
        }

        byte[] data = arrayStream.ToArray();
        arrayStream.Close();

        return data;
    }
}