using System.Diagnostics;
using mcLaunch.Launchsite.Models;
using mcLaunch.Launchsite.Utils;

namespace mcLaunch.Launchsite.Core;

public class LibrariesDownloader
{
    private readonly Dictionary<string, string> libVersions = new();

    public LibrariesDownloader(MinecraftFolder folder, string path = "libraries", string nativesPath = "bin") : this(
        $"{folder.Path}/{path}")
    {
        NativesPath = $"{folder.Path}/{nativesPath}/{Guid.NewGuid().ToString().Replace("-", "")}"
            .FixPath();
    }

    public LibrariesDownloader(string absolutePath)
    {
        Path = absolutePath;

        if (!Directory.Exists(Path))
            Directory.CreateDirectory(Path);
    }

    public string Path { get; }
    public string NativesPath { get; }

    public List<string> ClassPath { get; } = new();

    public LibrariesDownloader WithLibrary(string name, string version)
    {
        libVersions.Add(name, version);

        return this;
    }

    public void AddToClassPath(string path)
    {
        string filename = System.IO.Path.GetFileNameWithoutExtension(path);
        if (!filename.Contains('-'))
        {
            Debug.WriteLine($"Adding {filename} anyway : cannot deduct name and version");

            ClassPath.Add(path);
            return;
        }

        string[] tokens = filename.Split('-');
        string name = tokens[0];
        string version = tokens[1];

        if (!Version.TryParse(version, out Version toAddFileVersion))
        {
            Debug.WriteLine(
                $"Adding {name} version {toAddFileVersion} anyway : cannot parse version \"{toAddFileVersion}\"");
            ClassPath.Add(path);

            return;
        }

        foreach (string cpFile in ClassPath)
        {
            string cpFilename = System.IO.Path.GetFileNameWithoutExtension(cpFile);
            if (!cpFilename.Contains('-')) continue;

            string[] cpTokens = cpFilename.Split('-');
            string cpName = cpTokens[0];
            string cpVersion = cpTokens[1];

            if (!Version.TryParse(cpVersion, out Version cpFileVersion)) continue;

            if (cpName == name && toAddFileVersion < cpFileVersion)
            {
                Debug.WriteLine(
                    $"Refused to add {name} version {toAddFileVersion} : existing version is newer ({cpFileVersion})");
                return;
            }
        }

        ClassPath.Add(path);
    }

    private async Task<string> DownloadAsync(MinecraftVersion.ModelLibrary library)
    {
        if (!library.NeedsToDeduceUrlFromName) return null;

        LibraryName name = new(library.Name);

        string filename = library.GetFinalJarFilename();
        string path = $"{Path}/{name.Package.Replace('.', '/')}/{filename}".FixPath();
        string url = library.DeduceUrl()!;
        string dir = path.Replace(filename, "").Trim('/').FixPath();

        if (path.EndsWith(".jar") && !ClassPath.Contains(System.IO.Path.GetFullPath(path)))
            AddToClassPath(System.IO.Path.GetFullPath(path));

        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        await Context.Downloader.DownloadAsync(url, path, null);

        return path;
    }

    private async Task<string> DownloadArtifactAsync(FileArtifact artifact)
    {
        if (artifact == null) return null;

        string path = $"{Path}/{artifact.Path}".FixPath();
        string url = artifact.Url;
        string filename = System.IO.Path.GetFileName(path);
        string dir = path.Replace(filename, "").TrimEnd('/').FixPath();

        if (path.EndsWith(".jar") && !ClassPath.Contains(System.IO.Path.GetFullPath(path)))
            AddToClassPath(System.IO.Path.GetFullPath(path));

        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        await Context.Downloader.DownloadAsync(url, path, artifact.Hash);

        return path;
    }

    private string? GetLatestLwjglVersion(MinecraftVersion version)
    {
        string? versionString = null;
        ulong versionInt = 0;

        foreach (MinecraftVersion.ModelLibrary? lib in version.Libraries)
        {
            LibraryName name = new(lib.Name);

            if (name.Name != "lwjgl") continue;

            string versionNumbers = new(name.Version.Where(char.IsNumber).ToArray());
            ulong curVersionInt = ulong.Parse(versionNumbers);

            if (curVersionInt > versionInt)
            {
                versionInt = curVersionInt;
                versionString = name.Version;
            }
        }

        return versionString;
    }

    public async Task DownloadAsync(MinecraftVersion version, Action<float> percentCallback)
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

        foreach (MinecraftVersion.ModelLibrary? lib in version.Libraries)
        {
            List<string> nativeJars = new();
            LibraryName name = new(lib.Name);

            bool skip = false;
            foreach (KeyValuePair<string, string> kv in libVersions)
            {
                if (name.Name.StartsWith(kv.Key) && name.Version != null && name.Version != kv.Value)
                {
                    skip = true;
                    break;
                }
            }

            if (skip) continue;

            if (lib.Rules != null && !lib.AreRulesSatisfied()) continue;

            if (lib.Downloads == null)
            {
                if (lib.NeedsToDeduceUrlFromName) await DownloadAsync(lib);
            }
            else
            {
                // Natives
                if (lib.Downloads.Classifiers != null)
                {
                    if (lib.Downloads.Classifiers.NativesLinux != null && OperatingSystem.IsLinux())
                    {
                        string path = await DownloadArtifactAsync(lib.Downloads.Classifiers.NativesLinux);
                        if (!string.IsNullOrEmpty(path)) nativeJars.Add(path);
                    }

                    if (lib.Downloads.Classifiers.NativesOSX != null && OperatingSystem.IsMacOS())
                    {
                        string path = await DownloadArtifactAsync(lib.Downloads.Classifiers.NativesOSX);
                        if (!string.IsNullOrEmpty(path)) nativeJars.Add(path);
                    }

                    if (lib.Downloads.Classifiers.NativesWindows != null && OperatingSystem.IsWindows())
                    {
                        string path = await DownloadArtifactAsync(lib.Downloads.Classifiers.NativesWindows);
                        if (!string.IsNullOrEmpty(path)) nativeJars.Add(path);
                    }
                }

                await DownloadArtifactAsync(lib.Downloads.Artifact);

                // extracting dlls
                if (!Directory.Exists(NativesPath)) Directory.CreateDirectory(NativesPath);

                foreach (string jar in nativeJars) await Context.Downloader.ExtractAsync(jar, NativesPath);
            }

            cur++;
            percentCallback?.Invoke(cur / (float)version.Libraries.Length);
        }

        libVersions.Clear();
    }
}