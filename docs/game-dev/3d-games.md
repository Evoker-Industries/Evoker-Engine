# Making 2D and 3D Games with Evoker-Engine

This guide shows you how to easily create both 2D and 3D games using Evoker-Engine.

## Table of Contents
- [2D Game Development](#2d-game-development)
- [3D Game Development](#3d-game-development)
- [Math Utilities](#math-utilities)
- [Complete Examples](#complete-examples)

## 2D Game Development

### Setting Up a 2D Scene

```csharp
using EvokerEngine.Core;
using EvokerEngine.Rendering;
using EvokerEngine.ECS;
using System.Numerics;

public class My2DGame : Layer
{
    private Scene.Scene _scene;
    private Entity _player;
    private Entity _camera;

    public override void OnAttach()
    {
        _scene = Application.Instance.ActiveScene;

        // Create 2D camera
        _camera = _scene.CreateEntity("2D Camera");
        var camera2D = _scene.Registry.AddComponent<Camera2DComponent>(_camera);
        camera2D.IsPrimary = true;
        camera2D.ViewportSize = new Vector2(1280, 720);
        camera2D.Zoom = 1f;

        // Create player sprite
        _player = _scene.CreateEntity("Player");
        var transform = _scene.Registry.AddComponent<TransformComponent>(_player);
        transform.Position = new Vector3(0, 0, 0);
        
        var sprite = _scene.Registry.AddComponent<SpriteComponent>(_player);
        sprite.TextureId = "player_sprite";
        sprite.Size = new Vector2(32, 32);
        sprite.Color = Vector4.One;
    }

    public override void OnUpdate(float deltaTime)
    {
        // Move player with WASD
        var transform = _scene.Registry.GetComponent<TransformComponent>(_player);
        var speed = 200f * deltaTime;

        if (Input.IsKeyPressed(Key.W))
            transform.Position += new Vector3(0, speed, 0);
        if (Input.IsKeyPressed(Key.S))
            transform.Position += new Vector3(0, -speed, 0);
        if (Input.IsKeyPressed(Key.A))
            transform.Position += new Vector3(-speed, 0, 0);
        if (Input.IsKeyPressed(Key.D))
            transform.Position += new Vector3(speed, 0, 0);

        // Camera follows player
        var camera2D = _scene.Registry.GetComponent<Camera2DComponent>(_camera);
        camera2D.Position = new Vector2(transform.Position.X, transform.Position.Y);
    }
}
```

### Creating Animated Sprites

```csharp
// Create animated character
var character = _scene.CreateEntity("Animated Character");
var animSprite = _scene.Registry.AddComponent<AnimatedSpriteComponent>(character);
animSprite.TextureId = "character_spritesheet";
animSprite.Size = new Vector2(64, 64);
animSprite.FrameRate = 10f;  // 10 frames per second
animSprite.Loop = true;

// Define animation frames (UV coordinates in sprite sheet)
animSprite.Frames = new Vector4[]
{
    new Vector4(0f, 0f, 0.25f, 0.25f),    // Frame 1
    new Vector4(0.25f, 0f, 0.5f, 0.25f),  // Frame 2
    new Vector4(0.5f, 0f, 0.75f, 0.25f),  // Frame 3
    new Vector4(0.75f, 0f, 1f, 0.25f),    // Frame 4
};

animSprite.Play();  // Start animation

// Update in OnUpdate:
animSprite.Update(deltaTime);
```

### Creating a Tilemap

```csharp
// Create tilemap entity
var tilemapEntity = _scene.CreateEntity("Level Tilemap");
var tilemap = _scene.Registry.AddComponent<TilemapComponent>(tilemapEntity);

// Initialize tilemap
tilemap.TilesetId = "tileset_texture";
tilemap.TileSize = new Vector2(32, 32);
tilemap.Initialize(50, 50);  // 50x50 tiles

// Set tiles
for (int y = 0; y < tilemap.Height; y++)
{
    for (int x = 0; x < tilemap.Width; x++)
    {
        if (y == tilemap.Height - 1)
            tilemap.SetTile(x, y, 1);  // Ground tile
        else
            tilemap.SetTile(x, y, 0);  // Sky tile
    }
}
```

### 2D Camera Controls

```csharp
// Zoom with mouse wheel
var camera2D = _scene.Registry.GetComponent<Camera2DComponent>(_camera);
var scroll = Input.GetMouseScroll();
camera2D.Zoom += scroll.Y * 0.1f;
camera2D.Zoom = Math.Clamp(camera2D.Zoom, 0.1f, 5f);

// Smooth camera follow
var targetPos = new Vector2(playerTransform.Position.X, playerTransform.Position.Y);
camera2D.Position = Vector2.Lerp(
    camera2D.Position,
    targetPos,
    5f * deltaTime  // Smooth factor
);

// Convert screen to world position (for mouse clicks)
var mouseScreenPos = Input.GetMousePosition();
var mouseWorldPos = camera2D.ScreenToWorld(mouseScreenPos);
```

## 3D Game Development

### Setting Up a 3D Scene

```csharp
using EvokerEngine.Core;
using EvokerEngine.Rendering;
using EvokerEngine.Utilities;
using System.Numerics;

public class My3DGame : Layer
{
    private Scene.Scene _scene;
    private Entity _player;
    private Entity _camera;
    private float _cameraAngle = 0f;

    public override void OnAttach()
    {
        _scene = Application.Instance.ActiveScene;

        // Create 3D camera
        _camera = _scene.CreateEntity("3D Camera");
        var cameraTransform = _scene.Registry.AddComponent<TransformComponent>(_camera);
        cameraTransform.Position = new Vector3(0, 5, 10);
        
        var camera3D = _scene.Registry.AddComponent<Camera3DComponent>(_camera);
        camera3D.IsPrimary = true;
        camera3D.FieldOfView = 60f;
        camera3D.AspectRatio = 16f / 9f;
        camera3D.NearPlane = 0.1f;
        camera3D.FarPlane = 1000f;

        // Create player with mesh
        _player = _scene.CreateEntity("Player");
        var playerTransform = _scene.Registry.AddComponent<TransformComponent>(_player);
        playerTransform.Position = new Vector3(0, 0, 0);
        playerTransform.Scale = Vector3.One;
        
        var mesh = _scene.Registry.AddComponent<MeshComponent>(_player);
        mesh.MeshId = "player_model";
        mesh.MaterialId = "player_material";
        mesh.CastShadows = true;
    }

    public override void OnUpdate(float deltaTime)
    {
        // Player movement
        var transform = _scene.Registry.GetComponent<TransformComponent>(_player);
        var speed = 5f * deltaTime;

        if (Input.IsKeyPressed(Key.W))
            transform.Position += new Vector3(0, 0, -speed);
        if (Input.IsKeyPressed(Key.S))
            transform.Position += new Vector3(0, 0, speed);
        if (Input.IsKeyPressed(Key.A))
            transform.Position += new Vector3(-speed, 0, 0);
        if (Input.IsKeyPressed(Key.D))
            transform.Position += new Vector3(speed, 0, 0);

        // Third-person camera
        var cameraTransform = _scene.Registry.GetComponent<TransformComponent>(_camera);
        _cameraAngle += Input.GetMouseDelta().X * 0.1f;
        
        var offset = VectorHelper.RotateAround(
            new Vector3(0, 5, 10),
            Vector3.Zero,
            Vector3.UnitY,
            _cameraAngle
        );
        
        cameraTransform.Position = transform.Position + offset;
        cameraTransform.Rotation = QuaternionHelper.ToEuler(
            QuaternionHelper.LookRotation(
                transform.Position - cameraTransform.Position,
                Vector3.UnitY
            )
        );
    }
}
```

### Adding Lighting

```csharp
// Directional light (sun)
var sun = _scene.CreateEntity("Sun");
var sunTransform = _scene.Registry.AddComponent<TransformComponent>(sun);
sunTransform.Rotation = new Vector3(45, 45, 0);

var sunLight = _scene.Registry.AddComponent<LightComponent>(sun);
sunLight.Type = LightType.Directional;
sunLight.Color = new Vector3(1f, 0.95f, 0.9f);  // Warm sunlight
sunLight.Intensity = 1.5f;
sunLight.CastShadows = true;

// Point light (torch)
var torch = _scene.CreateEntity("Torch");
var torchTransform = _scene.Registry.AddComponent<TransformComponent>(torch);
torchTransform.Position = new Vector3(5, 2, 0);

var torchLight = _scene.Registry.AddComponent<LightComponent>(torch);
torchLight.Type = LightType.Point;
torchLight.Color = new Vector3(1f, 0.6f, 0.2f);  // Orange firelight
torchLight.Intensity = 10f;
torchLight.Range = 15f;

// Spot light (flashlight)
var flashlight = _scene.CreateEntity("Flashlight");
var flashlightLight = _scene.Registry.AddComponent<LightComponent>(flashlight);
flashlightLight.Type = LightType.Spot;
flashlightLight.Color = Vector3.One;
flashlightLight.Intensity = 20f;
flashlightLight.Range = 50f;
flashlightLight.SpotAngle = 30f;
```

### Adding Physics

```csharp
// Create a physics-enabled ball
var ball = _scene.CreateEntity("Ball");
var ballTransform = _scene.Registry.AddComponent<TransformComponent>(ball);
ballTransform.Position = new Vector3(0, 10, 0);

var mesh = _scene.Registry.AddComponent<MeshComponent>(ball);
mesh.MeshId = "sphere_mesh";

var rigidbody = _scene.Registry.AddComponent<RigidbodyComponent>(ball);
rigidbody.Mass = 1f;
rigidbody.UseGravity = true;
rigidbody.Drag = 0.1f;

var collider = _scene.Registry.AddComponent<ColliderComponent>(ball);
collider.Type = ColliderType.Sphere;
collider.Radius = 0.5f;

// Apply force
rigidbody.AddForce(new Vector3(5, 0, 0));

// Apply impulse (instant velocity change)
rigidbody.AddImpulse(new Vector3(0, 10, 0));
```

### Particle Effects

```csharp
// Create particle system for fire effect
var fire = _scene.CreateEntity("Fire");
var fireTransform = _scene.Registry.AddComponent<TransformComponent>(fire);
fireTransform.Position = new Vector3(0, 0, 0);

var particles = _scene.Registry.AddComponent<ParticleSystemComponent>(fire);
particles.MaxParticles = 500;
particles.EmissionRate = 100f;
particles.Lifetime = 1.5f;
particles.StartSize = 0.3f;
particles.EndSize = 0.1f;
particles.StartColor = new Vector4(1f, 0.8f, 0.2f, 1f);  // Yellow-orange
particles.EndColor = new Vector4(1f, 0.2f, 0f, 0f);      // Fade to red
particles.Velocity = new Vector3(0, 3, 0);
particles.VelocityVariation = new Vector3(0.5f, 0.5f, 0.5f);
particles.Gravity = new Vector3(0, 1f, 0);  // Rise upward
particles.IsPlaying = true;
particles.Loop = true;
```

### Skybox

```csharp
// Add skybox
var skybox = _scene.CreateEntity("Skybox");
var skyboxComponent = _scene.Registry.AddComponent<SkyboxComponent>(skybox);
skyboxComponent.CubemapId = "sky_cubemap";
skyboxComponent.Tint = Vector3.One;
skyboxComponent.Exposure = 1.2f;
skyboxComponent.Rotation = 0f;  // Rotate skybox
```

## Math Utilities

### Interpolation

```csharp
using EvokerEngine.Utilities;

// Lerp (linear interpolation)
float a = 0f, b = 10f;
float result = MathHelper.Lerp(a, b, 0.5f);  // 5.0

// Smooth interpolation (ease in/out)
float smooth = MathHelper.SmoothStep(0f, 10f, 0.5f);

// Vector lerp
var start = new Vector3(0, 0, 0);
var end = new Vector3(10, 10, 10);
var interpolated = VectorHelper.Lerp(start, end, 0.5f);

// Move towards target
float current = 0f;
float target = 100f;
float newValue = MathHelper.MoveTowards(current, target, 10f * deltaTime);
```

### Quaternion and Rotation

```csharp
// Create rotation from Euler angles
var rotation = QuaternionHelper.FromEuler(new Vector3(45, 90, 0));

// Convert back to Euler
var euler = QuaternionHelper.ToEuler(rotation);

// Slerp between rotations
var rotA = QuaternionHelper.FromEuler(new Vector3(0, 0, 0));
var rotB = QuaternionHelper.FromEuler(new Vector3(0, 180, 0));
var interpolated = QuaternionHelper.Slerp(rotA, rotB, 0.5f);

// Look at rotation
var forward = VectorHelper.Direction(position, targetPosition);
var lookRotation = QuaternionHelper.LookRotation(forward, Vector3.UnitY);
```

### Random Utilities

```csharp
// Random float between 0 and 1
float random = RandomHelper.Value;

// Random in range
float randomValue = RandomHelper.Range(-10f, 10f);
int randomInt = RandomHelper.Range(0, 100);

// Random point in circle/sphere
var pointInCircle = RandomHelper.InsideUnitCircle();
var pointInSphere = RandomHelper.InsideUnitSphere();
var pointOnSphere = RandomHelper.OnUnitSphere();

// Random color
var randomColor = RandomHelper.ColorRGBA();
```

## Complete Examples

### Simple 2D Platformer

```csharp
public class Platformer2D : Layer
{
    private Entity _player;
    private Entity _camera;
    private Vector3 _velocity = Vector3.Zero;
    private bool _isGrounded = false;

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;

        // Camera
        _camera = scene.CreateEntity("Camera");
        var camera2D = scene.Registry.AddComponent<Camera2DComponent>(_camera);
        camera2D.IsPrimary = true;

        // Player
        _player = scene.CreateEntity("Player");
        var transform = scene.Registry.AddComponent<TransformComponent>(_player);
        transform.Position = new Vector3(0, 5, 0);
        
        var sprite = scene.Registry.AddComponent<AnimatedSpriteComponent>(_player);
        sprite.TextureId = "player_spritesheet";
        sprite.Size = new Vector2(32, 48);
    }

    public override void OnUpdate(float deltaTime)
    {
        var scene = Application.Instance.ActiveScene;
        var transform = scene.Registry.GetComponent<TransformComponent>(_player);

        // Horizontal movement
        float moveInput = 0f;
        if (Input.IsKeyPressed(Key.A)) moveInput -= 1f;
        if (Input.IsKeyPressed(Key.D)) moveInput += 1f;

        _velocity.X = moveInput * 200f * deltaTime;

        // Jumping
        if (Input.IsKeyPressed(Key.Space) && _isGrounded)
        {
            _velocity.Y = 500f * deltaTime;
            _isGrounded = false;
        }

        // Gravity
        if (!_isGrounded)
        {
            _velocity.Y -= 980f * deltaTime;
        }

        // Apply movement
        transform.Position += _velocity;

        // Simple ground check
        if (transform.Position.Y <= 0f)
        {
            transform.Position = new Vector3(transform.Position.X, 0f, 0f);
            _velocity.Y = 0f;
            _isGrounded = true;
        }

        // Camera follow
        var camera2D = scene.Registry.GetComponent<Camera2DComponent>(_camera);
        camera2D.Position = new Vector2(transform.Position.X, transform.Position.Y);
    }
}
```

### Simple 3D FPS Controller

```csharp
public class FPSController : Layer
{
    private Entity _player;
    private Entity _camera;
    private float _pitch = 0f;
    private float _yaw = 0f;

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;

        // Player (body)
        _player = scene.CreateEntity("Player");
        var playerTransform = scene.Registry.AddComponent<TransformComponent>(_player);
        playerTransform.Position = new Vector3(0, 1.8f, 0);

        // Camera (eyes)
        _camera = scene.CreateEntity("Camera");
        var cameraTransform = scene.Registry.AddComponent<TransformComponent>(_camera);
        
        var camera3D = scene.Registry.AddComponent<Camera3DComponent>(_camera);
        camera3D.IsPrimary = true;
        camera3D.FieldOfView = 90f;

        // Weapon
        var weapon = scene.CreateEntity("Weapon");
        var weaponMesh = scene.Registry.AddComponent<MeshComponent>(weapon);
        weaponMesh.MeshId = "gun_model";
    }

    public override void OnUpdate(float deltaTime)
    {
        var scene = Application.Instance.ActiveScene;
        var playerTransform = scene.Registry.GetComponent<TransformComponent>(_player);
        var cameraTransform = scene.Registry.GetComponent<TransformComponent>(_camera);

        // Mouse look
        var mouseDelta = Input.GetMouseDelta();
        _yaw += mouseDelta.X * 0.1f;
        _pitch -= mouseDelta.Y * 0.1f;
        _pitch = Math.Clamp(_pitch, -89f, 89f);

        // Calculate forward and right vectors
        var yawRad = _yaw * MathHelper.Deg2Rad;
        var forward = new Vector3(
            (float)Math.Sin(yawRad),
            0,
            (float)Math.Cos(yawRad)
        );
        var right = Vector3.Cross(forward, Vector3.UnitY);

        // Movement
        var speed = 5f * deltaTime;
        if (Input.IsKeyPressed(Key.W)) playerTransform.Position += forward * speed;
        if (Input.IsKeyPressed(Key.S)) playerTransform.Position -= forward * speed;
        if (Input.IsKeyPressed(Key.A)) playerTransform.Position -= right * speed;
        if (Input.IsKeyPressed(Key.D)) playerTransform.Position += right * speed;

        // Camera position and rotation
        cameraTransform.Position = playerTransform.Position;
        cameraTransform.Rotation = new Vector3(_pitch, _yaw, 0);
    }
}
```

## Tips for Game Development

### 2D Games
- Use `Camera2DComponent` for proper 2D perspective
- Use `SpriteComponent` for static images
- Use `AnimatedSpriteComponent` for character animations
- Use `TilemapComponent` for level layouts
- Keep Z coordinate at 0 for pure 2D

### 3D Games
- Use `Camera3DComponent` for 3D perspective
- Use `MeshComponent` for 3D models
- Add `LightComponent` for proper lighting
- Use `RigidbodyComponent` for physics
- Use `ColliderComponent` for collision detection
- Use `ParticleSystemComponent` for effects

### Performance
- Batch similar objects together
- Use object pooling for frequently spawned objects
- Limit particle counts
- Use LOD (Level of Detail) for distant objects
- Cull objects outside camera view

Happy game development! 🎮
