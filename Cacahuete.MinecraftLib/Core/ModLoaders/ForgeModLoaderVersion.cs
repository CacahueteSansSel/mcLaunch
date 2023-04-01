using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class ForgeModLoaderVersion : ModLoaderVersion
{
    public string FullName => $"{MinecraftVersion}-{Name}";
    public string JvmExecutablePath { get; init; }
    public string SystemFolderPath { get; init; }

    public string InstallerUrl =>
        $"https://maven.minecraftforge.net/net/minecraftforge/forge/{FullName}/forge-{FullName}-installer.jar";

    public override async Task<MinecraftVersion?> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string versionName = $"{MinecraftVersion}-forge-{Name}";

        if (File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.jar") &&
            File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"))
            return JsonSerializer.Deserialize<MinecraftVersion>(
                await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"));

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ddLaunch", "1.0.0"));

        string filename = $"forge-{FullName}-installer.jar";
        string fullPath = $"temp/{filename}";

        if (!Directory.Exists("temp"))
            Directory.CreateDirectory("temp");

        if (!File.Exists(fullPath))
        {
            HttpResponseMessage resp = await client.GetAsync(InstallerUrl);
            resp.EnsureSuccessStatusCode();

            await File.WriteAllBytesAsync(fullPath, await resp.Content.ReadAsByteArrayAsync());
        }

        string wrapperJarPath = Path.GetFullPath("forge/target/wrapper.jar");

        string args =
            $"-cp {wrapperJarPath};{Path.GetFullPath(fullPath)} portablemc.wrapper.Main \"{SystemFolderPath}\" {versionName}";

        Process forgeInstaller = Process.Start(new ProcessStartInfo
        {
            Arguments = args,
            FileName = JvmExecutablePath,
            WorkingDirectory = SystemFolderPath
        });

        await forgeInstaller.WaitForExitAsync();

        return JsonSerializer.Deserialize<MinecraftVersion>(
            await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"));
    }
}