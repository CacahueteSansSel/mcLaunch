using System.Diagnostics;
using mcLaunch.Launchsite.Auth;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core;

public class Minecraft
{
    private readonly Dictionary<string, string> args = new();
    private bool disableChat;
    private bool disableMultiplayer;
    private string? jvmPath;
    private QuickPlayWorldType? quickPlayMode;
    private string? quickPlayPath;
    private string? quickPlaySingleplayerWorldName;
    private bool redirectOutput;
    private string? serverAddress;
    private uint serverPort;
    private MinecraftFolder sysFolder;
    private bool useDedicatedGraphics;
    private CommandLineSettings cmdLineSettings = CommandLineSettings.Default;

    public Minecraft(MinecraftVersion version, MinecraftFolder folder)
    {
        Version = version;
        Folder = folder;

        args["version_name"] = version.Id;
        args["version_type"] = version.Type;
        args["game_directory"] = folder.CompletePath;
    }

    public MinecraftVersion Version { get; }
    public MinecraftFolder Folder { get; }
    public List<string> StandardOutput { get; } = [];
    public string? JvmPath => jvmPath;
    public string LibrariesFolderPath => args["library_directory"];
    public string NativesFolderPath => args["natives_directory"];
    public event Action<string> OnStandardOutputLineReceived;

    public Minecraft WithSystemFolder(MinecraftFolder systemFolder)
    {
        sysFolder = systemFolder;

        return this;
    }

    public Minecraft WithUser(string username, Guid uuid)
    {
        args["auth_player_name"] = username;
        args["auth_uuid"] = uuid.ToString("N");

        return this;
    }

    public Minecraft WithUseDedicatedGraphics(bool useDedicatedGraphics)
    {
        this.useDedicatedGraphics = useDedicatedGraphics;

        return this;
    }

    public Minecraft WithRedirectOutput(bool redirectOutput)
    {
        this.redirectOutput = redirectOutput;

        return this;
    }
    
    public Minecraft WithCommandLineSettings(CommandLineSettings settings)
    {
        cmdLineSettings = settings;

        return this;
    }

    public Minecraft WithUser(MinecraftAuthenticationResult auth, AuthenticationPlatform platform)
    {
        args["auth_player_name"] = auth.Username;
        args["auth_uuid"] = auth.Uuid;
        args["auth_xuid"] = auth.Xuid;
        args["clientid"] = platform.ClientId;
        args["user_type"] = platform.UserType;
        args["auth_session"] = $"token:{auth.AccessToken}:{auth.Uuid}";
        args["auth_access_token"] = auth.AccessToken;

        return this;
    }

    public Minecraft WithMultiplayer(bool multiplayer)
    {
        disableMultiplayer = !multiplayer;

        return this;
    }

    public Minecraft WithSingleplayerQuickPlay(string profilePath, string worldName)
    {
        quickPlayMode = QuickPlayWorldType.Singleplayer;
        quickPlayPath = profilePath;
        quickPlaySingleplayerWorldName = worldName;

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
        serverPort = uint.Parse(port);

        return this;
    }

    public Minecraft WithCustomLauncherDetails(string launcherName, string launcherVersion, bool expose = false)
    {
        args["launcher_name"] = launcherName;
        args["launcher_version"] = launcherVersion;

        if (expose) args["version_type"] = launcherName;

        return this;
    }

    public Minecraft WithDownloaders(AssetsDownloader assets, LibrariesDownloader libraries, JvmDownloader jvm)
    {
        string classPathSeparator = OperatingSystem.IsWindows() ? ";" : ":";
        string jarPath = $"{sysFolder.Path}/versions/{Version.Id}/{Version.Id}.jar";

        string assetsRoot = Path.GetFullPath(assets.VirtualPath ?? assets.Path);
        string classPath = string.Join(classPathSeparator, libraries.ClassPath) + classPathSeparator;
        string nativesPath = Path.GetFullPath(libraries.NativesPath);

        jvmPath = jvm.GetJvmPath(Utilities.GetJavaPlatformIdentifier(), Version.JavaVersion?.Component ?? "jre-legacy")
            .TrimEnd('/');

        if (OperatingSystem.IsWindows()) jvmPath += "/bin/javaw.exe";
        else if (OperatingSystem.IsMacOS()) jvmPath += "/jre.bundle/Contents/Home/bin/java";
        else jvmPath += "/bin/java";

        args["assets_root"] = assetsRoot;
        args["game_assets"] = assetsRoot; // Fix for older versions
        args["assets_index_name"] = Version.AssetIndex.Id;
        args["classpath"] = classPath + Path.GetFullPath(jarPath);
        args["classpath_separator"] = classPathSeparator;
        args["natives_directory"] = nativesPath;
        args["library_directory"] = $"{sysFolder.CompletePath}/libraries";

        return this;
    }

    private void ReadOutput(Process process)
    {
        while (!process.HasExited)
        {
            string? line = process.StandardOutput.ReadLine();
            if (line == null) break;

            StandardOutput.Add(line);
            OnStandardOutputLineReceived?.Invoke(line);
        }

        StandardOutput.Add($"Minecraft exited with code {process.ExitCode}");
    }

    string BuildArgs()
    {
        string builtArgs = Version.Arguments.Build(args, Version.MainClass);

        if (disableMultiplayer) builtArgs += " --disableMultiplayer";
        if (disableChat) builtArgs += " --disableChat";
        if (serverAddress != null)
        {
            builtArgs += " --server " + serverAddress;
            builtArgs += " --port " + serverPort;
        }

        if (quickPlayMode.HasValue)
        {
            if (quickPlayPath != null)
                builtArgs += $" --quickPlayPath {quickPlayPath}";

            switch (quickPlayMode)
            {
                case QuickPlayWorldType.Singleplayer:
                    builtArgs += " --quickPlaySingleplayer";

                    if (quickPlaySingleplayerWorldName != null)
                        builtArgs += $" \"{quickPlaySingleplayerWorldName}\"";

                    break;
                case QuickPlayWorldType.Multiplayer:
                    // TODO: QuickPlay multiplayer & realms support
                    builtArgs += " --quickPlayMultiplayer";
                    break;
                case QuickPlayWorldType.Realms:
                    // TODO: QuickPlay multiplayer & realms support
                    builtArgs += " --quickPlayRealms";
                    break;
            }
        }

        builtArgs = cmdLineSettings.BuildArguments(builtArgs);

        return builtArgs;
    }

    public Process Run()
    {
        string jvm = jvmPath ?? sysFolder.GetJvm(Version.JavaVersion!.Component);

        if (Version.Arguments == null) Version.Arguments = MinecraftVersion.ModelArguments.Default;

        if (Version.Arguments.Jvm == null) Version.Arguments.Jvm = MinecraftVersion.ModelArguments.Default.Jvm;

        string builtArgs = BuildArgs();
        bool addClassPathToEnv = false;

        int commandLineLength = builtArgs.Length + jvm.Length + 1;
        if (commandLineLength > 32768)
        {
            string cp = args["classpath"];
            args["classpath"] = "--";
            
            // Rebuild the arguments line with the new classpath
            builtArgs = BuildArgs().Replace("-cp -- ", "");
            
            args["classpath"] = cp;
            addClassPathToEnv = true;
        }

        ProcessStartInfo info = new()
        {
            Arguments = builtArgs,
            FileName = jvm,
            UseShellExecute = false,
            WorkingDirectory = Folder.CompletePath,
            RedirectStandardError = true,
            RedirectStandardOutput = redirectOutput
        };
        if (addClassPathToEnv) info.EnvironmentVariables.Add("CLASSPATH", args["classpath"]);

        if (useDedicatedGraphics)
        {
            if (OperatingSystem.IsLinux() && File.Exists("/usr/bin/prime-run"))
            {
                info.Arguments = $"{info.FileName} {info.Arguments}";
                info.FileName = "/usr/bin/prime-run";
            }
        }

        // An attempt to fix the "java opens in TextEdit" bug
        if (OperatingSystem.IsMacOS()) File.SetUnixFileMode(info.FileName, UnixFileMode.UserExecute);

        Process java = Process.Start(info)!;
        if (redirectOutput) new Thread(() => ReadOutput(java)).Start();

        return java;
    }
}