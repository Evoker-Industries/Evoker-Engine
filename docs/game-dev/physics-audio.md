---
tags:
  - gamedev
  - physics
  - audio
---

# Physics and Audio Systems Guide

This guide demonstrates how to use the fully implemented physics and audio systems in Evoker Engine.

## Physics System

The physics system provides complete rigidbody simulation with collision detection and response.

### Initializing Physics

```csharp
using EvokerEngine.Physics;

// Initialize the physics system
PhysicsSystem.Instance.Initialize();

// Set custom gravity
PhysicsSystem.Instance.Gravity = new Vector3(0, -20f, 0); // Stronger gravity
```

### Creating Physics Objects

#### Box Collider with Rigidbody

```csharp
using EvokerEngine.Physics;
using EvokerEngine.ECS;
using System.Numerics;

var scene = Application.Instance.ActiveScene;

// Create entity
var box = scene.CreateEntity("PhysicsBox");

// Add transform
var transform = scene.Registry.AddComponent<TransformComponent>(box);
transform.Position = new Vector3(0, 10, 0);

// Add rigidbody for physics
var rigidbody = scene.Registry.AddComponent<RigidbodyComponent>(box);
rigidbody.Mass = 2.0f;
rigidbody.UseGravity = true;
rigidbody.Drag = 0.1f;

// Add box collider
var collider = scene.Registry.AddComponent<BoxColliderComponent>(box);
collider.Size = new Vector3(1, 1, 1);

// Register with physics system
PhysicsSystem.Instance.RegisterRigidbody(box);
PhysicsSystem.Instance.RegisterCollider(box);
```

#### Sphere Collider

```csharp
var sphere = scene.CreateEntity("PhysicsSphere");

var transform = scene.Registry.AddComponent<TransformComponent>(sphere);
transform.Position = new Vector3(5, 5, 0);

var rigidbody = scene.Registry.AddComponent<RigidbodyComponent>(sphere);
rigidbody.Mass = 1.0f;
rigidbody.UseGravity = true;

var collider = scene.Registry.AddComponent<SphereColliderComponent>(sphere);
collider.Radius = 0.5f;

PhysicsSystem.Instance.RegisterRigidbody(sphere);
PhysicsSystem.Instance.RegisterCollider(sphere);
```

#### Static Collider (No Rigidbody)

```csharp
// Create ground - no rigidbody, won't move
var ground = scene.CreateEntity("Ground");

var transform = scene.Registry.AddComponent<TransformComponent>(ground);
transform.Position = new Vector3(0, 0, 0);

var collider = scene.Registry.AddComponent<BoxColliderComponent>(ground);
collider.Size = new Vector3(20, 1, 20); // Large platform

PhysicsSystem.Instance.RegisterCollider(ground);
```

### Applying Forces

```csharp
// Get rigidbody component
var rigidbody = scene.Registry.GetComponent<RigidbodyComponent>(entity);

// Apply continuous force
rigidbody.AddForce(new Vector3(10, 0, 0)); // Push right

// Apply impulse (instant velocity change)
rigidbody.AddImpulse(new Vector3(0, 5, 0)); // Jump

// Apply torque (rotation)
rigidbody.AddTorque(new Vector3(0, 1, 0)); // Spin

// Set velocity directly
rigidbody.Velocity = new Vector3(5, 0, 0);
```

### Kinematic Rigidbodies

```csharp
// Kinematic objects are controlled manually, not by physics
rigidbody.IsKinematic = true;
rigidbody.UseGravity = false; // Usually disabled for kinematic

// Move kinematic object in update
transform.Position += new Vector3(1, 0, 0) * deltaTime;
// Other objects will collide with it, but it won't respond to forces
```

### Raycasting

```csharp
Vector3 origin = new Vector3(0, 10, 0);
Vector3 direction = new Vector3(0, -1, 0); // Downward

if (PhysicsSystem.Instance.Raycast(origin, direction, out RaycastHit hit, maxDistance: 100f))
{
    Logger.Info($"Hit: {hit.Point} at distance {hit.Distance}");
    Logger.Info($"Normal: {hit.Normal}");
    
    // Hit entity
    var hitEntity = hit.Entity;
    var hitCollider = hit.Collider;
}
```

### Overlap Detection

```csharp
// Check if sphere overlaps with any colliders
Vector3 position = new Vector3(0, 5, 0);
float radius = 2.0f;

if (PhysicsSystem.Instance.CheckSphere(position, radius))
{
    Logger.Info("Sphere overlaps with collider(s)");
}
```

### Updating Physics

```csharp
// In your game layer's OnUpdate or OnFixedUpdate
public class PhysicsLayer : Layer
{
    private const float FixedTimeStep = 1f / 60f; // 60 FPS physics
    private float _accumulator = 0f;

    public override void OnUpdate(float deltaTime)
    {
        // Accumulate time for fixed timestep
        _accumulator += deltaTime;

        // Update physics at fixed rate
        while (_accumulator >= FixedTimeStep)
        {
            PhysicsSystem.Instance.Update(FixedTimeStep);
            _accumulator -= FixedTimeStep;
        }
    }
}
```

### Physics Properties

```csharp
// Rigidbody properties
rigidbody.Mass = 5.0f;              // Heavier = more force needed
rigidbody.Drag = 0.5f;              // Air resistance (0 = no drag)
rigidbody.AngularDrag = 0.05f;      // Rotation damping
rigidbody.UseGravity = true;        // Affected by gravity
rigidbody.IsKinematic = false;      // Controlled by physics vs. manually

// Collider properties
collider.Center = new Vector3(0, 0.5f, 0);  // Offset from entity position
collider.IsTrigger = false;                  // Physical collision vs. trigger
collider.Material = new PhysicsMaterial
{
    StaticFriction = 0.6f,
    DynamicFriction = 0.4f,
    Bounciness = 0.5f
};
```

## Audio System

The audio system provides 3D spatial audio with OpenAL.

### Initializing Audio

```csharp
using EvokerEngine.Audio;

// Initialize the audio system
AudioSystem.Instance.Initialize();

// Check if initialized successfully
if (AudioSystem.Instance.IsInitialized)
{
    Logger.Info("Audio system ready!");
}
```

### Loading Audio Clips

```csharp
// Load WAV file
var clip = new AudioClip { Name = "Explosion" };
clip.LoadFromFile("assets/audio/explosion.wav");

Logger.Info($"Loaded: {clip.Duration:F2}s, {clip.SampleRate}Hz, {clip.Channels}ch");
```

### Playing Audio

#### Simple Playback

```csharp
// Create audio source
var source = AudioSystem.Instance.CreateSource();

// Load and set clip
uint buffer = AudioSystem.Instance.LoadClip(clip);
source.SetBuffer(buffer);
source.Clip = clip;

// Configure source
source.Volume = 0.8f;
source.Pitch = 1.0f;
source.Loop = false;

// Play
source.Play();

// Control playback
source.Pause();
source.Stop();

// Check status
if (source.IsPlaying)
{
    Logger.Info("Audio is playing");
}
```

#### One-Shot Sound Effects

```csharp
// Play a sound effect without managing source manually
AudioSystem.Instance.PlayOneShot(clip, volume: 0.7f);
```

### Audio Properties

```csharp
// Volume (0.0 to 1.0)
source.Volume = 0.5f;

// Pitch (0.5 to 2.0)
source.Pitch = 1.5f; // Higher pitch = faster playback

// Looping
source.Loop = true;

// Mute
source.Mute = true;
```

### 3D Spatial Audio

```csharp
// Set up audio listener (usually attached to camera)
var listener = new AudioListener();
listener.Position = cameraPosition;
listener.Velocity = cameraVelocity;
listener.Volume = 1.0f;

// For 3D positioned audio sources, you can set source position
// (This would require extending the AudioSource class with position properties)
```

### Multiple Audio Sources

```csharp
// Create multiple sources for layered audio
var musicSource = AudioSystem.Instance.CreateSource();
musicSource.Loop = true;
musicSource.Volume = 0.3f;

var sfxSource = AudioSystem.Instance.CreateSource();
sfxSource.Loop = false;
sfxSource.Volume = 1.0f;

// Play background music
uint musicBuffer = AudioSystem.Instance.LoadClip(musicClip);
musicSource.SetBuffer(musicBuffer);
musicSource.Clip = musicClip;
musicSource.Play();

// Play sound effects on top
AudioSystem.Instance.PlayOneShot(explosionClip);
```

### Cleanup

```csharp
// Stop all audio and cleanup
AudioSystem.Instance.Shutdown();
```

## Complete Example: Physics + Audio

Here's a complete example combining physics and audio:

```csharp
using EvokerEngine.Core;
using EvokerEngine.Physics;
using EvokerEngine.Audio;
using EvokerEngine.ECS;
using System.Numerics;

var app = new Application("Physics & Audio Demo", 1280, 720);
app.PushLayer(new PhysicsAudioLayer());
app.Run();

class PhysicsAudioLayer : Layer
{
    private Entity _ball;
    private Entity _ground;
    private AudioClip? _bounceClip;
    private const float FixedTimeStep = 1f / 60f;
    private float _accumulator = 0f;
    private Vector3 _lastPosition;
    private bool _hasLanded = false;

    public PhysicsAudioLayer() : base("Physics & Audio") { }

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;

        // Initialize systems
        PhysicsSystem.Instance.Initialize();
        AudioSystem.Instance.Initialize();

        // Load audio
        _bounceClip = new AudioClip { Name = "Bounce" };
        _bounceClip.LoadFromFile("assets/audio/bounce.wav");

        // Create ground
        _ground = scene.CreateEntity("Ground");
        var groundTransform = scene.Registry.AddComponent<TransformComponent>(_ground);
        groundTransform.Position = new Vector3(0, 0, 0);
        
        var groundCollider = scene.Registry.AddComponent<BoxColliderComponent>(_ground);
        groundCollider.Size = new Vector3(20, 1, 20);
        
        PhysicsSystem.Instance.RegisterCollider(_ground);

        // Create bouncing ball
        _ball = scene.CreateEntity("Ball");
        var ballTransform = scene.Registry.AddComponent<TransformComponent>(_ball);
        ballTransform.Position = new Vector3(0, 10, 0);
        _lastPosition = ballTransform.Position;

        var rigidbody = scene.Registry.AddComponent<RigidbodyComponent>(_ball);
        rigidbody.Mass = 1.0f;
        rigidbody.UseGravity = true;
        rigidbody.Drag = 0.01f;

        var collider = scene.Registry.AddComponent<SphereColliderComponent>(_ball);
        collider.Radius = 0.5f;

        PhysicsSystem.Instance.RegisterRigidbody(_ball);
        PhysicsSystem.Instance.RegisterCollider(_ball);

        Logger.Info("Physics & Audio demo ready! Ball will bounce and play sound.");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Fixed timestep physics
        _accumulator += deltaTime;
        while (_accumulator >= FixedTimeStep)
        {
            PhysicsSystem.Instance.Update(FixedTimeStep);
            _accumulator -= FixedTimeStep;
        }

        // Check for collision (velocity change indicates bounce)
        var scene = Application.Instance.ActiveScene;
        if (scene == null) return;

        var transform = scene.Registry.GetComponent<TransformComponent>(_ball);
        var rigidbody = scene.Registry.GetComponent<RigidbodyComponent>(_ball);

        if (transform != null && rigidbody != null)
        {
            // Detect bounce by checking if Y velocity flipped
            if (!_hasLanded && transform.Position.Y < 1.0f && rigidbody.Velocity.Y > 0.1f)
            {
                // Ball bounced!
                if (_bounceClip != null && AudioSystem.Instance.IsInitialized)
                {
                    AudioSystem.Instance.PlayOneShot(_bounceClip, volume: 0.8f);
                }
                _hasLanded = true;
            }
            else if (transform.Position.Y > 2.0f)
            {
                _hasLanded = false; // Reset for next bounce
            }

            // Log position
            if (Time.TotalTime % 1.0f < deltaTime)
            {
                Logger.Info($"Ball position: {transform.Position.Y:F2}m, velocity: {rigidbody.Velocity.Y:F2}m/s");
            }
        }

        // Reset ball if it falls too far
        if (transform != null && transform.Position.Y < -5)
        {
            transform.Position = new Vector3(0, 10, 0);
            if (rigidbody != null)
                rigidbody.Velocity = Vector3.Zero;
        }
    }

    public override void OnDetach()
    {
        PhysicsSystem.Instance.Shutdown();
        AudioSystem.Instance.Shutdown();
    }
}
```

## Tips & Best Practices

### Physics

- **Use Fixed Timestep**: Update physics at a constant rate (e.g., 60 FPS) for stable simulation
- **Appropriate Masses**: Use realistic mass ratios (car = 1000kg, ball = 1kg)
- **Layer Management**: Only register entities that need physics to avoid unnecessary checks
- **Kinematic for Player**: Consider using kinematic rigidbodies for player-controlled objects

### Audio

- **Preload Audio**: Load audio clips during initialization, not during gameplay
- **Volume Mixing**: Keep total volume under 1.0 to avoid clipping
- **One-Shot for Effects**: Use `PlayOneShot` for short sound effects
- **Persistent Sources**: Use `CreateSource` for looping background music
- **Cleanup**: Always shutdown the audio system when done

## Troubleshooting

### Physics Not Working

```csharp
// Make sure to:
1. Initialize: PhysicsSystem.Instance.Initialize();
2. Register: PhysicsSystem.Instance.RegisterRigidbody(entity);
3. Register: PhysicsSystem.Instance.RegisterCollider(entity);
4. Update: Call PhysicsSystem.Instance.Update(deltaTime) each frame
```

### Audio Not Playing

```csharp
// Check:
1. System initialized: AudioSystem.Instance.IsInitialized
2. File exists: File.Exists("audio.wav")
3. Format supported: Only WAV is currently supported
4. OpenAL installed: Required on the system
```

## Next Steps

- Learn about [3D Game Development](3d-games.md)
- Explore [Scripting System](../modding/creating-mods.md)
- Check out [API Reference](../api/overview.md)
