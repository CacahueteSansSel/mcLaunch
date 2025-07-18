using System.Text.Json;
using System.Text.Json.Nodes;
using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;
using mcLaunch.Launchsite.Utils;

namespace mcLaunch.Launchsite.Core;

public class JvmDownloader
{
    public const string ManifestUrl =
        "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";

    public JvmDownloader(MinecraftFolder systemFolder) : this($"{systemFolder.CompletePath}/runtime")
    {
        SystemFolder = systemFolder;
    }

    public JvmDownloader(string customPath)
    {
        BasePath = customPath.FixPath();
    }

    public MinecraftFolder SystemFolder { get; private set; }
    public string BasePath { get; }
    public event Action<string, JsonNode, string, string> GotJvmManifest;

    public Task DownloadForCurrentPlatformAsync(string name) =>
        DownloadAsync(Utilities.GetJavaPlatformIdentifier(), name);

    public string GetJvmPath(string platform, string name) => $"{BasePath}/{name}/{platform}".FixPath();

    public string GetAndPrepareJvmExecPath(string platform, string name)
    {
        if (OperatingSystem.IsMacOS())
        {
            string path = $"{BasePath}/{name}/{platform}/jre.bundle/Contents/Home/bin/java".FixPath();
            //File.SetUnixFileMode(path, UnixFileMode.UserExecute);

            return path;
        }

        return $"{BasePath}/{name}/{platform}/bin/{(OperatingSystem.IsWindows() ? "javaw.exe" : "java")}".FixPath();
    }

    public bool HasJvm(string platform, string name) => Directory.Exists(GetJvmPath(platform, name));

    public async Task DownloadAsync(string platform, string name)
    {
        string targetPath = GetJvmPath(platform, name);

        JsonNode? root = await Api.GetNodeAsync(ManifestUrl);

        List<string> platforms = [platform];
        JvmEntry jvm = null;

        if (platform == "mac-os-arm64")
        {
            // Adding macos intel platform if the java version we want is too old to even exist on apple silicon platforms
            platforms.Add("mac-os");
        }

        foreach (string curPlatform in platforms)
        {
            JsonNode platformNode = root[curPlatform];
            JsonArray jvmNode = (JsonArray)platformNode[name];
            if (jvmNode.Count == 0) continue;

            jvm = jvmNode[0].Deserialize<JvmEntry>()!;
            break;
        }

        if (jvm == null) throw new Exception($"Failed to download {name} for {platform}");
        
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        JsonNode jvmManifest = await Api.GetNodeAsync(jvm.Manifest.Url);
        GotJvmManifest?.Invoke(jvm.Manifest.Url, jvmManifest, platform, name);

        foreach ((string? relPath, JsonNode? value) in jvmManifest["files"].AsObject())
        {
            string fullPath = $"{targetPath}/{relPath}";
            JvmFileEntry? file = value.Deserialize<JvmFileEntry>();

            switch (file.Type)
            {
                case "directory":
                    Directory.CreateDirectory(fullPath);
                    break;
                case "file":
                    await Context.Downloader.DownloadAsync(file.Downloads.Raw.Url, fullPath, file.Downloads.Raw.Hash);

                    if (file.Executable) await Context.Downloader.ChmodAsync(fullPath, "+x");
                    break;
            }
        }

        if (OperatingSystem.IsMacOS())
        {
            // On macOS, we have a .bundle folder
            // We need to make the java executable inside this bundle folder executable

            string macosJavaPath = $"{targetPath}/jre.bundle/Contents/Home/bin/java".FixPath();

            await Context.Downloader.ChmodAsync(macosJavaPath, "+x");
        }
    }
}