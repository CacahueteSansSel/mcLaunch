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

    public override async Task<Result<MinecraftVersion>> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string[] installerUrls =
        {
            $"https://maven.minecraftforge.net/net/minecraftforge/forge/{FullName}/forge-{FullName}-installer.jar",
            $"https://maven.minecraftforge.net/net/minecraftforge/forge/{FullName}-{MinecraftVersion}/forge-{FullName}-{MinecraftVersion}-installer.jar"
        };

        return await GetForgeMinecraftVersionAsync(minecraftVersionId, installerUrls, "Forge");
    }

    protected async Task<Result<MinecraftVersion>> GetForgeMinecraftVersionAsync(string minecraftVersionId,
        string[] installerUrls, string slug)
    {
        string versionName = $"{MinecraftVersion}-{slug.ToLower()}-{Name}";

        if (File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.jar") &&
            File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"))
            return new Result<MinecraftVersion>(JsonSerializer.Deserialize<MinecraftVersion>(
                await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json")));

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mcLaunch", "1.0.0"));

        string filename = $"{slug.ToLower()}-{FullName}-installer.jar";
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

            if (successfulInstallerUrl == null)
            {
                return Result<MinecraftVersion>.Error($"The {slug} installer file cannot be found for" +
                                                      $" version {minecraftVersionId} : the version may be too old" +
                                                      $" or have been deleted since");
            }

            await Context.Downloader.BeginSectionAsync($"{slug} {Name} installer", false);
            await Context.Downloader.DownloadAsync(successfulInstallerUrl, fullPath, null);
            await Context.Downloader.EndSectionAsync(false);

            await Context.Downloader.FlushAsync();
        }

        ForgeInstallResult result = await ForgeInstaller.InstallAsync(new ForgeInstallerFile(fullPath),
            SystemFolderPath, JvmExecutablePath, $"{SystemFolderPath}/temp", slug);
        return new Result<MinecraftVersion>(result.MinecraftVersion);
    }
}