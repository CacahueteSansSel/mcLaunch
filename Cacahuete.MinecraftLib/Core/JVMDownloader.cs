using System.Text.Json;
using System.Text.Json.Nodes;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core;

public class JVMDownloader
{
    public const string ManifestUrl =
        "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";
    public MinecraftFolder SystemFolder { get; private set; }
    public string BasePath { get; private set; }
    
    public JVMDownloader(MinecraftFolder systemFolder) : this($"{systemFolder.CompletePath}/runtime")
    {
        SystemFolder = systemFolder;
    }

    public JVMDownloader(string customPath)
    {
        BasePath = customPath;
    }

    public Task DownloadForCurrentPlatformAsync(string name)
        => DownloadAsync(Utilities.GetJavaPlatformIdentifier(), name);

    public string GetJVMPath(string platform, string name)
        => $"{BasePath}/{name}/{platform}";

    public string GetJVMExecutablePath(string platform, string name)
        => $"{BasePath}/{name}/{platform}/bin/{(OperatingSystem.IsWindows() ? "javaw.exe" : "java")}";

    public bool HasJVM(string platform, string name)
        => Directory.Exists(GetJVMPath(platform, name));

    public async Task DownloadAsync(string platform, string name)
    {
        string targetPath = GetJVMPath(platform, name);
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
        
        JsonNode? root = await Api.GetNodeAsync(ManifestUrl);

        JsonNode platformNode = root[platform];
        JsonArray jvmNode = (JsonArray)platformNode[name];

        JVMEntry jvm = jvmNode[0].Deserialize<JVMEntry>()!;

        JsonNode jvmManifest = await Api.GetNodeAsync(jvm.Manifest.Url);

        foreach (var (relPath, value) in jvmManifest["files"].AsObject())
        {
            string fullPath = $"{targetPath}/{relPath}";
            JVMFileEntry? file = value.Deserialize<JVMFileEntry>();

            switch (file.Type)
            {
                case "directory":
                    Directory.CreateDirectory(fullPath);
                    break;
                case "file":
                    await Context.Downloader.DownloadAsync(file.Downloads.Raw.Url, fullPath);

                    if (file.Executable) await Context.Downloader.ChmodAsync(fullPath, "+x");
                    break;
            }
        }
    }
}