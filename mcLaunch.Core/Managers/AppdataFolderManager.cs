using mcLaunch.Core.Utilities;

namespace mcLaunch.Core.Managers;

public static class AppdataFolderManager
{
    private static readonly string[] FoldersToMigrate =
    {
        "boxes", "cache", "forge", "launcher_crashreports", "system", "temp"
    };

    private static readonly string[] FilesToMigrate =
    {
        "settings.json"
    };

    static AppdataFolderManager()
    {
        Init();
    }

    public static string Path { get; private set; }

    public static bool NeedsMigration => Directory.Exists("boxes") && Directory.Exists("system");

    private static void Init()
    {
        if (Path != null) return;

        string suffix = "";

#if DEBUG
        suffix = "_debug";
#endif

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
            Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/.mclaunch{suffix}".FixPath();
        else if (OperatingSystem.IsLinux())
            Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.mclaunch{suffix}".FixPath();

        if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
    }

    private static async Task MigrateFolderAsync(string name)
    {
        if (!Directory.Exists(name)) return;

        await Task.Run(() => FileSystemUtilities.CopyDirectory(System.IO.Path.GetFullPath(name), GetValidPath(name)));
        if (Directory.Exists(name)) Directory.Delete(System.IO.Path.GetFullPath(name), true);
    }

    private static async Task MigrateFileAsync(string name)
    {
        if (!File.Exists(name)) return;

        await Task.Run(() => File.Copy(System.IO.Path.GetFullPath(name), GetPath(name)));
        if (File.Exists(name)) File.Delete(System.IO.Path.GetFullPath(name));
    }

    public static void SetCustomPath(string path)
    {
        Path = path;
    }

    public static async Task MigrateToAppdataAsync(Action<string, float>? statusCallback)
    {
        int c = 0;

        foreach (string folder in FoldersToMigrate)
        {
            await MigrateFolderAsync(folder);

            c++;
            statusCallback?.Invoke($"Moving {folder} ({c}/{FoldersToMigrate.Length + FilesToMigrate.Length})",
                c / (float) FoldersToMigrate.Length / 2);
        }

        foreach (string file in FilesToMigrate)
        {
            await MigrateFileAsync(file);

            c++;
            statusCallback?.Invoke($"Moving {file} ({c}/{FoldersToMigrate.Length + FilesToMigrate.Length})",
                0.5f + c / (float) FilesToMigrate.Length / 2);
        }
    }

    public static string GetPath(string folderName)
    {
        if (Path == null) Init();

        return $"{Path}/{folderName}".FixPath();
    }

    public static string GetValidPath(string folderName)
    {
        if (Path == null) Init();

        string path = $"{Path}/{folderName}";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        return path.FixPath();
    }
}