namespace mcLaunch.Launchsite.Models;

public class LibraryName
{
    public LibraryName(string raw)
    {
        string[] tokens = raw
            .Trim('[', ']')
            .Trim('\'', '\'')
            .Split(':');

        Package = tokens[0];
        Name = tokens[1];
        Extension = "jar";

        if (tokens.Length < 3) return;

        Version = tokens[2];
        if (Version.Contains('@'))
        {
            string[] versionTokens = Version.Split('@');
            Version = versionTokens[0];
            Extension = versionTokens[1];
        }

        if (tokens.Length < 4) return;

        Classifier = tokens[3];
        if (Classifier.Contains('@'))
        {
            string[] classifierTokens = Classifier.Split('@');
            Classifier = classifierTokens[0];
            Extension = classifierTokens[1];
        }
    }

    public string Package { get; }
    public string Name { get; }
    public string Version { get; }
    public string? Classifier { get; }
    public string Extension { get; }

    public string JarFilename => $"{Name}-{Version}{(Classifier != null ? $"-{Classifier}" : "")}.{Extension}";

    public string MavenFilename => $"{Package.Replace('.', '/').Replace(':', '/')}" +
                                   $"/{Name.Replace('.', '/').Replace(':', '/')}" +
                                   $"/{Version}" +
                                   $"/{JarFilename}";

    public string BuildMavenUrl(string baseUrl)
    {
        return $"{baseUrl.TrimEnd('/')}/{MavenFilename}";
    }

    public static bool operator ==(LibraryName? left, LibraryName? right)
    {
        return left?.Package == right?.Package && left?.Name == right?.Name && left?.Version == right?.Version;
    }

    public static bool operator !=(LibraryName? left, LibraryName? right)
    {
        return left?.Package != right?.Package || left?.Name != right?.Name || left?.Version != right?.Version;
    }
}