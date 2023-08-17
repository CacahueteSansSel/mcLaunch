using System.IO.Compression;
using System.Net;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core;

public class LibrariesDownloader
{
    Dictionary<string, string> libVersions = new();
    public string Path { get; }
    public string NativesPath { get; }

    public List<string> ClassPath { get; private set; } = new();

    public LibrariesDownloader(MinecraftFolder folder, string path = "libraries", string nativesPath = "bin")
    {
        Path = $"{folder.Path}/{path}";
        NativesPath = $"{folder.Path}/{nativesPath}/{Guid.NewGuid().ToString().Replace("-", "")}";
    }

    public LibrariesDownloader(string absolutePath)
    {
        Path = absolutePath;
    }

    public LibrariesDownloader WithLibrary(string name, string version)
    {
        libVersions.Add(name, version);

        return this;
    }

    async Task<string> DownloadAsync(MinecraftVersion.ModelLibrary library,
        bool force)
    {
        if (!library.NeedsToDeduceUrlFromName) return null;

        LibraryName name = new LibraryName(library.Name);

        string filename = library.GetFinalJarFilename();
        string path = $"{Path}/{name.Package.Replace('.', '/')}/{filename}";
        string url = library.DeduceUrl()!;
        string dir = path.Replace(filename, "").Trim('/');

        if (path.EndsWith(".jar") && !ClassPath.Contains(System.IO.Path.GetFullPath(path))) 
            ClassPath.Add(System.IO.Path.GetFullPath(path));

        if (File.Exists(path) && !force) return path;

        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        await Context.Downloader.DownloadAsync(url, path);

        return path;
    }

    async Task<string> DownloadArtifactAsync(FileArtifact artifact,
        bool force)
    {
        if (artifact == null) return null;

        string path = $"{Path}/{artifact.Path}";
        string url = artifact.Url;
        string filename = System.IO.Path.GetFileName(path);
        string dir = path.Replace(filename, "").Trim('/');

        if (path.EndsWith(".jar") && !ClassPath.Contains(System.IO.Path.GetFullPath(path))) 
            ClassPath.Add(System.IO.Path.GetFullPath(path));

        if (File.Exists(path) && !force) return path;

        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        await Context.Downloader.DownloadAsync(url, path);

        return path;
    }

    string? GetLatestLwjglVersion(MinecraftVersion version)
    {
        string? versionString = null;
        ulong versionInt = 0;

        foreach (var lib in version.Libraries)
        {
            LibraryName name = new LibraryName(lib.Name);

            if (name.Name != "lwjgl") continue;

            string versionNumbers = new string(name.Version.Where(char.IsNumber).ToArray());
            ulong curVersionInt = ulong.Parse(versionNumbers);
            
            if (curVersionInt > versionInt)
            {
                versionInt = curVersionInt;
                versionString = name.Version;
            }
        }

        return versionString;
    }

    public async Task DownloadAsync(MinecraftVersion version, Action<float> percentCallback, bool force = false)
    {
        int cur = 0;

        ClassPath.Clear();

        if (version.Assets != "pre-1.6" && version.Assets != "legacy")
        {
            // Try to force the latest LWJGL version
            string? latestLwjglVersion = GetLatestLwjglVersion(version);
            if (latestLwjglVersion != null) 
                WithLibrary("lwjgl", latestLwjglVersion);
        }

        foreach (var lib in version.Libraries)
        {
            List<string> nativeJars = new();
            LibraryName name = new LibraryName(lib.Name);

            bool skip = false;
            foreach (var kv in libVersions)
            {
                if (name.Name.StartsWith(kv.Key) && name.Version != null && name.Version != kv.Value)
                {
                    skip = true;
                    break;
                }
            }

            if (skip) continue;

            if (lib.Rules != null)
            {
                bool abort = false;
                foreach (var rule in lib.Rules)
                {
                    bool satisfied = rule.Os == null || rule.Os.CheckIfSatisfied();
                    
                    // Invert the boolean value if it's a "disallow" rule
                    if (rule.Action == "disallow") satisfied = !satisfied;
                    
                    if (!satisfied)
                    {
                        abort = true;
                        break;
                    }
                }

                if (abort) continue;
            }

            if (lib.Downloads == null)
            {
                if (lib.NeedsToDeduceUrlFromName) await DownloadAsync(lib, force);
            }
            else
            {
                // Natives
                if (lib.Downloads.Classifiers != null)
                {
                    if (lib.Downloads.Classifiers.NativesLinux != null && OperatingSystem.IsLinux())
                    {
                        string path = await DownloadArtifactAsync(lib.Downloads.Classifiers.NativesLinux, force);
                        if (!string.IsNullOrEmpty(path)) nativeJars.Add(path);
                    }

                    if (lib.Downloads.Classifiers.NativesOSX != null && OperatingSystem.IsMacOS())
                    {
                        string path = await DownloadArtifactAsync(lib.Downloads.Classifiers.NativesOSX, force);
                        if (!string.IsNullOrEmpty(path)) nativeJars.Add(path);
                    }

                    if (lib.Downloads.Classifiers.NativesWindows != null && OperatingSystem.IsWindows())
                    {
                        string path = await DownloadArtifactAsync(lib.Downloads.Classifiers.NativesWindows, force);
                        if (!string.IsNullOrEmpty(path)) nativeJars.Add(path);
                    }
                }
            
                await DownloadArtifactAsync(lib.Downloads.Artifact, force);

                // extracting dlls
                if (!Directory.Exists(NativesPath)) Directory.CreateDirectory(NativesPath);

                foreach (string jar in nativeJars)
                {
                    await Context.Downloader.ExtractAsync(jar, NativesPath);
                }
            }

            cur++;
            percentCallback?.Invoke(cur / (float) version.Libraries.Length);
        }
        
        libVersions.Clear();
    }
}