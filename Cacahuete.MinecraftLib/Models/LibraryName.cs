namespace Cacahuete.MinecraftLib.Models;

public class LibraryName
{
    public string Package { get; }
    public string Name { get; }
    public string Version { get; }

    public string JarFilename => $"{Name}-{Version}.jar";

    public LibraryName(string raw)
    {
        string[] tokens = raw.Split(':');

        Package = tokens[0];
        Name = tokens[1];

        if (tokens.Length > 2) Version = tokens[2];
    }

    public string BuildMavenUrl(string baseUrl)
    {
        return $"{baseUrl.TrimEnd('/')}" +
               $"/{Package.Replace('.', '/').Replace(':', '/')}" +
               $"/{Name.Replace('.', '/').Replace(':', '/')}" +
               $"/{Version}" +
               $"/{JarFilename}";
    }
}