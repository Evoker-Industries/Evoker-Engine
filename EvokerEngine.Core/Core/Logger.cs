using System;

namespace EvokerEngine.Core;

/// <summary>
/// Logging system for the engine
/// </summary>
public static class Logger
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

    public static void Trace(string message) => Log(LogLevel.Trace, message);
    public static void Debug(string message) => Log(LogLevel.Debug, message);
    public static void Info(string message) => Log(LogLevel.Info, message);
    public static void Warning(string message) => Log(LogLevel.Warning, message);
    public static void Error(string message) => Log(LogLevel.Error, message);
    public static void Fatal(string message) => Log(LogLevel.Fatal, message);

    private static void Log(LogLevel level, string message)
    {
        if (level < MinimumLevel) return;

        var color = level switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.White,
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => ConsoleColor.White
        };

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{timestamp}] [{level}] {message}");
        Console.ForegroundColor = oldColor;
    }
}
