using mcLaunch.Core.Contents;

namespace mcLaunch.Core.Boxes;

public class BoxStoredContent
{
    private string? id, platformId, name, author;
    private MinecraftContentType? type;

    public MinecraftContent? Content { get; set; }

    [Obsolete("Use Content instead")]
    public string Id
    {
        get => id ?? Content.Id;
        set => id = value;
    }

    public string VersionId { get; init; }

    [Obsolete("Use Content instead")]
    public string PlatformId
    {
        get => platformId ?? Content.ModPlatformId;
        set => platformId = value;
    }

    public string[] Filenames { get; set; }

    [Obsolete("Use Content instead")]
    public string Name
    {
        get => name ?? Content.Name;
        set => name = value;
    }

    [Obsolete("Use Content instead")]
    public string Author
    {
        get => author ?? Content.Author;
        set => author = value;
    }

    [Obsolete("Use Content instead")]
    public MinecraftContentType Type
    {
        get => type ?? Content.Type;
        set => type = value;
    }

    public void Delete(string boxRootPath)
    {
        foreach (string file in Filenames)
        {
            if (Path.IsPathFullyQualified(file))
                throw new Exception("Mod filename is absolute : was the manifest updated to Manifest Version 2 ?");

            string path = $"{boxRootPath}/{file.TrimStart('/')}";

            if (File.Exists(path)) File.Delete(path);
        }
    }
}
