﻿using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core.ModLoaders;

public class NeoForgeModLoaderVersion : ModLoaderVersion
{
    public bool IsNewerFormat { get; set; }
    public string FullName => $"{MinecraftVersion}-{Name}";
    public string JvmExecutablePath { get; set; }
    public string SystemFolderPath { get; init; }

    public override async Task<MinecraftVersion?> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string[] installerUrls = new[]
        {
            IsNewerFormat
                ? $"https://maven.neoforged.net/releases/net/neoforged/neoforge/{Name}/neoforge-{Name}-installer.jar"
                : $"https://maven.neoforged.net/releases/net/neoforged/forge/{FullName}/forge-{FullName}-installer.jar",
        };
        string versionName = $"{MinecraftVersion}-forge-{Name}";

        if (File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.jar") &&
            File.Exists($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"))
            return JsonSerializer.Deserialize<MinecraftVersion>(
                await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"));

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("mcLaunch", "1.0.0"));

        string filename = $"neoforge-{FullName}-installer.jar";
        string fullPath = $"{SystemFolderPath}/temp/{filename}";

        if (!Directory.Exists($"{SystemFolderPath}/temp"))
            Directory.CreateDirectory($"{SystemFolderPath}/temp");

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

        string wrapperJarPath = Path.GetFullPath($"{SystemFolderPath}/forge/target/wrapper.jar");
        string classPathSeparator = OperatingSystem.IsWindows() ? ";" : ":";

        string args =
            $"-cp {wrapperJarPath}{classPathSeparator}{Path.GetFullPath(fullPath)} portablemc.wrapper.Main \"{SystemFolderPath}\" {versionName}";

        if (!File.Exists(JvmExecutablePath)) JvmExecutablePath = "java";

        Process forgeInstaller = Process.Start(new ProcessStartInfo
        {
            Arguments = args,
            FileName = JvmExecutablePath,
            WorkingDirectory = SystemFolderPath,
            UseShellExecute = true,
            CreateNoWindow = true,
        });

        await forgeInstaller.WaitForExitAsync();

        return JsonSerializer.Deserialize<MinecraftVersion>(
            await File.ReadAllTextAsync($"{SystemFolderPath}/versions/{versionName}/{versionName}.json"));
    }
}