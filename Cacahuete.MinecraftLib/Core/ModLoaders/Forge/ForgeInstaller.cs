using System.Diagnostics;
using System.IO.Compression;
using Cacahuete.MinecraftLib.Models;
using Cacahuete.MinecraftLib.Utils;

namespace Cacahuete.MinecraftLib.Core.ModLoaders.Forge;

// Based on portablemc's Forge installer
// https://github.com/mindstorm38/portablemc/blob/main/portablemc/forge.py

public static class ForgeInstaller
{
    public static async Task<ForgeInstallResult> InstallAsync(ForgeInstallerFile installerFile,
        string minecraftFolderPath, string jvmExecutablePath, string tempPath)
    {
        // Install the vanilla minecraft version files (jar & json)
        await Context.Downloader.BeginSectionAsync($"Forge {installerFile.Name.Trim()}");

        if (!installerFile.IsV2)
        {
            string targetFilename =
                $"{minecraftFolderPath}/libraries/{installerFile.EmbeddedForgeJarLibraryName!.MavenFilename}";
            string folder = targetFilename.Replace(Path.GetFileName(targetFilename), 
                string.Empty).Trim();

            if (!Directory.Exists(folder)) 
                Directory.CreateDirectory(folder);
            
            installerFile.ExtractFile(installerFile.EmbeddedForgeJarPath!, targetFilename);

            await Context.Downloader.EndSectionAsync();
            return new ForgeInstallResult(installerFile.Version);
        }

        string forgeVersionPath = $"{minecraftFolderPath}/versions/{installerFile.Version.Id}";
        string vanillaVersionPath = $"{minecraftFolderPath}/versions/{installerFile.MinecraftVersionId}";

        if (File.Exists($"{vanillaVersionPath}/{installerFile.MinecraftVersionId}.jar"))
        {
            Directory.CreateDirectory(forgeVersionPath);
            File.Copy($"{vanillaVersionPath}/{installerFile.MinecraftVersionId}.jar",
                $"{forgeVersionPath}/{installerFile.Version.Id}.jar", true);
        }

        foreach (ForgeInstallerFile.LibraryEntry library in installerFile.Libraries)
        {
            string targetLibPath = $"{minecraftFolderPath}/libraries/{library.ArtifactPath}";

            if (library.IsLocal)
            {
                installerFile.ExtractLocalLibrary(library, targetLibPath);
                continue;
            }

            await Context.Downloader.DownloadAsync(library.ArtifactUrl, targetLibPath, null);
        }

        await Context.Downloader.EndSectionAsync();
        await Context.Downloader.FlushAsync();

        Dictionary<string, string> variables = new(installerFile.DataVariables);
        variables.Add("SIDE", "client");
        variables.Add("MINECRAFT_JAR", $"{forgeVersionPath}/{installerFile.Version.Id}.jar");

        // Extract any needed file in the temp folder
        foreach (var kv in installerFile.DataVariables)
        {
            string value = kv.Value.Replace("\\", "/").TrimStart('\\', '/');
            if (!installerFile.HasFile(value)) continue;

            string folder = value.Replace(Path.GetFileName(value), "").Trim();

            if (!Directory.Exists($"{tempPath}/{folder}"))
                Directory.CreateDirectory($"{tempPath}/{folder}");

            installerFile.ExtractFile(value, $"{tempPath}/{value}");

            variables[kv.Key] = $"{tempPath}/{value}";
        }

        foreach (ForgeInstallerFile.PostProcessor processor in installerFile.Processors)
        {
            ForgeInstallerFile.LibraryEntry? library = installerFile.GetProcessorLibrary(processor);
            if (library is null) continue;

            string libraryFilename = $"{minecraftFolderPath}/libraries/{library.ArtifactPath}";

            using var zip = new ZipArchive(new FileStream(libraryFilename, FileMode.Open));
            var dict = MetaInfParser.Parse(zip);
            string mainClass = dict["Main-Class"];
            string procClassPath = string.Join(Path.PathSeparator, processor.Classpath
                .Select(cp =>
                    cp.Contains(':') ? $"{minecraftFolderPath}/libraries/{new LibraryName(cp).MavenFilename}" : cp));
            string[] arguments = BuildArgumentList(processor.Arguments, variables, minecraftFolderPath);

            ProcessStartInfo processStartInfo = new()
            {
                FileName = jvmExecutablePath,
                Arguments = $"-cp {libraryFilename}{Path.PathSeparator}{procClassPath} {mainClass}" +
                            $" {string.Join(' ', arguments)}",
                UseShellExecute = false
            };

            Process process = Process.Start(processStartInfo)!;
            await process.WaitForExitAsync();
        }

        return new ForgeInstallResult(installerFile.Version);
    }

    static string[] BuildArgumentList(string[] args, Dictionary<string, string> variables, string minecraftFolderPath)
    {
        string[] finalArgs = new string[args.Length];
        Array.Copy(args, finalArgs, args.Length);

        for (int i = 0; i < args.Length; i++)
        {
            foreach (var kv in variables)
            {
                if (!args[i].Contains($"{{{kv.Key}}}")) continue;

                finalArgs[i] = args[i].Replace($"{{{kv.Key}}}", kv.Value);
                break;
            }

            // We need to transform every maven thing into proper paths
            if (finalArgs[i].StartsWith('[') && finalArgs[i].EndsWith(']'))
                finalArgs[i] = $"{minecraftFolderPath}/libraries/{new LibraryName(finalArgs[i]).MavenFilename}";
        }

        return finalArgs;
    }
}

public record ForgeInstallResult(MinecraftVersion MinecraftVersion);