using System;
using System.IO;
using Avalonia.Platform;

namespace mcLaunch.Utilities;

public static class CurrentBuild
{
    public static Version Version => new("0.1.8");

    public static string? Commit
    {
        get
        {
            try
            {
                using Stream stream = AssetLoader.Open(new Uri("avares://mcLaunch/resources/commit"));
                using StreamReader rd = new(stream);
                
                return rd.ReadToEnd();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}