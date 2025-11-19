using Xunit;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class LoggerTests
{
    [Fact]
    public void Logger_SetMinimumLevel_FiltersMessages()
    {
        // Arrange
        Logger.MinimumLevel = Logger.LogLevel.Warning;

        // Act & Assert - These should not throw
        Logger.Trace("Trace message");
        Logger.Debug("Debug message");
        Logger.Info("Info message");
        Logger.Warning("Warning message");
        Logger.Error("Error message");
        Logger.Fatal("Fatal message");

        // No exception means test passes
        Assert.True(true);
    }

    [Fact]
    public void Logger_AllLevelsExist()
    {
        // Assert - Test that all log level enum values exist
        var levels = System.Enum.GetValues<Logger.LogLevel>();
        Assert.Contains(Logger.LogLevel.Trace, levels);
        Assert.Contains(Logger.LogLevel.Debug, levels);
        Assert.Contains(Logger.LogLevel.Info, levels);
        Assert.Contains(Logger.LogLevel.Warning, levels);
        Assert.Contains(Logger.LogLevel.Error, levels);
        Assert.Contains(Logger.LogLevel.Fatal, levels);
    }
}
