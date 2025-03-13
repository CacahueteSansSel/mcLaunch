using System;
using System.IO;
using Avalonia.Platform;

namespace mcLaunch.Utilities;

public static class Credentials
{
    public static string Get(string name)
    {
        try
        {
            using Stream? stream = AssetLoader.Open(new Uri($"avares://mcLaunch/resources/credentials/{name}.txt"));

            return new StreamReader(stream).ReadToEnd();
        }
        catch (Exception e)
        {
            return null;
        }
    }
}