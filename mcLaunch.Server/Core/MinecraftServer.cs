using System.Diagnostics;
using System.Text.RegularExpressions;
using Cacahuete.MinecraftLib.Models;
using Spectre.Console;

namespace mcLaunch.Server.Core;

public class MinecraftServer
{
    public static MinecraftServer Instance { get; private set; }
    public Settings ServerSettings { get; private set; }
    public Context ServerContext { get; private set; }
    public Process? JavaProcess { get; private set; }
    
    Regex minecraftVersionRegex = RegularExpressions.MinecraftVersion();
    Regex versionRegex = RegularExpressions.SemanticVersionRegex();
    Regex modloaderRegex = RegularExpressions.ModLoaderRegex();
    Regex playerConnectRegex = RegularExpressions.PlayerConnectRegex();
    Regex playerDisconnectRegex = RegularExpressions.PlayerDisconnectRegex();
    
    public MinecraftServer(Settings serverSettings)
    {
        Instance = this;
        
        ServerSettings = serverSettings;
    }

    async Task ProcessStandardOutLine(string outputLine)
    {
        if (minecraftVersionRegex.IsMatch(outputLine) && ServerContext.MinecraftVersion == null)
        {
            ServerContext.MinecraftVersion = versionRegex.Match(minecraftVersionRegex.Match(outputLine).Value.Trim()).Value.Trim();
            ServerContext.ModLoader = outputLine.Contains("Forge Mod Loader")
                ? "Forge"
                : modloaderRegex.Match(outputLine).Value.Replace("with ", "").Trim();
                
            return;
        }
            
        if (playerConnectRegex.IsMatch(outputLine))
        {
            string username = playerConnectRegex.Match(outputLine).Value.Replace(" joined the game", "").Trim();

            // TODO: add username to a list
            
            return;
        }
            
        if (playerDisconnectRegex.IsMatch(outputLine))
        {
            string username = playerDisconnectRegex.Match(outputLine).Value.Replace(" left the game", "").Trim();
            string uuid = ServerContext.Uuids.GetUuid(username);
                
            // TODO: remove username from list
            
            return;
        }
            
        if (ServerContext.Uuids.IsUuidLine(outputLine))
        {
            (string username, string uuid)? v = ServerContext.Uuids.Process(outputLine);

            if (v.HasValue)
            {
                // TODO: process uuid
            }
                
            return;
        }
            
        if (ServerContext.Chat.IsChatMessage(outputLine))
        {
            ChatMessage chatMessage = ServerContext.Chat.Parse(outputLine)!
                .RemoveForbiddenCharacters();

                
                
            return;
        }
    }
 
    public async Task LaunchAsync(StatusContext ctx)
    {
        JavaProcess = Process.Start(new ProcessStartInfo
        {
            FileName = ServerSettings.JavaExecutablePath ?? "java",
            Arguments =
                $"-Xmx{ServerSettings.AllocatedRamSize}G -jar -Dfml.queryResult=confirm \"{ServerSettings.MinecraftJarPath}\" nogui",
            WorkingDirectory = ServerSettings.MinecraftFolder,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false
        });

        ServerContext = new Context
        {
            Uuids = new UuidStorage(),
            Chat = new ChatParser()
        };

        while (!JavaProcess.HasExited)
        {
            string? outputLine = await JavaProcess.StandardOutput.ReadLineAsync();
            if (outputLine == null) break;

            AnsiConsole.MarkupLineInterpolated($"[gray italic]{outputLine}[/]");

            await ProcessStandardOutLine(outputLine);
            
            if (outputLine.Contains("Done") && outputLine.Contains("For help, type \"help\""))
            {
                // The server started successfully

                ctx.Status("Done !");
                return;
            }
        }
    }

    public async Task RunAsync()
    {
        while (!JavaProcess.HasExited)
        {
            string? outputLine = await JavaProcess.StandardOutput.ReadLineAsync();
            if (outputLine == null) break;

            AnsiConsole.MarkupLineInterpolated($"[gray italic]{outputLine}[/]");
            
            await ProcessStandardOutLine(outputLine);
        }
    }

    public class Settings
    {
        public string? JavaExecutablePath { get; set; }
        public string MinecraftJarPath { get; set; }
        public string MinecraftFolder { get; set; }
        public int AllocatedRamSize { get; set; }
    }

    public class Context
    {
        public string MinecraftVersion { get; set; }
        public string ModLoader { get; set; }
        public string PublicIPAddress { get; set; }
        public UuidStorage Uuids { get; set; }
        public ChatParser Chat { get; set; }
    }
}