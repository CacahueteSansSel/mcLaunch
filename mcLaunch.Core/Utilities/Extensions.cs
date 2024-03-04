using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Launchsite.Core.ModLoaders;

namespace mcLaunch.Core.Utilities;

public static class Extensions
{
    public static string Capitalize(this string text)
    {
        return string.IsNullOrWhiteSpace(text) ? text : char.ToUpper(text[0]) + text[1..];
    }

    public static string NormalizeCase(this string text)
    {
        string str = "";
        int counter = -1;

        foreach (char c in text)
        {
            counter++;

            if (counter == 0)
            {
                str += char.ToUpper(c);
            }
            else if (c == '_')
            {
                counter = -1;
                str += " ";
            }
            else if (char.IsUpper(c))
            {
                str += $" {c}";
            }
            else
            {
                str += c;
            }
        }

        return str;
    }

    // This ugly hacks must be used to ensure that two mods are similar on different platforms
    public static string NormalizeTitle(this string title)
    {
        return title
            .Replace("(Fabric)", "")
            .Replace("(fabric)", "")
            .Replace("(Forge)", "")
            .Replace("(forge)", "")
            .Replace("(Forge/Fabric)", "")
            .Replace("(forge/fabric)", "")
            .Replace("(Fabric/Forge)", "")
            .Replace("(fabric/forge)", "")
            .Trim();
    }

    public static string NormalizeUsername(this string username)
    {
        return username.Trim().TrimEnd('_');
    }

    public static byte[] ReadToEndAndClose(this Stream stream, long? size = null)
    {
        if (stream.CanSeek && stream.Position != 0)
            stream.Seek(0, SeekOrigin.Begin);

        MemoryStream arrayStream = new();
        stream.CopyTo(arrayStream);

        if (arrayStream.Length == 0 && (size == null || size != 0))
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer);

            arrayStream.Write(buffer);
        }

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

    public static string ToDisplay(this int i)
    {
        if (i < 1000) return i.ToString();
        if (i < 1000000) return $"{(int) MathF.Round(i / 1000f)}k";

        return $"{(int) MathF.Round(i / 1000000f)}M";
    }

    public static string ToDisplay(this TimeSpan span)
    {
        int days = (int) Math.Round(span.TotalDays);
        int weeks = (int) Math.Round(span.TotalDays / 7);
        int months = (int) Math.Round(span.TotalDays / 30);
        int years = (int) Math.Round(span.TotalDays / 365);

        if (days == 1) return "a day ago";
        if (weeks == 1) return "a week ago";
        if (months == 1) return "a month ago";
        if (years == 1) return "a year ago";

        if (span.TotalDays < 1) return $"{span.Hours}h ago";
        if (span.TotalDays < 7) return $"{days} days ago";
        if (span.TotalDays < 30) return $"{weeks} weeks ago";
        if (span.TotalDays < 365) return $"{months} months ago";

        return $"{span.Days / 365} years ago";
    }

    public static Bitmap? LoadIcon(this ModLoaderSupport? modLoader)
    {
        if (modLoader == null) return null;

        return new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{modLoader.Id}.png")));
    }
}