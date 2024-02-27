namespace mcLaunch.Core.Utilities;

public static class FileSystemUtilities
{
    public static void CopyDirectory(string fromPath, string toPath)
    {
        foreach (string directory in Directory.GetDirectories(fromPath, "*", SearchOption.AllDirectories))
        {
            string newPath = directory.Replace(fromPath, toPath);
            Directory.CreateDirectory(newPath);
        }

        foreach (string file in Directory.GetFiles(fromPath, "*", SearchOption.AllDirectories))
        {
            string newPath = file.Replace(fromPath, toPath);
            File.Copy(file, newPath, true);
        }
    }

    public static string NormalizePath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }
}