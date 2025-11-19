# Installation

This guide will help you set up Evoker Engine for development.

## Prerequisites

Before you begin, ensure you have the following installed:

### Required

- **.NET 9.0 SDK or later** - [Download here](https://dotnet.microsoft.com/download)
- **Vulkan SDK** (runtime only) - [Download here](https://vulkan.lunarg.com/)

### Optional (for Mobile Development)

If you want to build for iOS or Android:

```bash
dotnet workload install ios android
```

## Installation Methods

### Method 1: Clone the Repository

Clone the Evoker Engine repository to get started:

```bash
git clone https://github.com/evokerking1/Evoker-Engine.git
cd Evoker-Engine
```

### Method 2: Use as a Library

Add Evoker Engine as a reference to your project:

```bash
# Create a new project
dotnet new console -n MyGame

# Add reference to EvokerEngine.Core
dotnet add reference path/to/EvokerEngine.Core/EvokerEngine.Core.csproj
```

## Building the Engine

### Desktop Platforms (Windows, Linux, macOS)

```bash
# Build the entire solution
dotnet build

# Build in Release mode
dotnet build -c Release
```

### Mobile Platforms (iOS, Android)

To build for mobile platforms (requires workloads installed):

```bash
# Build for mobile
dotnet build -p:BuildForMobile=true

# Or build specific platforms
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
```

## Running Tests

Evoker Engine includes 149 comprehensive unit tests:

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

An HTML test report is automatically generated at: `EvokerEngine.Tests/TestResults/TestResults.html`

## Running the Demo

Try the included demo application:

```bash
cd EvokerEngine.Demo
dotnet run
```

### Demo Controls

**Keyboard:**
- ESC - Exit application
- F1 - Toggle VSync (logged only)
- WASD - Camera movement (logged only)

**Gamepad:**
- A/Cross - Action button
- Start - Pause
- Left Stick - Movement
- Right Stick - Camera

## Verify Installation

Create a simple test application to verify everything is working:

```csharp title="Program.cs"
using EvokerEngine.Core;

var app = new Application("Test App", 800, 600);
app.Run();
```

Run it:

```bash
dotnet run
```

If you see a window open successfully, your installation is complete! 🎉

## Troubleshooting

### Vulkan SDK Not Found

**Problem:** Build fails with Vulkan-related errors.

**Solution:** 
1. Download and install the [Vulkan SDK](https://vulkan.lunarg.com/)
2. Restart your terminal/IDE
3. Verify installation: `vulkaninfo` or `vkcube`

### .NET SDK Version Issues

**Problem:** Build fails with .NET version errors.

**Solution:**
1. Check your .NET version: `dotnet --version`
2. Install .NET 9.0 SDK or later
3. Update global.json if present

### Mobile Workload Issues

**Problem:** Mobile builds fail.

**Solution:**
```bash
# Update workloads
dotnet workload update

# Repair workloads
dotnet workload repair

# Reinstall workloads
dotnet workload install ios android
```

## Next Steps

Now that you have Evoker Engine installed:

- 📖 Continue to [Quick Start](quick-start.md) to create your first application
- 🎮 Learn about [Game Development](../game-dev/overview.md)
- 🔧 Explore [Modding](../modding/creating-mods.md)

## Platform-Specific Notes

=== "Windows"
    - Requires Visual C++ Redistributable
    - Vulkan SDK includes all necessary drivers
    - Use Visual Studio or Visual Studio Code for development

=== "Linux"
    - Install Vulkan drivers for your GPU:
        - AMD: `sudo apt install mesa-vulkan-drivers`
        - NVIDIA: Install proprietary drivers
        - Intel: `sudo apt install mesa-vulkan-drivers intel-media-va-driver`
    - May need to install additional dependencies:
        ```bash
        sudo apt install libx11-dev libxcb1-dev libxcb-randr0-dev
        ```

=== "macOS"
    - Vulkan support via MoltenVK (included in Vulkan SDK)
    - Requires macOS 10.15 (Catalina) or later
    - Use Visual Studio Code or Rider for development

=== "iOS"
    - Requires macOS with Xcode installed
    - Need Apple Developer account for device deployment
    - Simulator testing available without account

=== "Android"
    - Requires Android SDK
    - Install via Visual Studio or Android Studio
    - Test on emulator or physical device with Vulkan support
