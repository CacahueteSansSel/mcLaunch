using System.Runtime.CompilerServices;

namespace mcLaunch.Core.Logging;

public static class Logs
{
    public static void Debug(string message, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogSeverity.Debug, message, file, lineNumber);
    
    public static void Info(string message, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogSeverity.Info, message, file, lineNumber);
    
    public static void Warning(string message, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogSeverity.Warning, message, file, lineNumber);
    
    public static void Error(string message, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        => Log(LogSeverity.Error, message, file, lineNumber);
    
    public static void Log(LogSeverity severity, string message, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.Write($"[{DateTime.Now:T}] [{Path.GetFileName(file)}:{lineNumber}] [");

        switch (severity)
        {
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.White;
                
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.Cyan;
                
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                
                break;
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                
                break;
        }
        
        Console.Write(severity.ToString().ToUpper());
        Console.ResetColor();
        
        Console.WriteLine($"] {message}");
    }
}

public enum LogSeverity
{
    Debug,
    Info,
    Warning,
    Error
}