# Resource Loading Examples

This guide demonstrates how to use the newly implemented resource loading features.

## Texture Loading

### Load from File

```csharp
using EvokerEngine.Resources;

// Create and load a texture
var texture = new Texture { Name = "MyTexture", FilePath = "assets/textures/player.png" };
texture.Load(); // Automatically loads from FilePath

// Or load directly
texture.LoadFromFile("assets/textures/enemy.png");

// Access texture data
Console.WriteLine($"Size: {texture.Width}x{texture.Height}");
Console.WriteLine($"Format: {texture.Format}"); // RGBA
Console.WriteLine($"Data size: {texture.Data?.Length} bytes");
```

**Supported formats:**
- PNG
- JPG/JPEG
- BMP
- TGA

### Load from Memory

```csharp
byte[] imageData = File.ReadAllBytes("image.png");
var texture = new Texture { Name = "MemoryTexture" };
texture.LoadFromMemory(imageData);
```

### Create Solid Color Texture

```csharp
// Create a 256x256 red texture
var redTexture = Texture.CreateSolidColor(256, 256, 255, 0, 0, 255);

// White texture
var whiteTexture = Texture.CreateSolidColor(64, 64, 255, 255, 255, 255);

// Transparent blue
var blueTexture = Texture.CreateSolidColor(128, 128, 0, 0, 255, 128);
```

### Using with Resource Manager

```csharp
var resourceManager = Application.Instance.ResourceManager;

// Load through resource manager
var texture = resourceManager.Load<Texture>("PlayerTexture");
texture.FilePath = "assets/player.png";
texture.Load();

// Get by ID
var loadedTexture = resourceManager.Get<Texture>(texture.Id);

// Unload when done
resourceManager.Unload(texture.Id);
```

## Mesh Generation

### Create Primitives

```csharp
using EvokerEngine.Resources;

// Create a cube (2x2x2)
var cube = Mesh.CreateCube(2.0f);
Console.WriteLine($"Cube: {cube.Vertices.Length / 3} vertices, {cube.Indices.Length / 3} triangles");

// Create a plane (10x10)
var plane = Mesh.CreatePlane(10.0f, 10.0f);

// Create a sphere (radius 1.0, 32 segments)
var sphere = Mesh.CreateSphere(1.0f, segments: 32, rings: 32);
```

### Access Mesh Data

```csharp
var cube = Mesh.CreateCube(1.0f);

// Vertices (x, y, z)
float[] vertices = cube.Vertices;

// Indices
uint[] indices = cube.Indices;

// Normals (x, y, z)
float[] normals = cube.Normals;

// Texture coordinates (u, v)
float[] texCoords = cube.TexCoords;

// Iterate vertices
for (int i = 0; i < vertices.Length; i += 3)
{
    float x = vertices[i];
    float y = vertices[i + 1];
    float z = vertices[i + 2];
    Console.WriteLine($"Vertex: ({x}, {y}, {z})");
}
```

### Custom Mesh from Data

```csharp
var mesh = new Mesh
{
    Name = "CustomMesh",
    Vertices = new float[] { /* vertex data */ },
    Indices = new uint[] { /* index data */ },
    Normals = new float[] { /* normal data */ },
    TexCoords = new float[] { /* UV data */ }
};
```

## Shader Loading

### Load Shader Files

```csharp
using EvokerEngine.Resources;

// Load shader from files
var shader = new Shader { Name = "BasicShader" };
shader.LoadFromFiles("shaders/vertex.glsl", "shaders/fragment.glsl");

// Compile (marks as ready, full SPIR-V compilation coming soon)
bool success = shader.Compile();
if (success)
{
    Console.WriteLine("Shader ready!");
}
```

### Load from Source Strings

```csharp
string vertexSource = @"
#version 450
layout(location = 0) in vec3 inPosition;
void main() {
    gl_Position = vec4(inPosition, 1.0);
}
";

string fragmentSource = @"
#version 450
layout(location = 0) out vec4 outColor;
void main() {
    outColor = vec4(1.0, 0.0, 0.0, 1.0);
}
";

var shader = new Shader { Name = "InlineShader" };
shader.LoadFromSource(vertexSource, fragmentSource);
shader.Compile();
```

### Using with Resource Manager

```csharp
var shader = resourceManager.Load<Shader>("MainShader");
shader.VertexShaderPath = "shaders/main.vert";
shader.FragmentShaderPath = "shaders/main.frag";
shader.Load();
shader.Compile();
```

## Scripting System

### Create a Custom Script

```csharp
using EvokerEngine.Scripting;
using EvokerEngine.ECS;
using System.Numerics;

public class PlayerController : Script
{
    public float Speed = 5.0f;
    public float JumpForce = 10.0f;

    public override void OnStart()
    {
        Core.Logger.Info("Player controller initialized");
    }

    public override void OnUpdate(float deltaTime)
    {
        var transform = GetComponent<TransformComponent>();
        if (transform == null) return;

        // Movement
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.W))
            transform.Position += new Vector3(0, 0, -Speed * deltaTime);
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.S))
            transform.Position += new Vector3(0, 0, Speed * deltaTime);
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.A))
            transform.Position += new Vector3(-Speed * deltaTime, 0, 0);
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.D))
            transform.Position += new Vector3(Speed * deltaTime, 0, 0);

        // Jump
        if (Core.Input.IsKeyPressed(Silk.NET.Input.Key.Space))
        {
            // Add jump logic here
        }
    }
}
```

### Attach Script to Entity

```csharp
using EvokerEngine.Scripting;

var scene = Application.Instance.ActiveScene;
var player = scene.CreateEntity("Player");

// Add transform component
var transform = scene.Registry.AddComponent<TransformComponent>(player);
transform.Position = Vector3.Zero;

// Add script component
var scriptComponent = scene.Registry.AddComponent<ScriptComponent>(player);
scriptComponent.Script = new PlayerController();

// Register with script manager
ScriptManager.Instance.RegisterScriptComponent(scriptComponent);
```

### Update Scripts

```csharp
// In your game loop (Layer's OnUpdate method)
public override void OnUpdate(float deltaTime)
{
    // Update all scripts
    ScriptManager.Instance.Update(deltaTime);
}

// For physics
public override void OnFixedUpdate(float fixedDeltaTime)
{
    ScriptManager.Instance.FixedUpdate(fixedDeltaTime);
}
```

### Built-in Example Scripts

```csharp
// Simple movement
var movementScript = new SimpleMovementScript { Speed = 8.0f };

// Simple rotation
var rotationScript = new SimpleRotationScript { RotationSpeed = 90.0f };

// Attach to entity
scriptComponent.Script = movementScript;
```

## Complete Example

Here's a complete example combining all features:

```csharp
using EvokerEngine.Core;
using EvokerEngine.Resources;
using EvokerEngine.Scripting;
using EvokerEngine.ECS;

var app = new Application("Resource Demo", 1280, 720);
app.PushLayer(new ResourceDemoLayer());
app.Run();

class ResourceDemoLayer : Layer
{
    private Entity _cube;

    public ResourceDemoLayer() : base("Resource Demo") { }

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;
        var resourceManager = Application.Instance.ResourceManager;

        // Load texture
        var texture = new Texture { Name = "CubeTexture" };
        texture.LoadFromFile("assets/cube_texture.png");
        Logger.Info($"Loaded texture: {texture.Width}x{texture.Height}");

        // Create mesh
        var cubeMesh = Mesh.CreateCube(2.0f);
        Logger.Info($"Created cube with {cubeMesh.Vertices.Length / 3} vertices");

        // Load shader
        var shader = new Shader { Name = "BasicShader" };
        shader.LoadFromFiles("shaders/basic.vert", "shaders/basic.frag");
        shader.Compile();

        // Create entity with script
        _cube = scene.CreateEntity("Cube");
        
        var transform = scene.Registry.AddComponent<TransformComponent>(_cube);
        transform.Position = new Vector3(0, 0, 0);

        var scriptComponent = scene.Registry.AddComponent<ScriptComponent>(_cube);
        scriptComponent.Script = new SimpleRotationScript { RotationSpeed = 45.0f };
        
        ScriptManager.Instance.RegisterScriptComponent(scriptComponent);
    }

    public override void OnUpdate(float deltaTime)
    {
        // Update all scripts
        ScriptManager.Instance.Update(deltaTime);
    }
}
```

## Next Steps

- Learn about [3D Game Development](3d-games.md)
- Explore [Core Systems](../core/ecs.md)
- Check out [API Reference](../api/overview.md)
