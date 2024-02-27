namespace mcLaunch.Tests;

public static class Logging
{
    public static void Log(LogType type, string message, int offset = 0)
    {
        Console.ResetColor();
        Console.Write($"{string.Concat(Enumerable.Repeat("    ", offset))}[");

        switch (type)
        {
            case LogType.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogType.Failure:
            case LogType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogType.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogType.Success:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case LogType.Pending:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
        }

        Console.Write(type.ToString());
        Console.ResetColor();
        Console.WriteLine($"] {message}");
    }
}

public enum LogType
{
    Info,
    Error,
    Warning,
    Success,
    Pending,
    Failure
}