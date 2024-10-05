using System;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;

namespace mcLaunch.Utilities;

public static class CurrentBuild
{
    public static BuildManifest? Manifest { get; private set; }

    public static Version Version => new("0.2.4");

    public static string Commit => Manifest?.CommitId ?? "unknown";
    public static string Branch => Manifest?.Branch ?? "unknown";
    public static string[] Changelog => Manifest?.Changelog ?? [];

    public static void Load()
    {
        try
        {
            using Stream stream = AssetLoader.Open(new Uri("avares://mcLaunch/resources/settings/build.json"));
            Manifest = JsonSerializer.Deserialize<BuildManifest>(stream)!;
        }
        catch (Exception e)
        {
        }
    }
}

public record BuildManifest(string CommitId, string Branch, string[] Changelog);