﻿using System.Diagnostics;
using System.IO.Compression;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class DirectJarMergingModLoaderSupport : ModLoaderSupport
{
    public override string Id { get; } = "directjar";
    public override string Name { get; set; } = "Jar mods";
    public override string Type { get; set; } = "vanilla";
    public override ModLoaderVersion LatestVersion { get; set; }
    public override bool IsAdvanced => true;

    public override async Task<MinecraftVersion> PostProcessMinecraftVersionAsync(MinecraftVersion minecraftVersion)
    {
        minecraftVersion.Id += "_djar";

        return minecraftVersion;
    }

    public override async Task<ModLoaderVersion[]?> GetVersionsAsync(string minecraftVersion) =>
    [
        new DirectJarMergingModLoaderVersion { Name = minecraftVersion, MinecraftVersion = minecraftVersion }
    ];

    public override async Task<Result> FinalizeMinecraftInstallationAsync(string jarFilename, string[] additionalFiles)
    {
        // We need to take Minecraft's jar file & merging it with the additional files
        // We also need to remove the META-INF folder to avoid the Jvm running integrity checks

        if (additionalFiles.Length == 0) return new Result();

        using ZipArchive minecraftJar = new(new FileStream(jarFilename, FileMode.Open, FileAccess.ReadWrite),
            ZipArchiveMode.Update);

        ZipArchiveEntry[] entries =
            minecraftJar.Entries.Where(entry => entry.FullName.StartsWith("META-INF")).ToArray();
        foreach (ZipArchiveEntry? entry in entries)
            entry.Delete();
        minecraftJar.GetEntry("META-INF")?.Delete();

        foreach (string additionalFile in additionalFiles)
        {
            if (!additionalFile.EndsWith(".zip") && !additionalFile.EndsWith(".jar")) continue;

            using ZipArchive modFile = new(new FileStream(additionalFile, FileMode.Open));
            foreach (ZipArchiveEntry entry in modFile.Entries)
            {
                long entryLength = entry.Length;
                ZipArchiveEntry? mcEntry = minecraftJar.GetEntry(entry.FullName);
                mcEntry?.Delete();

                mcEntry = minecraftJar.CreateEntry(entry.FullName);

                await using Stream targetStream = mcEntry.Open();
                await using Stream sourceStream = entry.Open();

                await sourceStream.CopyToAsync(targetStream);

                Debug.Assert(targetStream.Length == entryLength);
            }
        }

        return new Result();
    }
}