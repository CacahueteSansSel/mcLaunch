using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Models;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class DebugBoxCrashPopup : Window
{
    public DebugBoxCrashPopup()
    {
        InitializeComponent();
        
        DataContext = new DebugBoxCrashWindowData
        {
            Box = null, 
            ProcessArguments = [], 
            ProcessClassPath = [],
            BoxSettings = [
                new DebugBoxSettingEntry("Title", "Hello, World !"),
                new DebugBoxSettingEntry("Description (unused)", "test")
            ]
        };
    }

    public DebugBoxCrashPopup(Box box, Process javaProcess) : this()
    {
        Title = "Loading...";
        LoadBoxAsync(box, javaProcess);
    }

    async void LoadBoxAsync(Box box, Process javaProcess)
    {
        List<DebugBoxSettingEntry> boxSettings = [];
        List<DebugBoxSettingEntry> mcSettings = [];
        List<DebugBoxMinecraftLibrary> libs = [];

        ArgumentsParser argParser = new ArgumentsParser(javaProcess.StartInfo.Arguments);
        char classPathCharacter = OperatingSystem.IsWindows() ? ';' : ':';
        string[] classPath = argParser.Get("cp")!.Split(classPathCharacter);
        MinecraftVersion? mcVersion = await MinecraftManager.GetAsync(box.Manifest.Version);
        
        await box.CreateMinecraftAsync();

        foreach (MinecraftVersion.ModelLibrary lib in mcVersion.Libraries)
        {
            bool isNative = lib.Natives != null && lib.Downloads.Classifiers != null;
            string desc = $"{(isNative ? "(Native)" : "(Java)")} {lib.Name}";
            bool satisfied = lib.AreRulesSatisfied(out MinecraftVersion.ModelLibrary.ModelRule? rule);

            if (rule != null && rule.Os != null) 
                desc += $" - {rule.Action} {rule.Os.Name.Trim()} ({(string.IsNullOrEmpty(rule.Os.Arch) ? "no-arch" : "").Trim()})";

            if (lib.Downloads != null)
            {
                if (!string.IsNullOrWhiteSpace(lib.Downloads.Artifact?.Url))
                    desc += $"\n -  artifact: {lib.Downloads.Artifact.Url}";
            
                if (!string.IsNullOrWhiteSpace(lib.Downloads.Classifiers?.NativesWindows?.Url))
                    desc += $"\n -  classifier (windows): {lib.Downloads.Classifiers.NativesWindows.Url}";
                if (!string.IsNullOrWhiteSpace(lib.Downloads.Classifiers?.NativesOSX?.Url))
                    desc += $"\n -  classifier (mac-os): {lib.Downloads.Classifiers.NativesOSX.Url}";
                if (!string.IsNullOrWhiteSpace(lib.Downloads.Classifiers?.NativesLinux?.Url))
                    desc += $"\n -  classifier (linux): {lib.Downloads.Classifiers.NativesLinux.Url}";
            }
                
            libs.Add(new DebugBoxMinecraftLibrary(desc, satisfied));
        }
        
        foreach (string lib in Directory.GetFiles(box.Minecraft.NativesFolderPath))
            libs.Add(new DebugBoxMinecraftLibrary($"(Shared) {lib}", true));
        
        boxSettings.Add(new DebugBoxSettingEntry("Title", box.Manifest.Name));
        boxSettings.Add(new DebugBoxSettingEntry("Author", box.Manifest.Author));
        boxSettings.Add(new DebugBoxSettingEntry("Box type", box.Manifest.Type.ToString()));
        boxSettings.Add(new DebugBoxSettingEntry("Box manifest version", box.Manifest.ManifestVersion.ToString() ?? "None"));
        boxSettings.Add(new DebugBoxSettingEntry("Last launched", box.Manifest.LastLaunchTime.ToLongDateString()));
        boxSettings.Add(new DebugBoxSettingEntry("Description (unused)", box.Manifest.Description));
        boxSettings.Add(new DebugBoxSettingEntry("Path", box.Path));
        boxSettings.Add(new DebugBoxSettingEntry("Mod loader", box.ModLoader?.Name ?? "None"));
        boxSettings.Add(new DebugBoxSettingEntry("Mod loader version", box.Manifest.ModLoaderVersion));

        if (mcVersion != null)
        {
            mcSettings.Add(new DebugBoxSettingEntry("Version", mcVersion.Id));
            mcSettings.Add(new DebugBoxSettingEntry("JVM", mcVersion.JavaVersion?.Component ?? "Unknown"));
            mcSettings.Add(new DebugBoxSettingEntry("Main Class", mcVersion.MainClass));
            mcSettings.Add(new DebugBoxSettingEntry("Asset Index", mcVersion.AssetIndex.Id));
        }
        
        DataContext = new DebugBoxCrashWindowData
        {
            Box = box, 
            ProcessArguments = argParser.Dictionary
                .Where(arg => arg.Key != "accessToken" && arg.Key != "cp")
                .Select(arg => new DebugBoxSettingEntry(arg.Key, arg.Value)).ToArray(), 
            ProcessClassPath = classPath.Select(cp => new DebugBoxClassPathEntry(cp)).ToArray(),
            BoxSettings = boxSettings.ToArray(),
            MinecraftSettings = mcSettings.ToArray(),
            Libraries = libs.ToArray()
        };

        Title = $"Debug crashing box {box.Manifest.Name}";
    }
}

public class DebugBoxSettingEntry(string title, string contents)
{
    public string Title { get; set; } = title;
    public string Contents { get; set; } = contents;
}

public class DebugBoxCrashWindowData
{
    public Box Box { get; set; }
    public DebugBoxSettingEntry[] BoxSettings { get; set; }
    public DebugBoxSettingEntry[] MinecraftSettings { get; set; }
    public DebugBoxSettingEntry[] ProcessArguments { get; set; }
    public DebugBoxClassPathEntry[] ProcessClassPath { get; set; }
    public DebugBoxClassPathEntry[] ProcessNatives { get; set; }
    public DebugBoxMinecraftLibrary[] Libraries { get; set; }
}

public class DebugBoxClassPathEntry(string filename)
{
    public string Filename { get; set; } = filename;
    public bool Exists => File.Exists(Filename);
}

public class DebugBoxMinecraftLibrary(string description, bool present)
{
    public string Description { get; set; } = description;
    public bool Present { get; set; } = present;
}