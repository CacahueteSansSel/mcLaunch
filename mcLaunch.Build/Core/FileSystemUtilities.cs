namespace mcLaunch.Build.Core;

public class FileSystemUtilities
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
}