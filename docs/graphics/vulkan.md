---
tags:
  - graphics
  - rendering
  - vulkan
---

# Vulkan Rendering

Evoker Engine uses Vulkan as its primary graphics API for high-performance, cross-platform rendering. The Vulkan integration is handled through Silk.NET.Vulkan.

## Overview

The Vulkan system provides:

- **VulkanContext**: Core Vulkan initialization and device management
- **VulkanSwapchain**: Presentation and frame buffering
- **Cross-platform Support**: Windows (Win32), Linux (XCB), macOS/iOS (Metal), Android
- **Automatic Surface Creation**: Platform-specific surface extensions

## VulkanContext

The VulkanContext manages the core Vulkan instance, physical and logical devices, and queues.

```csharp
public class VulkanContext
{
    public void Initialize(IWindow window);
    public void Cleanup();
    
    // Properties
    public Vk Vk { get; }
    public Instance Instance { get; }
    public PhysicalDevice PhysicalDevice { get; }
    public Device Device { get; }
    public Queue GraphicsQueue { get; }
    public Queue PresentQueue { get; }
}
```

### Initialization

The VulkanContext is automatically initialized by the Application class:

```csharp
// Automatic initialization (done by Application)
var app = new Application("My Game", 1280, 720);
// VulkanContext is created and initialized

// Access from anywhere
var vulkanContext = Application.Instance.VulkanContext;
```

### Platform Detection

The engine automatically detects the platform and creates the appropriate Vulkan surface:

```csharp
// Automatic platform-specific surface creation
// Windows: VK_KHR_win32_surface
// Linux: VK_KHR_xcb_surface
// macOS/iOS: VK_EXT_metal_surface
// Android: VK_KHR_android_surface

if (Platform.IsWindows)
{
    // Uses Win32 surface
}
else if (Platform.IsLinux)
{
    // Uses XCB surface
}
else if (Platform.IsMacOS || Platform.IsIOS)
{
    // Uses Metal surface
}
else if (Platform.IsAndroid)
{
    // Uses Android surface
}
```

## VulkanSwapchain

Manages the swapchain for presenting rendered frames to the screen.

```csharp
public class VulkanSwapchain
{
    public void Create(uint width, uint height);
    public void Recreate(uint width, uint height);
    public void Cleanup();
    
    public SwapchainKHR Swapchain { get; }
    public Format ImageFormat { get; }
    public Extent2D Extent { get; }
}
```

### Swapchain Management

```csharp
// Created automatically by Application
// Recreated on window resize
var swapchain = Application.Instance.VulkanContext.Swapchain;

// Manual recreation (if needed)
swapchain.Recreate(1920, 1080);
```

## Rendering Pipeline (Planned)

The rendering pipeline is being developed with the following components:

### Shaders

```csharp
// Load GLSL shaders (planned)
var vertexShader = LoadShader("shaders/vertex.glsl", ShaderStage.Vertex);
var fragmentShader = LoadShader("shaders/fragment.glsl", ShaderStage.Fragment);

// Compile to SPIR-V (planned)
var vertexSPIRV = CompileShader(vertexShader);
var fragmentSPIRV = CompileShader(fragmentShader);
```

### Pipeline Creation (Planned)

```csharp
// Create graphics pipeline (planned)
var pipelineInfo = new GraphicsPipelineCreateInfo
{
    VertexShader = vertexSPIRV,
    FragmentShader = fragmentSPIRV,
    VertexInputState = vertexInputState,
    InputAssemblyState = inputAssemblyState,
    RasterizationState = rasterizationState,
    ColorBlendState = colorBlendState,
    DepthStencilState = depthStencilState
};

var pipeline = CreateGraphicsPipeline(pipelineInfo);
```

### Command Buffers (Planned)

```csharp
// Record rendering commands (planned)
var commandBuffer = BeginCommandBuffer();

commandBuffer.BeginRenderPass(renderPass);
commandBuffer.BindPipeline(pipeline);
commandBuffer.BindVertexBuffer(vertexBuffer);
commandBuffer.BindIndexBuffer(indexBuffer);
commandBuffer.DrawIndexed(indexCount);
commandBuffer.EndRenderPass();

EndCommandBuffer(commandBuffer);
```

## Current State

The Vulkan integration currently provides:

✅ **Implemented:**
- Vulkan instance creation
- Physical device selection
- Logical device and queue creation
- Surface creation (cross-platform)
- Swapchain management
- Automatic window resize handling

🚧 **In Progress:**
- Graphics pipeline creation
- Shader loading and compilation
- Vertex and index buffer management
- Texture loading and sampling
- Render pass configuration

📋 **Planned:**
- Advanced rendering features
- Compute shader support
- Ray tracing support (on capable hardware)
- Multi-threaded rendering

## Example: Custom Rendering

While the full rendering pipeline is being developed, here's how you would extend it:

```csharp
public class CustomRenderer : Layer
{
    private VulkanContext _vulkanContext;
    
    public override void OnAttach()
    {
        _vulkanContext = Application.Instance.VulkanContext;
        
        // Setup your custom rendering resources
        CreateRenderPass();
        CreatePipeline();
        CreateBuffers();
    }

    public override void OnRender()
    {
        // Custom rendering code
        // (Full implementation pending)
    }

    private void CreateRenderPass()
    {
        // Create custom render pass
    }

    private void CreatePipeline()
    {
        // Create custom graphics pipeline
    }

    private void CreateBuffers()
    {
        // Create vertex and index buffers
    }
}
```

## Best Practices

### ✅ Do's

- Let the engine handle Vulkan initialization
- Use the provided VulkanContext
- Handle window resize events for swapchain recreation
- Clean up Vulkan resources properly

### ❌ Don'ts

- Don't create your own Vulkan instance
- Don't bypass the engine's swapchain management
- Don't forget to handle minimized windows
- Don't perform rendering operations on minimized windows

## Platform Support

The engine provides first-class Vulkan support on:

| Platform | Surface Extension | Status |
|----------|------------------|--------|
| Windows | `VK_KHR_win32_surface` | ✅ Supported |
| Linux | `VK_KHR_xcb_surface` | ✅ Supported |
| macOS | `VK_EXT_metal_surface` | ✅ Supported |
| iOS | `VK_EXT_metal_surface` | ✅ Supported |
| Android | `VK_KHR_android_surface` | ✅ Supported |

## Requirements

- **Vulkan 1.2** or later
- **Vulkan SDK** (for development)
- **Compatible GPU** with Vulkan support

## See Also

- [Rendering Pipeline](rendering-pipeline.md) - Complete rendering pipeline
- [Materials](materials.md) - Material system
- [Application](../core/application.md) - Application initialization
