using System.Diagnostics;
using Spectre.Console;

namespace mcLaunch.Server.Logging;

public static class ManagerLogger
{
    public static void Log(string message, LogSeverity severity = LogSeverity.Info)
    {
        string callerClass = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType?.Name ?? "Unknown";
        
        switch (severity)
        {
            case LogSeverity.Warning:
                AnsiConsole.MarkupLine($"[[[yellow bold]{callerClass}[/]]] WARNING: {message}");
                break;
            case LogSeverity.Error:
                AnsiConsole.MarkupLine($"[[[red bold]{callerClass}[/]]] ERROR: {message}");
                break;
            default:
                AnsiConsole.MarkupLine($"[[[blue bold]{callerClass}[/]]] INFO: {message}");
                break;
        }
    }
}

public enum LogSeverity
{
    Info,
    Warning,
    Error
}