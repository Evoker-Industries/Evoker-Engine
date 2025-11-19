# Material System

The Material system in Evoker Engine defines how objects appear when rendered, including textures, shaders, and rendering properties.

## Overview (Planned)

The material system will include:

- **Material**: Defines surface appearance
- **Shaders**: Vertex and fragment programs
- **Textures**: Albedo, normal, roughness, metallic maps
- **Properties**: Color, roughness, metallic values
- **Rendering Modes**: Opaque, transparent, cutout

## Material Class (Planned)

```csharp
public class Material
{
    public string Name { get; set; }
    public Shader Shader { get; set; }
    
    // Textures
    public Texture? AlbedoTexture { get; set; }
    public Texture? NormalTexture { get; set; }
    public Texture? MetallicTexture { get; set; }
    public Texture? RoughnessTexture { get; set; }
    public Texture? EmissiveTexture { get; set; }
    
    // Properties
    public Vector4 AlbedoColor { get; set; } = Vector4.One;
    public float Metallic { get; set; } = 0f;
    public float Roughness { get; set; } = 0.5f;
    public Vector3 Emissive { get; set; } = Vector3.Zero;
    
    // Rendering
    public RenderMode RenderMode { get; set; } = RenderMode.Opaque;
    public CullMode CullMode { get; set; } = CullMode.Back;
}

public enum RenderMode
{
    Opaque,       // Standard opaque rendering
    Transparent,  // Alpha blending
    Cutout        // Alpha testing
}

public enum CullMode
{
    Off,    // No culling (two-sided)
    Front,  // Cull front faces
    Back    // Cull back faces (default)
}
```

## Creating Materials (Planned)

### Simple Material

```csharp
var material = new Material
{
    Name = "SimpleMaterial",
    AlbedoColor = new Vector4(1, 0, 0, 1),  // Red
    Metallic = 0f,
    Roughness = 0.8f
};
```

### Textured Material

```csharp
var material = new Material
{
    Name = "TexturedMaterial",
    AlbedoTexture = TextureLoader.Load("textures/albedo.png"),
    NormalTexture = TextureLoader.Load("textures/normal.png"),
    MetallicTexture = TextureLoader.Load("textures/metallic.png"),
    RoughnessTexture = TextureLoader.Load("textures/roughness.png"),
    AlbedoColor = Vector4.One,
    Metallic = 1f,
    Roughness = 0.5f
};
```

### Emissive Material

```csharp
var glowMaterial = new Material
{
    Name = "GlowMaterial",
    AlbedoColor = new Vector4(0.2f, 0.2f, 0.2f, 1),
    Emissive = new Vector3(0, 1, 0),  // Green glow
    EmissiveTexture = TextureLoader.Load("textures/emission.png")
};
```

## PBR Materials (Physically Based Rendering)

### Metallic-Roughness Workflow

```csharp
public class PBRMaterial : Material
{
    // Metallic: 0 = dielectric, 1 = metal
    // Roughness: 0 = smooth, 1 = rough
    
    public static PBRMaterial CreateMetal(Vector4 color, float roughness)
    {
        return new PBRMaterial
        {
            AlbedoColor = color,
            Metallic = 1.0f,
            Roughness = roughness
        };
    }
    
    public static PBRMaterial CreateDielectric(Vector4 color, float roughness)
    {
        return new PBRMaterial
        {
            AlbedoColor = color,
            Metallic = 0.0f,
            Roughness = roughness
        };
    }
}

// Gold metal
var gold = PBRMaterial.CreateMetal(
    new Vector4(1f, 0.766f, 0.336f, 1f),
    roughness: 0.3f
);

// Plastic
var plastic = PBRMaterial.CreateDielectric(
    new Vector4(0.8f, 0.1f, 0.1f, 1f),
    roughness: 0.6f
);
```

## Common Materials

### Glass

```csharp
var glass = new Material
{
    Name = "Glass",
    AlbedoColor = new Vector4(1, 1, 1, 0.2f),  // Almost transparent
    Metallic = 0f,
    Roughness = 0.0f,  // Very smooth
    RenderMode = RenderMode.Transparent
};
```

### Wood

```csharp
var wood = new Material
{
    Name = "Wood",
    AlbedoTexture = TextureLoader.Load("textures/wood_albedo.png"),
    NormalTexture = TextureLoader.Load("textures/wood_normal.png"),
    RoughnessTexture = TextureLoader.Load("textures/wood_roughness.png"),
    Metallic = 0f,
    Roughness = 0.7f
};
```

### Stone

```csharp
var stone = new Material
{
    Name = "Stone",
    AlbedoTexture = TextureLoader.Load("textures/stone_albedo.png"),
    NormalTexture = TextureLoader.Load("textures/stone_normal.png"),
    Metallic = 0f,
    Roughness = 0.9f
};
```

## Shader System (Planned)

### Custom Shaders

```csharp
var customShader = new Shader
{
    VertexShaderPath = "shaders/custom.vert",
    FragmentShaderPath = "shaders/custom.frag"
};

var material = new Material
{
    Shader = customShader,
    // Custom properties...
};
```

### Shader Properties

```csharp
// Set custom shader properties (planned)
material.SetFloat("_TimeScale", 1.0f);
material.SetVector("_WindDirection", new Vector3(1, 0, 0));
material.SetTexture("_CustomMap", customTexture);
```

## Material Properties

### Tiling and Offset

```csharp
// Texture tiling (planned)
material.SetTextureScale("_MainTex", new Vector2(2, 2));  // Tile 2x
material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.5f));  // Offset
```

### Alpha Cutoff

```csharp
// For cutout materials (planned)
var foliage = new Material
{
    RenderMode = RenderMode.Cutout,
    AlphaClipThreshold = 0.5f,  // Discard pixels with alpha < 0.5
    AlbedoTexture = TextureLoader.Load("textures/leaves.png")
};
```

## Material Library Example

```csharp
public static class MaterialLibrary
{
    private static Dictionary<string, Material> _materials = new();

    public static void Initialize()
    {
        // Metals
        Register("gold", CreateGold());
        Register("silver", CreateSilver());
        Register("copper", CreateCopper());
        
        // Common materials
        Register("wood", CreateWood());
        Register("stone", CreateStone());
        Register("glass", CreateGlass());
        Register("plastic", CreatePlastic());
    }

    public static void Register(string name, Material material)
    {
        _materials[name] = material;
    }

    public static Material? Get(string name)
    {
        return _materials.TryGetValue(name, out var material) ? material : null;
    }

    private static Material CreateGold()
    {
        return new Material
        {
            Name = "Gold",
            AlbedoColor = new Vector4(1f, 0.766f, 0.336f, 1f),
            Metallic = 1.0f,
            Roughness = 0.3f
        };
    }

    private static Material CreateSilver()
    {
        return new Material
        {
            Name = "Silver",
            AlbedoColor = new Vector4(0.972f, 0.960f, 0.915f, 1f),
            Metallic = 1.0f,
            Roughness = 0.2f
        };
    }

    private static Material CreateWood()
    {
        return new Material
        {
            Name = "Wood",
            AlbedoTexture = TextureLoader.Load("textures/wood_albedo.png"),
            NormalTexture = TextureLoader.Load("textures/wood_normal.png"),
            Metallic = 0f,
            Roughness = 0.7f
        };
    }

    private static Material CreateGlass()
    {
        return new Material
        {
            Name = "Glass",
            AlbedoColor = new Vector4(1, 1, 1, 0.2f),
            Metallic = 0f,
            Roughness = 0f,
            RenderMode = RenderMode.Transparent
        };
    }

    private static Material CreateStone() { /* ... */ return new Material(); }
    private static Material CreateCopper() { /* ... */ return new Material(); }
    private static Material CreatePlastic() { /* ... */ return new Material(); }
}

// Usage
var goldMaterial = MaterialLibrary.Get("gold");
```

## Applying Materials to Objects

```csharp
var scene = Application.Instance.ActiveScene;
var entity = scene.CreateEntity("Sphere");

// Add mesh renderer
var renderer = scene.Registry.AddComponent<MeshRendererComponent>(entity);
renderer.MeshId = sphereMesh.Id;
renderer.MaterialId = goldMaterial.Id;  // Apply gold material
```

## Best Practices

### ✅ Do's

- Reuse materials when possible (material instancing)
- Use texture atlases to reduce draw calls
- Set appropriate roughness/metallic values for realism
- Use normal maps for surface detail
- Keep emissive values reasonable

### ❌ Don'ts

- Don't create unique materials for every object
- Don't use extremely high resolution textures (optimize)
- Don't forget to set render mode for transparent materials
- Don't mix units (keep texture scales consistent)

## Performance Tips

1. **Material Batching**: Group objects with same material
2. **Texture Atlasing**: Combine textures into atlases
3. **LOD Materials**: Use simpler materials for distant objects
4. **Texture Compression**: Use compressed texture formats

## See Also

- [Rendering Pipeline](rendering-pipeline.md) - Rendering system
- [Resource Loading](../game-dev/resource-loading.md) - Loading textures
- [Vulkan](vulkan.md) - Graphics API
