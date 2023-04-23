using System;
using System.IO;
using Avalonia;
using Avalonia.Platform;

namespace mcLaunch.Utilities;

public static class Credentials
{
    public static string Get(string name)
    {
        try
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using var stream = assets.Open(new Uri($"avares://mcLaunch/resources/credentials/{name}.txt"));

            return new StreamReader(stream).ReadToEnd();
        }
        catch (Exception e)
        {
            return null;
        }
    }
}