using System.Diagnostics;
using System.IO.Compression;
using mcLaunch.Launchsite.Models;
using mcLaunch.Launchsite.Utils;

namespace mcLaunch.Launchsite.Core.ModLoaders.Forge;

// Based on portablemc's Forge installer
// https://github.com/mindstorm38/portablemc/blob/main/portablemc/forge.py

public static class ForgeInstaller
{
    public static async Task<Result<ForgeInstallResult>> InstallAsync(ForgeInstallerFile installerFile,
        string minecraftFolderPath, string jvmExecutablePath, string tempPath, string slug = "Forge")
    {
        // Install the vanilla minecraft version files (jar & json)
        await Context.Downloader.BeginSectionAsync($"{slug} {installerFile.Name.Trim()}", false);
        
        // An attempt to fix the "java opens in TextEdit" bug
        if (OperatingSystem.IsMacOS()) File.SetUnixFileMode(jvmExecutablePath, UnixFileMode.UserExecute);

        if (installerFile.EmbeddedForgeJarPath != null)
        {
            string targetFilename =
                $"{minecraftFolderPath}/libraries/{installerFile.EmbeddedForgeJarLibraryName!.MavenFilename}";
            string folder = targetFilename.Replace(Path.GetFileName(targetFilename), 
                string.Empty).Trim();

            if (!Directory.Exists(folder)) 
                Directory.CreateDirectory(folder);
            
            installerFile.ExtractFile(installerFile.EmbeddedForgeJarPath!, targetFilename);

            if (!installerFile.IsV2)
            {
                await Context.Downloader.EndSectionAsync(false);
                return new Result<ForgeInstallResult>(new ForgeInstallResult(installerFile.Version));
            }
        }

        string forgeVersionPath = $"{minecraftFolderPath}/versions/{installerFile.Version.Id}";
        string vanillaVersionPath = $"{minecraftFolderPath}/versions/{installerFile.MinecraftVersionId}";

        if (File.Exists($"{vanillaVersionPath}/{installerFile.MinecraftVersionId}.jar"))
        {
            Directory.CreateDirectory(forgeVersionPath);
            File.Copy($"{vanillaVersionPath}/{installerFile.MinecraftVersionId}.jar",
                $"{forgeVersionPath}/{installerFile.Version.Id}.jar", true);
        }
        else
        {
            await Context.Downloader.EndSectionAsync(false);
            
            return Result<ForgeInstallResult>.Error(
                $"The {slug} installer needs the base vanilla version {installerFile.MinecraftVersionId} and it" +
                $" is not installed right now. You shouldn't see this message as mcLaunch should download it" +
                $" automatically. You can manually download Minecraft {installerFile.MinecraftVersionId} via" +
                $" running a vanilla FastLaunch instance or creating a new vanilla box");
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

        await Context.Downloader.EndSectionAsync(false);
        await Context.Downloader.FlushAsync();

        Dictionary<string, string> variables = new(installerFile.DataVariables);
        variables.Add("SIDE", "client");
        variables.Add("MINECRAFT_JAR", $"{forgeVersionPath}/{installerFile.Version.Id}.jar");
        
        await Context.Downloader.BeginSectionAsync($"Installing {slug} {installerFile.Name.Trim()}", true);

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

        bool error = false;
        for (int retryCount = 0; retryCount < 5; retryCount++)
        {
            int processorCount = 0;
            
            foreach (ForgeInstallerFile.PostProcessor processor in installerFile.Processors)
            {
                ForgeInstallerFile.LibraryEntry? library = installerFile.GetProcessorLibrary(processor);
                if (library is null) continue;

                string libraryFilename = $"libraries/{library.ArtifactPath}";

                using var zip = new ZipArchive(new FileStream($"{minecraftFolderPath}/{libraryFilename}", FileMode.Open));
                var dict = MetaInfParser.Parse(zip);
                string mainClass = dict["Main-Class"];
                string procClassPath = string.Join(Path.PathSeparator, processor.Classpath
                    .Select(cp =>
                        cp.Contains(':') ? $"{minecraftFolderPath}/libraries/{new LibraryName(cp).MavenFilename}" : cp));
                string[] arguments = BuildArgumentList(processor.Arguments, variables, minecraftFolderPath);

                ProcessStartInfo processStartInfo = new()
                {
                    FileName = jvmExecutablePath,
                    WorkingDirectory = minecraftFolderPath,
                    Arguments = $"-cp \"{libraryFilename}{Path.PathSeparator}{procClassPath}\" {mainClass}" +
                                $" {string.Join(' ', arguments)}",
                    UseShellExecute = false
                };

                Process process = Process.Start(processStartInfo)!;
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    error = true;
                    break;
                }

                await Context.Downloader.SetSectionProgressAsync(processor.JarName.Name,
                    (float) processorCount / installerFile.Processors.Count);
                processorCount++;
            }

            if (error) continue;
            
            break;
        }

        if (error)
        {
            return Result<ForgeInstallResult>.Error("One or more installer processor failed to execute properly. " +
                                                    "This is a problem within mcLaunch, please report it to CacahueteDev");
        }

        await Context.Downloader.EndSectionAsync(true);

        return new Result<ForgeInstallResult>(new ForgeInstallResult(installerFile.Version));
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

                finalArgs[i] = args[i].Replace($"{{{kv.Key}}}", kv.Value.Contains(' ') 
                    ? $"\"{kv.Value}\"" : kv.Value);
                break;
            }

            // We need to transform every maven thing into proper paths
            if (finalArgs[i].StartsWith('[') && finalArgs[i].EndsWith(']'))
                finalArgs[i] = $"\"{minecraftFolderPath}/libraries/{new LibraryName(finalArgs[i]).MavenFilename}\"";
        }

        return finalArgs;
    }
}

public record ForgeInstallResult(MinecraftVersion MinecraftVersion);