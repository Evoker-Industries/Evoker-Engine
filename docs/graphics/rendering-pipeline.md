# Rendering Pipeline

The rendering pipeline in Evoker Engine is built on top of Vulkan and provides a modern, high-performance rendering system for both 2D and 3D graphics.

## Overview

The rendering pipeline includes:

- **Render Passes**: Define rendering stages
- **Graphics Pipelines**: Shader programs and rendering state
- **Command Buffers**: Recording draw commands
- **Frame Synchronization**: Managing frame presentation
- **Resource Management**: Buffers, textures, and samplers

## Current Implementation Status

### ✅ Implemented

- Vulkan context and device initialization
- Swapchain creation and management
- Window resize handling
- Cross-platform surface creation

### 🚧 In Development

- Graphics pipeline creation
- Render pass configuration
- Command buffer recording
- Shader compilation (GLSL to SPIR-V)
- Vertex and index buffer management
- Texture loading and binding

### 📋 Planned

- Deferred rendering
- Shadow mapping
- Post-processing effects
- Particle systems rendering
- Skeletal animation
- Level of Detail (LOD) system

## Rendering Architecture

```
┌─────────────────────────────────────┐
│         Application Layer           │
├─────────────────────────────────────┤
│         Scene Management            │
│    (Entities, Components, Camera)   │
├─────────────────────────────────────┤
│       Rendering Interface           │
│    (Draw calls, material binding)   │
├─────────────────────────────────────┤
│      Graphics Pipeline              │
│  (Shaders, buffers, render passes)  │
├─────────────────────────────────────┤
│      Vulkan Abstraction             │
│  (Device, queues, command buffers)  │
├─────────────────────────────────────┤
│         Vulkan API                  │
└─────────────────────────────────────┘
```

## Planned Pipeline Stages

### 1. Geometry Stage

```csharp
// Load and manage meshes (planned)
var mesh = MeshLoader.LoadFromFile("models/character.obj");
var vertexBuffer = CreateVertexBuffer(mesh.Vertices);
var indexBuffer = CreateIndexBuffer(mesh.Indices);
```

### 2. Vertex Stage

```glsl
// Vertex shader example
#version 450

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inNormal;
layout(location = 2) in vec2 inTexCoord;

layout(location = 0) out vec3 fragPosition;
layout(location = 1) out vec3 fragNormal;
layout(location = 2) out vec2 fragTexCoord;

layout(set = 0, binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 projection;
} ubo;

void main() {
    gl_Position = ubo.projection * ubo.view * ubo.model * vec4(inPosition, 1.0);
    fragPosition = vec3(ubo.model * vec4(inPosition, 1.0));
    fragNormal = mat3(transpose(inverse(ubo.model))) * inNormal;
    fragTexCoord = inTexCoord;
}
```

### 3. Fragment Stage

```glsl
// Fragment shader example
#version 450

layout(location = 0) in vec3 fragPosition;
layout(location = 1) in vec3 fragNormal;
layout(location = 2) in vec2 fragTexCoord;

layout(location = 0) out vec4 outColor;

layout(set = 1, binding = 0) uniform sampler2D texSampler;

void main() {
    vec3 lightDir = normalize(vec3(1.0, 1.0, 1.0));
    float diff = max(dot(fragNormal, lightDir), 0.0);
    
    vec3 texColor = texture(texSampler, fragTexCoord).rgb;
    vec3 result = diff * texColor;
    
    outColor = vec4(result, 1.0);
}
```

## Forward Rendering (Current)

The engine currently implements forward rendering:

```
For each object:
    1. Set pipeline state
    2. Bind resources (textures, uniforms)
    3. Draw geometry
```

**Advantages:**
- Simple implementation
- Good for simpler scenes
- Transparent objects rendered easily

**Disadvantages:**
- Performance degrades with many lights
- Lighting calculated multiple times per pixel

## Deferred Rendering (Planned)

Planned deferred rendering pipeline:

```
Pass 1 - G-Buffer:
    Render all geometry to multiple render targets:
    - Position
    - Normal
    - Albedo/Color
    - Material properties

Pass 2 - Lighting:
    For each light:
        Calculate lighting using G-Buffer data
        
Pass 3 - Forward Pass:
    Render transparent objects
    Apply post-processing
```

**Advantages:**
- Efficient with many lights
- Decouples geometry from lighting
- Enables advanced effects

## Render Pass Example (Planned)

```csharp
public class GameRenderer
{
    private RenderPass _mainRenderPass;
    private Pipeline _geometryPipeline;
    private Pipeline _lightingPipeline;

    public void Initialize()
    {
        // Create render passes
        _mainRenderPass = CreateMainRenderPass();
        
        // Create pipelines
        _geometryPipeline = CreateGeometryPipeline();
        _lightingPipeline = CreateLightingPipeline();
    }

    public void Render(Scene scene, Camera camera)
    {
        var commandBuffer = BeginFrame();
        
        // Begin render pass
        commandBuffer.BeginRenderPass(_mainRenderPass);
        
        // Render opaque geometry
        RenderOpaqueGeometry(commandBuffer, scene, camera);
        
        // Render transparent geometry
        RenderTransparentGeometry(commandBuffer, scene, camera);
        
        // End render pass
        commandBuffer.EndRenderPass();
        
        // Present frame
        EndFrame(commandBuffer);
    }

    private void RenderOpaqueGeometry(CommandBuffer cmd, Scene scene, Camera camera)
    {
        cmd.BindPipeline(_geometryPipeline);
        
        // Update camera uniform
        UpdateCameraUniform(camera);
        
        // Render all opaque meshes
        var renderers = scene.Registry.GetAllComponents<MeshRendererComponent>();
        foreach (var renderer in renderers)
        {
            var transform = scene.Registry.GetComponent<TransformComponent>(renderer.Entity);
            if (transform != null)
            {
                // Update model matrix
                UpdateModelUniform(transform);
                
                // Draw mesh
                DrawMesh(cmd, renderer.MeshId);
            }
        }
    }

    private void RenderTransparentGeometry(CommandBuffer cmd, Scene scene, Camera camera)
    {
        // Sort transparent objects back-to-front
        // Render with alpha blending
    }
}
```

## Resource Management

### Vertex Buffers (Planned)

```csharp
// Create vertex buffer
var vertices = new Vertex[]
{
    new Vertex { Position = new Vector3(-0.5f, -0.5f, 0), Color = new Vector3(1, 0, 0) },
    new Vertex { Position = new Vector3(0.5f, -0.5f, 0), Color = new Vector3(0, 1, 0) },
    new Vertex { Position = new Vector3(0, 0.5f, 0), Color = new Vector3(0, 0, 1) }
};

var vertexBuffer = CreateVertexBuffer(vertices);
```

### Uniform Buffers (Planned)

```csharp
// Camera uniform
struct CameraUniform
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;
    public Vector3 Position;
}

var cameraBuffer = CreateUniformBuffer<CameraUniform>();
UpdateUniformBuffer(cameraBuffer, cameraData);
```

### Textures (Planned)

```csharp
// Load texture
var texture = TextureLoader.LoadFromFile("textures/albedo.png");
var sampler = CreateTextureSampler(FilterMode.Linear, WrapMode.Repeat);
```

## Performance Considerations

### Batching

```csharp
// Batch similar draw calls (planned)
public class DrawBatch
{
    public Material Material { get; set; }
    public List<MeshInstance> Instances { get; set; }
}

// Draw all instances with same material in one call
foreach (var batch in batches)
{
    BindMaterial(batch.Material);
    DrawInstanced(batch.Instances);
}
```

### Frustum Culling

```csharp
// Cull objects outside camera view (planned)
public bool IsInFrustum(BoundingBox bounds, Camera camera)
{
    var frustum = camera.GetFrustum();
    return frustum.Intersects(bounds);
}

// Only render visible objects
foreach (var renderer in renderers)
{
    if (IsInFrustum(renderer.Bounds, camera))
    {
        RenderObject(renderer);
    }
}
```

### Level of Detail (LOD)

```csharp
// Use different mesh detail based on distance (planned)
public class LODGroup
{
    public Mesh HighDetail { get; set; }  // < 10 units
    public Mesh MediumDetail { get; set; }  // 10-50 units
    public Mesh LowDetail { get; set; }  // > 50 units
}

var distance = Vector3.Distance(objectPos, cameraPos);
var mesh = distance < 10 ? lod.HighDetail :
           distance < 50 ? lod.MediumDetail :
           lod.LowDetail;
```

## Best Practices

### ✅ Do's

- Batch draw calls with the same material
- Use frustum culling for large scenes
- Implement LOD for distant objects
- Sort transparent objects back-to-front
- Use texture atlases to reduce draw calls
- Pool and reuse command buffers

### ❌ Don'ts

- Don't rebind resources unnecessarily
- Don't render objects outside the view frustum
- Don't use unique materials for every object
- Don't allocate resources every frame
- Don't perform CPU-side calculations that could be done in shaders

## Future Features

### Planned Additions

- **Compute Shaders**: GPU-accelerated computations
- **Ray Tracing**: Real-time ray traced reflections and shadows (RTX)
- **Mesh Shaders**: Advanced geometry processing
- **Variable Rate Shading**: Performance optimization
- **HDR Rendering**: High dynamic range output
- **Temporal Anti-Aliasing**: Advanced anti-aliasing

## See Also

- [Vulkan](vulkan.md) - Vulkan integration
- [Materials](materials.md) - Material system
- [Resource Loading](../game-dev/resource-loading.md) - Loading assets
