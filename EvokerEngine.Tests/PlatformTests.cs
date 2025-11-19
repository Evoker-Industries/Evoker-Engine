using Xunit;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class PlatformTests
{
    [Fact]
    public void Platform_HasValidPlatformName()
    {
        // Act
        var platformName = Platform.PlatformName;

        // Assert
        Assert.NotNull(platformName);
        Assert.NotEmpty(platformName);
        Assert.NotEqual("Unknown", platformName); // Should detect a known platform
    }

    [Fact]
    public void Platform_OnlyOneDesktopPlatformIsTrue()
    {
        // Arrange
        int desktopPlatformCount = 0;
        if (Platform.IsWindows) desktopPlatformCount++;
        if (Platform.IsLinux) desktopPlatformCount++;
        if (Platform.IsMacOS) desktopPlatformCount++;

        // Assert - Should be exactly 1 desktop platform on current system
        Assert.True(desktopPlatformCount <= 1, "Multiple desktop platforms detected");
    }

    [Fact]
    public void Platform_MobileAndDesktopAreMutuallyExclusive()
    {
        // Assert
        if (Platform.IsMobile)
        {
            Assert.False(Platform.IsDesktop, "Platform cannot be both mobile and desktop");
        }
        else if (Platform.IsDesktop)
        {
            Assert.False(Platform.IsMobile, "Platform cannot be both desktop and mobile");
        }
    }

    [Fact]
    public void Platform_AndroidAndIOSAreMutuallyExclusive()
    {
        // Assert
        Assert.False(Platform.IsAndroid && Platform.IsIOS, "Platform cannot be both Android and iOS");
    }

    [Fact]
    public void Platform_GetVulkanSurfaceExtension_ReturnsValidExtension()
    {
        // Act
        var extension = Platform.GetVulkanSurfaceExtension();

        // Assert
        Assert.NotNull(extension);
        Assert.NotEmpty(extension);
        Assert.StartsWith("VK_", extension);
        
        // Verify it's one of the known extensions
        var validExtensions = new[]
        {
            "VK_KHR_win32_surface",
            "VK_KHR_xcb_surface",
            "VK_EXT_metal_surface",
            "VK_KHR_android_surface"
        };
        Assert.Contains(extension, validExtensions);
    }

    [Fact]
    public void Platform_IsMobile_ConsistentWithIndividualFlags()
    {
        // Assert
        Assert.Equal(Platform.IsAndroid || Platform.IsIOS, Platform.IsMobile);
    }

    [Fact]
    public void Platform_IsDesktop_ConsistentWithIndividualFlags()
    {
        // Assert
        Assert.Equal(Platform.IsWindows || Platform.IsLinux || Platform.IsMacOS, Platform.IsDesktop);
    }

    [Fact]
    public void Platform_PlatformName_MatchesDetectedPlatform()
    {
        // Act
        var platformName = Platform.PlatformName;

        // Assert
        if (Platform.IsWindows)
            Assert.Equal("Windows", platformName);
        else if (Platform.IsLinux)
            Assert.Equal("Linux", platformName);
        else if (Platform.IsMacOS)
            Assert.Equal("macOS", platformName);
        else if (Platform.IsAndroid)
            Assert.Equal("Android", platformName);
        else if (Platform.IsIOS)
            Assert.Equal("iOS", platformName);
    }
}
