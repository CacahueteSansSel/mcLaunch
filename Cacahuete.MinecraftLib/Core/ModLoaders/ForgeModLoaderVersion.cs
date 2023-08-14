using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class ForgeModLoaderVersion : ModLoaderVersion
{
    public string FullName => $"{MinecraftVersion}-{Name}";
    public string JvmExecutablePath { get; set; }
    public string SystemFolderPath { get; init; }

    public override async Task<MinecraftVersion?> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string[] installerUrls = new[]
        {
            $"https://maven.minecraftforge.net/net/minecraftforge/forge/{FullName}/forge-{FullName}-installer.jar",
            $"https://maven.minecraftforge.net/net/minecraftforge/forge/{FullName}-{MinecraftVersion}/forge-{FullName}-{MinecraftVersion}-installer.jar",
        };
        string versionName = $"{MinecraftVersion}-forge-{Name}";

        if (File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.jar") &&
            File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"))
            return JsonSerializer.Deserialize<MinecraftVersion>(
                await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"));

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mcLaunch", "1.0.0"));

        string filename = $"forge-{FullName}-installer.jar";
        string fullPath = $"temp/{filename}";

        if (!Directory.Exists("temp"))
            Directory.CreateDirectory("temp");

        if (!File.Exists(fullPath))
        {
            bool success = false;
            foreach (string installerUrl in installerUrls)
            {
                HttpResponseMessage resp = await client.GetAsync(installerUrl);
                if (!resp.IsSuccessStatusCode) continue;

                await File.WriteAllBytesAsync(fullPath, await resp.Content.ReadAsByteArrayAsync());
                success = true;
                
                break;
            }

            if (!success) throw new Exception("unable to find the Forge installer URL");
        }

        // TODO: Implement a new way for installing Forge
        
        string wrapperJarPath = Path.GetFullPath("forge/target/wrapper.jar");

        string args =
            $"-cp {wrapperJarPath};{Path.GetFullPath(fullPath)} portablemc.wrapper.Main \"{SystemFolderPath}\" {versionName}";

        if (!File.Exists(JvmExecutablePath)) JvmExecutablePath = "java";

        Process forgeInstaller = Process.Start(new ProcessStartInfo
        {
            Arguments = args,
            FileName = JvmExecutablePath,
            WorkingDirectory = SystemFolderPath,
            UseShellExecute = true,
            CreateNoWindow = true
        });

        await forgeInstaller.WaitForExitAsync();

        return JsonSerializer.Deserialize<MinecraftVersion>(
            await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"));
    }
}