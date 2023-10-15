using mcLaunch.Core.Utilities;

namespace mcLaunch.Core.Managers;

public static class AppdataFolderManager
{
    static string[] FoldersToMigrate = new string[]
    {
        "boxes", "cache", "forge", "launcher_crashreports", "system", "temp"
    };
    static string[] FilesToMigrate = new string[]
    {
        "settings.json"
    };
    
    public static string Path { get; private set; }

    public static bool NeedsMigration => Directory.Exists("boxes") && Directory.Exists("system");
    
    static AppdataFolderManager()
    {
        Init();
    }

    static void Init()
    {
        if (Path != null) return;
        
        string suffix = "";
        
#if DEBUG
        suffix = "_debug";
#endif
        
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/.mclaunch{suffix}";
        } else if (OperatingSystem.IsLinux())
        {
            Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.mclaunch{suffix}";
        }

        if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
    }

    static async Task MigrateFolderAsync(string name)
    {
        if (!Directory.Exists(name)) return;

        await Task.Run(() => IOUtilities.CopyDirectory(System.IO.Path.GetFullPath(name), GetValidPath(name)));
        if (Directory.Exists(name)) Directory.Delete(System.IO.Path.GetFullPath(name), true);
    }

    static async Task MigrateFileAsync(string name)
    {
        if (!File.Exists(name)) return;

        await Task.Run(() => File.Copy(System.IO.Path.GetFullPath(name), GetPath(name)));
        if (File.Exists(name)) File.Delete(System.IO.Path.GetFullPath(name));
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
            statusCallback?.Invoke($"Moving {file} ({c}/{FoldersToMigrate.Length+FilesToMigrate.Length})", 
                0.5f + c / (float)FilesToMigrate.Length / 2);
        }
    }

    public static string GetPath(string folderName)
    {
        if (Path == null) Init();

        return $"{Path}/{folderName}";
    }

    public static string GetValidPath(string folderName)
    {
        if (Path == null) Init();
        
        string path = $"{Path}/{folderName}";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        return path;
    }
}