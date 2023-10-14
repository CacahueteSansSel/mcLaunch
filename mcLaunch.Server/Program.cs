using System.Text.Json;
using mcLaunch.Server.Core;
using mcLaunch.Utilities;
using Spectre.Console;

AnsiConsole.MarkupLine("[bold]mcLaunch Server[/] - Minecraft Server Management Software");
AnsiConsole.MarkupLine($"Based on [bold]mcLaunch {CurrentBuild.Version}[/]");
AnsiConsole.WriteLine();

MinecraftServer.Settings settings = JsonSerializer.Deserialize<MinecraftServer.Settings>(File.ReadAllText("settings.json"));
MinecraftServer server = new(settings);

// TODO: the entire software
await AnsiConsole.Status()
    .Spinner(Spinner.Known.Line)
    .StartAsync("Launching Minecraft...", server.LaunchAsync);
    
AnsiConsole.MarkupLine($"Minecraft [bold]{server.ServerContext.MinecraftVersion}[/] launched successfully");

await server.RunAsync();