using System;
using System.IO;
using Avalonia.Platform;

namespace mcLaunch.Utilities;

public static class InternalSettings
{
    public static string Get(string filename)
    {
        try
        {
            using Stream? stream = AssetLoader.Open(new Uri($"avares://mcLaunch/resources/settings/{filename}"));

            return new StreamReader(stream).ReadToEnd();
        }
        catch (Exception e)
        {
            return null;
        }
    }
}