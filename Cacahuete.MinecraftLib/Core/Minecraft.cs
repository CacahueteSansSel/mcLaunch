using System.Diagnostics;
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
    }

    public Minecraft WithSystemFolder(MinecraftFolder systemFolder)
    {
        sysFolder = systemFolder;

        return this;
    }

    public Minecraft WithJVM(string jvmPath)
    {
        this.jvmPath = jvmPath;

        return this;
    }

    public Minecraft WithJVMBaseFolder(string jvmPath)
    {
        string platform = $"{Utilities.GetPlatformIdentifier()}-{Utilities.GetArchitecture()}";

        this.jvmPath = $"{Path.GetFullPath(jvmPath)}/{Version.JavaVersion.Component}/{platform}/{Version.JavaVersion.Component}/bin/javaw" +
                       (platform.StartsWith("windows") ? ".exe" : "");

        return this;
    }

    public Minecraft WithUser(string username, Guid uuid)
    {
        this.username = username;
        this.uuid = uuid.ToString().Replace("-", "");

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

    public Minecraft WithCustomLauncherDetails(string launcherName, string launcherVersion)
    {
        this.launcherName = launcherName;
        this.launcherVersion = launcherVersion;

        return this;
    }
    
    public Minecraft WithDownloaders(AssetsDownloader assets, LibrariesDownloader libraries)
    {
        assetsRoot = Path.GetFullPath(assets.Path);
        classPath = string.Join(classPathSeparator, libraries.ClassPath) + classPathSeparator;
        nativesPath = Path.GetFullPath(libraries.NativesPath);

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
        
        string args = Version.Arguments.Build(new Dictionary<string, string>()
        {
            {"auth_player_name", username},
            {"version_name", Version.Id},
            {"game_directory", Folder.CompletePath},
            {"assets_root", assetsRoot},
            {"assets_index_name", Version.AssetIndex.Id},
            {"auth_uuid", uuid},
            {"auth_access_token", accessToken},
            {"clientid", clientId},
            {"auth_xuid", xuid},
            {"user_type", userType},
            {"version_type", Version.Type},
            {"launcher_name", launcherName},
            {"launcher_version", launcherVersion},
            {"classpath", classPath + Path.GetFullPath(jarPath)},
            {"natives_directory", nativesPath},
        }, Version.MainClass);
        
        Console.WriteLine(args);

        if (disableMultiplayer) args += "--disableMultiplayer";
        if (disableChat) args += "--disableChat";
        if (serverAddress != null)
        {
            args += "--server " + serverAddress;
            args += "--port " + serverPort;
        }
        
        Console.WriteLine(args);

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