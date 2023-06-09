﻿using System.Diagnostics;
using Cacahuete.MinecraftLib.Auth;
using Cacahuete.MinecraftLib.Models;

namespace Cacahuete.MinecraftLib.Core;

public class Minecraft
{
    bool disableMultiplayer;
    bool disableChat;
    bool demo;
    string? serverAddress;
    int serverPort;
    string launcherName;
    string launcherVersion;
    string classPath;
    string nativesPath;
    string classPathSeparator = ";";
    string username;
    string assetsRoot;
    string assetIndexName;
    string uuid;
    string accessToken;
    string clientId;
    string xuid;
    string userType;
    string versionType;
    string? jvmPath;
    Process mc;
    MinecraftFolder sysFolder;

    public MinecraftVersion Version { get; }
    public MinecraftFolder Folder { get; }

    public Minecraft(MinecraftVersion version, MinecraftFolder folder)
    {
        Version = version;
        Folder = folder;
        sysFolder = folder;
        versionType = version.Type;
    }

    public Minecraft WithSystemFolder(MinecraftFolder systemFolder)
    {
        sysFolder = systemFolder;

        return this;
    }

    public Minecraft WithUser(string username, Guid uuid)
    {
        this.username = username;
        this.uuid = uuid.ToString().Replace("-", "");

        return this;
    }

    public Minecraft WithUser(MinecraftAuthenticationResult auth, AuthenticationPlatform platform)
    {
        username = auth.Username;
        uuid = auth.Uuid;
        xuid = auth.Xuid;
        clientId = platform.ClientId;
        accessToken = auth.AccessToken;
        userType = platform.UserType;

        return this;
    }

    public Minecraft WithMultiplayer(bool multiplayer)
    {
        disableMultiplayer = !multiplayer;

        return this;
    }

    public Minecraft WithChat(bool chat)
    {
        disableChat = !chat;

        return this;
    }

    public Minecraft WithServer(string address, string port)
    {
        serverAddress = address;
        serverPort = int.Parse(port);

        return this;
    }

    public Minecraft WithCustomLauncherDetails(string launcherName, string launcherVersion, bool expose = false)
    {
        this.launcherName = launcherName;
        this.launcherVersion = launcherVersion;
        
        if (expose) versionType = launcherName;

        return this;
    }
    
    public Minecraft WithDownloaders(AssetsDownloader assets, LibrariesDownloader libraries, JVMDownloader jvm)
    {
        assetsRoot = Path.GetFullPath(assets.Path);
        classPath = string.Join(classPathSeparator, libraries.ClassPath) + classPathSeparator;
        nativesPath = Path.GetFullPath(libraries.NativesPath);
        jvmPath = jvm.GetJVMPath(Utilities.GetJavaPlatformIdentifier(), Version.JavaVersion?.Component ?? "jre-legacy")
            .TrimEnd('/');

        if (OperatingSystem.IsWindows()) jvmPath += "/bin/javaw.exe";
        else if (OperatingSystem.IsMacOS()) jvmPath += "/jre.bundle/Contents/Home/bin/java";
        else jvmPath += "/bin/java";

        return this;
    }
    
    public Process Run()
    {
        string jarPath = $"{sysFolder.Path}/versions/{Version.Id}/{Version.Id}.jar";
        string jvm = jvmPath ?? sysFolder.GetJVM(Version.JavaVersion.Component);
        
        if (Version.Arguments == null)
        {
            Version.Arguments = MinecraftVersion.ModelArguments.Default;
        }

        if (Version.Arguments.JVM == null)
        {
            Version.Arguments.JVM = MinecraftVersion.ModelArguments.Default.JVM;
        }
        
        string args = Version.Arguments.Build(new Dictionary<string, string>()
        {
            {"auth_player_name", username},
            {"auth_session", $"token:{accessToken}:{uuid}"},
            {"version_name", Version.Id},
            {"game_assets", $"{sysFolder.CompletePath}/assets/virtual"},
            {"game_directory", Folder.CompletePath},
            {"assets_root", assetsRoot},
            {"assets_index_name", Version.AssetIndex.Id},
            {"auth_uuid", uuid},
            {"auth_access_token", accessToken},
            {"clientid", clientId},
            {"auth_xuid", xuid},
            {"user_type", userType},
            {"version_type", versionType},
            {"launcher_name", launcherName},
            {"launcher_version", launcherVersion},
            {"classpath", classPath + Path.GetFullPath(jarPath)},
            {"classpath_separator", classPathSeparator},
            {"natives_directory", nativesPath},
            {"library_directory", $"{sysFolder.CompletePath}/libraries"},
            {"user_properties", "{}"}
        }, Version.MainClass);

        if (disableMultiplayer) args += " --disableMultiplayer";
        if (disableChat) args += " --disableChat";
        if (serverAddress != null)
        {
            args += " --server " + serverAddress;
            args += " --port " + serverPort;
        }

        ProcessStartInfo info = new()
        {
            Arguments = args,
            FileName = jvm,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Folder.CompletePath
        };
        
        return Process.Start(info);
    }
}