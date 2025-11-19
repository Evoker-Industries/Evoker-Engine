using System;
using System.Runtime.InteropServices;

namespace EvokerEngine.Core;

/// <summary>
/// Platform detection and utilities for cross-platform support
/// </summary>
public static class Platform
{
    /// <summary>
    /// Check if running on Windows
    /// </summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Check if running on Linux (non-Android)
    /// </summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !IsAndroid;

    /// <summary>
    /// Check if running on macOS (non-iOS)
    /// </summary>
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !IsIOS;

    /// <summary>
    /// Check if running on Android
    /// </summary>
    public static bool IsAndroid
    {
        get
        {
            #if ANDROID
                return true;
            #else
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return false;
                
                // Android reports as Linux, check for Android-specific indicators
                var osDescription = RuntimeInformation.OSDescription.ToLower();
                return osDescription.Contains("android");
            #endif
        }
    }

    /// <summary>
    /// Check if running on iOS
    /// </summary>
    public static bool IsIOS
    {
        get
        {
            #if IOS
                return true;
            #else
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return false;
                
                // iOS would need additional checks if not using the iOS target framework
                var osDescription = RuntimeInformation.OSDescription.ToLower();
                return osDescription.Contains("ios") || osDescription.Contains("iphone") || osDescription.Contains("ipad");
            #endif
        }
    }

    /// <summary>
    /// Check if running on any mobile platform (iOS or Android)
    /// </summary>
    public static bool IsMobile => IsAndroid || IsIOS;

    /// <summary>
    /// Check if running on any desktop platform (Windows, Linux, macOS)
    /// </summary>
    public static bool IsDesktop => IsWindows || IsLinux || IsMacOS;

    /// <summary>
    /// Get the current platform name
    /// </summary>
    public static string PlatformName
    {
        get
        {
            if (IsWindows) return "Windows";
            if (IsLinux) return "Linux";
            if (IsMacOS) return "macOS";
            if (IsAndroid) return "Android";
            if (IsIOS) return "iOS";
            return "Unknown";
        }
    }

    /// <summary>
    /// Get the Vulkan surface extension name for the current platform
    /// </summary>
    public static string GetVulkanSurfaceExtension()
    {
        if (IsWindows) return "VK_KHR_win32_surface";
        if (IsLinux) return "VK_KHR_xcb_surface";
        if (IsMacOS || IsIOS) return "VK_EXT_metal_surface";
        if (IsAndroid) return "VK_KHR_android_surface";
        throw new PlatformNotSupportedException($"Platform {RuntimeInformation.OSDescription} is not supported");
    }
}
