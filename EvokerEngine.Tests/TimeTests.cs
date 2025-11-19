using Xunit;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class TimeTests
{
    [Fact]
    public void Time_Reset_SetsTimeToZero()
    {
        // Act
        Time.Reset();

        // Assert
        Assert.True(Time.TotalTime >= 0);
        Assert.Equal(0f, Time.DeltaTime);
    }

    [Fact]
    public void Time_Update_IncreasesDeltaTime()
    {
        // Arrange
        Time.Reset();
        System.Threading.Thread.Sleep(10); // Sleep for 10ms

        // Act
        Time.Update();

        // Assert
        Assert.True(Time.DeltaTime > 0);
        Assert.True(Time.TotalTime > 0);
    }

    [Fact]
    public void Time_TimeScale_AffectsDeltaTime()
    {
        // Arrange
        Time.Reset();
        Time.TimeScale = 0.5f;
        System.Threading.Thread.Sleep(10);
        Time.Update();
        var scaledDelta = Time.DeltaTime;
        var unscaledDelta = Time.UnscaledDeltaTime;

        // Assert
        Assert.True(scaledDelta < unscaledDelta);
        Assert.Equal(0.5f, Time.TimeScale);
    }

    [Fact]
    public void Time_TimeScale_CannotBeNegative()
    {
        // Act
        Time.TimeScale = -1.0f;

        // Assert
        Assert.True(Time.TimeScale >= 0);
    }

    [Fact]
    public void Time_FPS_IsCalculatedCorrectly()
    {
        // Arrange
        Time.Reset();
        System.Threading.Thread.Sleep(10);
        Time.Update();

        // Assert
        Assert.True(Time.FPS > 0);
    }
}
