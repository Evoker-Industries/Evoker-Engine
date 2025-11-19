using System;
using System.Diagnostics;

namespace EvokerEngine.Core;

/// <summary>
/// Time management system
/// </summary>
public static class Time
{
    private static Stopwatch _stopwatch = Stopwatch.StartNew();
    private static float _lastFrameTime = 0f;
    private static float _deltaTime = 0f;
    private static float _timeScale = 1f;

    /// <summary>
    /// Time since application start in seconds
    /// </summary>
    public static float TotalTime => (float)_stopwatch.Elapsed.TotalSeconds;

    /// <summary>
    /// Time since last frame in seconds
    /// </summary>
    public static float DeltaTime => _deltaTime * _timeScale;

    /// <summary>
    /// Unscaled time since last frame in seconds
    /// </summary>
    public static float UnscaledDeltaTime => _deltaTime;

    /// <summary>
    /// Time scale (1.0 = normal speed, 0.5 = half speed, 2.0 = double speed)
    /// </summary>
    public static float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Math.Max(0f, value);
    }

    /// <summary>
    /// Frames per second
    /// </summary>
    public static float FPS => _deltaTime > 0f ? 1f / _deltaTime : 0f;

    internal static void Update()
    {
        var currentTime = TotalTime;
        _deltaTime = currentTime - _lastFrameTime;
        _lastFrameTime = currentTime;
    }

    internal static void Reset()
    {
        _stopwatch.Restart();
        _lastFrameTime = 0f;
        _deltaTime = 0f;
    }
}
