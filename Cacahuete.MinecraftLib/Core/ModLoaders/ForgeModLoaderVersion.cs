using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Cacahuete.MinecraftLib.Core.ModLoaders.Forge;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class ForgeModLoaderVersion : ModLoaderVersion
{
    public string FullName => $"{MinecraftVersion}-{Name}";
    public string JvmExecutablePath { get; set; }
    public string SystemFolderPath { get; init; }

    public override async Task<MinecraftVersion?> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string[] installerUrls =
        {
            $"https://maven.minecraftforge.net/net/minecraftforge/forge/{FullName}/forge-{FullName}-installer.jar",
            $"https://maven.minecraftforge.net/net/minecraftforge/forge/{FullName}-{MinecraftVersion}/forge-{FullName}-{MinecraftVersion}-installer.jar"
        };
        string versionName = $"{MinecraftVersion}-forge-{Name}";

        if (File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.jar") &&
            File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"))
            return JsonSerializer.Deserialize<MinecraftVersion>(
                await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"));

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mcLaunch", "1.0.0"));

        string filename = $"forge-{FullName}-installer.jar";
        string fullPath = $"{SystemFolderPath}/temp/{filename}";

        if (!Directory.Exists($"{SystemFolderPath}/temp"))
            Directory.CreateDirectory($"{SystemFolderPath}/temp");

        if (!File.Exists(fullPath))
        {
            string? successfulInstallerUrl = null;
            foreach (string installerUrl in installerUrls)
            {
                CancellationTokenSource cancelSource = new();
                HttpResponseMessage resp = await client.GetAsync(installerUrl, 
                    HttpCompletionOption.ResponseHeadersRead, cancelSource.Token);
                if (!resp.IsSuccessStatusCode) continue;
                
                await cancelSource.CancelAsync();
                successfulInstallerUrl = installerUrl;
                
                break;
            }

            if (successfulInstallerUrl == null) throw new Exception("unable to find the Forge installer URL");

            await Context.Downloader.BeginSectionAsync($"Forge {Name} installer");
            await Context.Downloader.DownloadAsync(successfulInstallerUrl, fullPath, null);
            await Context.Downloader.EndSectionAsync();
            
            await Context.Downloader.FlushAsync();
        }

        ForgeInstallResult result = await ForgeInstaller.InstallAsync(new ForgeInstallerFile(fullPath), 
            SystemFolderPath, JvmExecutablePath, $"{SystemFolderPath}/temp");
        return result.MinecraftVersion;
    }
}