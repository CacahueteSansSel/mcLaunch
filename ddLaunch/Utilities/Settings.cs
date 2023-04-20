using System;
using System.IO;
using Avalonia;
using Avalonia.Platform;

namespace ddLaunch.Utilities;

public static class Settings
{
    public static string Get(string filename)
    {
        try
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var stream = assets.Open(new Uri($"avares://ddLaunch/resources/settings/{filename}"));

            return new StreamReader(stream).ReadToEnd();
        }
        catch (Exception e)
        {
            return null;
        }
    }
}