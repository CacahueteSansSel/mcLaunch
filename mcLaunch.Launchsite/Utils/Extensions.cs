using System.IO.Compression;
using System.Text;

namespace mcLaunch.Launchsite.Utils;

public static class Extensions
{
    public static List<T> Adds<T>(this List<T> list, IEnumerable<T>? array)
    {
        if (array == null) return list;

        list.AddRange(array);
        return list;
    }

    public static List<T> AddsOnce<T>(this List<T> list, IEnumerable<T>? array)
    {
        if (array == null) return list;

        list.AddRange(array.Where(e => !list.Contains(e)));
        return list;
    }

    public static bool Exists(this ZipArchive zip, string path) => zip.GetEntry(path) != null;

    public static byte[] ReadAllBytes(this ZipArchive zip, string path)
    {
        ZipArchiveEntry entry = zip.GetEntry(path)!;
        using Stream stream = entry.Open();

        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    public static string ReadAllText(this ZipArchive zip, string path) =>
        Encoding.UTF8.GetString(ReadAllBytes(zip, path));
}