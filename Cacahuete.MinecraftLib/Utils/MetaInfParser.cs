using System.IO.Compression;

namespace Cacahuete.MinecraftLib.Utils;

public static class MetaInfParser
{
    public static Dictionary<string, string> Parse(string input)
    {
        Dictionary<string, string> entries = new();
        string[] lines = input.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains(':')) continue;

            string[] tokens = line.Split(':').Select(str => str.Trim()).ToArray();
            if (!entries.ContainsKey(tokens[0])) entries.Add(tokens[0], string.Join(':', tokens.Skip(1)));
        }

        return entries;
    }

    public static Dictionary<string, string> Parse(ZipArchive zipArchive)
        => Parse(zipArchive.ReadAllText("META-INF/MANIFEST.MF"));
}