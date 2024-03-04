using System.Text.Json;
using System.Text.Json.Nodes;
using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;

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
        BasePath = customPath;
    }

    public MinecraftFolder SystemFolder { get; private set; }
    public string BasePath { get; }
    public event Action<string, JsonNode, string, string> GotJvmManifest;

    public Task DownloadForCurrentPlatformAsync(string name)
    {
        return DownloadAsync(Utilities.GetJavaPlatformIdentifier(), name);
    }

    public string GetJvmPath(string platform, string name)
    {
        return $"{BasePath}/{name}/{platform}";
    }

    public string GetAndPrepareJvmExecPath(string platform, string name)
    {
        if (OperatingSystem.IsMacOS())
        {
            string path = $"{BasePath}/{name}/{platform}/jre.bundle/Contents/Home/bin/java";
            //File.SetUnixFileMode(path, UnixFileMode.UserExecute);
            
            return path;
        }
        
        return $"{BasePath}/{name}/{platform}/bin/{(OperatingSystem.IsWindows() ? "javaw.exe" : "java")}";
    }

    public bool HasJvm(string platform, string name)
    {
        return Directory.Exists(GetJvmPath(platform, name));
    }

    public async Task DownloadAsync(string platform, string name)
    {
        string targetPath = GetJvmPath(platform, name);
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        JsonNode? root = await Api.GetNodeAsync(ManifestUrl);

        JsonNode platformNode = root[platform];
        JsonArray jvmNode = (JsonArray) platformNode[name];

        JvmEntry jvm = jvmNode[0].Deserialize<JvmEntry>()!;

        JsonNode jvmManifest = await Api.GetNodeAsync(jvm.Manifest.Url);
        GotJvmManifest?.Invoke(jvm.Manifest.Url, jvmManifest, platform, name);

        foreach (var (relPath, value) in jvmManifest["files"].AsObject())
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

            string macosJavaPath = $"{targetPath}/jre.bundle/Contents/Home/bin/java";
            
            await Context.Downloader.ChmodAsync(macosJavaPath, "+x");
        }
    }
}