using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Jdenticon;

namespace mcLaunch.Managers;

public static class AnonymityManager
{
    private static string[] anonNames;

    public static void Init()
    {
        using Stream fileStream =
            AssetLoader.Open(new Uri("avares://mcLaunch/resources/settings/anonymized_names.txt"));
        StreamReader reader = new(fileStream);

        anonNames = reader.ReadToEnd().Split("\r\n");
    }

    public static AnonymitySession CreateSession()
    {
        return new AnonymitySession(anonNames);
    }
}

public class AnonymitySession
{
    private readonly List<string> initialNames;
    private List<string> availableNames;

    public AnonymitySession(string[] names)
    {
        initialNames = new List<string>(names);
        availableNames = new List<string>(names);
    }

    public string TakeName()
    {
        if (availableNames.Count == 0)
            availableNames = new List<string>(initialNames);

        int index = Random.Shared.Next(0, availableNames.Count);
        string word = availableNames[(int) MathF.Min(index, availableNames.Count - 1)];
        availableNames.Remove(word);

        return word;
    }

    public (string, Bitmap) TakeNameAndIcon()
    {
        string name = TakeName();
        MemoryStream iconStream = new();

        Identicon.FromValue(name, 512)
            .SaveAsPng(iconStream);

        iconStream.Seek(0, SeekOrigin.Begin);

        return (name, new Bitmap(iconStream));
    }
}