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

    public static byte[] ReadToEndAndClose(this Stream stream, long? size = null)
    {
        byte[] data = new byte[size ?? stream.Length];
        stream.Read(data, 0, data.Length);

        try
        {
            stream.Close();
        }
        catch (Exception e)
        {
            
        }

        return data;
    }
}