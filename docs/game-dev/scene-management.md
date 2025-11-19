# Scene Management

Scene Management in Evoker Engine provides a way to organize entities, manage multiple scenes, and work with cameras for rendering.

## Overview

The scene system includes:

- **Scene**: Container for entities and their components
- **Entity Management**: Creating and destroying entities within scenes
- **Scene Switching**: Loading and unloading different scenes
- **Camera System**: Managing active cameras for rendering

## Scene Class

```csharp
public class Scene
{
    public string Name { get; set; }
    public ECSRegistry Registry { get; }
    
    public Entity CreateEntity(string name = "Entity");
    public void DestroyEntity(Entity entity);
    public void Update(float deltaTime);
    public IEnumerable<Entity> GetRootEntities();
    
    public static Scene? GetActiveScene();
    public static void SetActiveScene(Scene scene);
}
```

## Creating Scenes

### Basic Scene

```csharp
using EvokerEngine.Scene;
using EvokerEngine.Core;

// Create a new scene
var scene = new Scene("Level 1");

// Create entities in the scene
var player = scene.CreateEntity("Player");
var enemy = scene.CreateEntity("Enemy");

// Add components
var transform = scene.Registry.AddComponent<TransformComponent>(player);
transform.Position = new Vector3(0, 0, 0);
```

### Accessing Active Scene

```csharp
// Get active scene from Application
var scene = Application.Instance.ActiveScene;

// Or use static method
var scene = Scene.GetActiveScene();
```

## Entity Management

### Creating Entities

```csharp
var scene = Application.Instance.ActiveScene;

// Create entity with name
var player = scene.CreateEntity("Player");

// Add transform
var transform = scene.Registry.AddComponent<TransformComponent>(player);
transform.Position = new Vector3(0, 1, 0);
transform.Scale = Vector3.One;

// Add renderer
var renderer = scene.Registry.AddComponent<MeshRendererComponent>(player);
renderer.MeshId = 0;
renderer.MaterialId = 0;
```

### Destroying Entities

```csharp
// Destroy specific entity
scene.DestroyEntity(player);

// Destroy all enemies
var allTransforms = scene.Registry.GetAllComponents<TransformComponent>();
foreach (var transform in allTransforms)
{
    // Check if enemy (example logic)
    if (transform.Position.Y < -10)
    {
        scene.DestroyEntity(transform.Entity);
    }
}
```

### Finding Entities

```csharp
// Get all root entities
var entities = scene.GetRootEntities();
foreach (var entity in entities)
{
    Logger.Info($"Entity ID: {entity.Id}");
}

// Find entity by component
var healthComponents = scene.Registry.GetAllComponents<HealthComponent>();
foreach (var health in healthComponents)
{
    if (health.MaxHealth == 100)  // Player identification
    {
        var playerEntity = health.Entity;
        break;
    }
}
```

## Camera System

### Creating a Camera

```csharp
// 3D Camera
var cameraEntity = scene.CreateEntity("MainCamera");
var cameraTransform = scene.Registry.AddComponent<TransformComponent>(cameraEntity);
cameraTransform.Position = new Vector3(0, 5, 10);
cameraTransform.Rotation = new Vector3(-30, 0, 0);

var camera3D = scene.Registry.AddComponent<Camera3DComponent>(cameraEntity);
camera3D.IsPrimary = true;
camera3D.FieldOfView = 60f;
camera3D.AspectRatio = 16f / 9f;
camera3D.NearPlane = 0.1f;
camera3D.FarPlane = 1000f;
```

### 2D Camera

```csharp
var cameraEntity = scene.CreateEntity("2DCamera");
var camera2D = scene.Registry.AddComponent<Camera2DComponent>(cameraEntity);
camera2D.IsPrimary = true;
camera2D.ViewportSize = new Vector2(1280, 720);
camera2D.Zoom = 1f;
camera2D.Position = Vector2.Zero;
```

### Camera Controls

```csharp
public class CameraController : Layer
{
    private Entity _camera;
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    public override void OnUpdate(float deltaTime)
    {
        var scene = Application.Instance.ActiveScene;
        var transform = scene.Registry.GetComponent<TransformComponent>(_camera);
        
        // WASD movement
        var moveSpeed = 5f * deltaTime;
        if (Input.IsKeyPressed(Key.W))
            transform.Position += GetForward(transform) * moveSpeed;
        if (Input.IsKeyPressed(Key.S))
            transform.Position -= GetForward(transform) * moveSpeed;
        if (Input.IsKeyPressed(Key.A))
            transform.Position -= GetRight(transform) * moveSpeed;
        if (Input.IsKeyPressed(Key.D))
            transform.Position += GetRight(transform) * moveSpeed;
        
        // Mouse look
        var mouseDelta = Input.GetMouseDelta();
        _rotationY += mouseDelta.X * 0.1f;
        _rotationX -= mouseDelta.Y * 0.1f;
        _rotationX = Math.Clamp(_rotationX, -89f, 89f);
        
        transform.Rotation = new Vector3(_rotationX, _rotationY, 0);
    }

    private Vector3 GetForward(TransformComponent transform)
    {
        var yaw = transform.Rotation.Y * MathHelper.Deg2Rad;
        return new Vector3(MathF.Sin(yaw), 0, MathF.Cos(yaw));
    }

    private Vector3 GetRight(TransformComponent transform)
    {
        return Vector3.Cross(GetForward(transform), Vector3.UnitY);
    }
}
```

## Scene Switching

### Basic Scene Switching

```csharp
public class SceneManager : Layer
{
    private Dictionary<string, Scene> _scenes = new();
    private Scene? _currentScene;

    public void RegisterScene(string name, Scene scene)
    {
        _scenes[name] = scene;
    }

    public void LoadScene(string name)
    {
        if (_scenes.TryGetValue(name, out var scene))
        {
            // Cleanup current scene
            if (_currentScene != null)
            {
                CleanupScene(_currentScene);
            }
            
            // Set new scene
            _currentScene = scene;
            Scene.SetActiveScene(scene);
            
            // Initialize new scene
            InitializeScene(scene);
            
            Logger.Info($"Loaded scene: {name}");
        }
    }

    private void CleanupScene(Scene scene)
    {
        // Destroy all entities
        var entities = scene.GetRootEntities().ToList();
        foreach (var entity in entities)
        {
            scene.DestroyEntity(entity);
        }
    }

    private void InitializeScene(Scene scene)
    {
        // Setup scene-specific entities
    }
}
```

### Scene with Data Persistence

```csharp
public class PersistentSceneManager
{
    private Scene _currentScene;
    private Dictionary<string, object> _persistentData = new();

    public void SaveSceneData(string key, object data)
    {
        _persistentData[key] = data;
    }

    public T? GetSceneData<T>(string key)
    {
        if (_persistentData.TryGetValue(key, out var data))
        {
            return (T)data;
        }
        return default;
    }

    public void LoadSceneWithData(Scene scene)
    {
        _currentScene = scene;
        Scene.SetActiveScene(scene);
        
        // Restore persistent data
        var playerPos = GetSceneData<Vector3>("player_position");
        if (playerPos != default)
        {
            // Set player position
        }
    }
}
```

## Complete Examples

### Example 1: Multi-Scene Game

```csharp
public class GameSceneManager : Layer
{
    private Scene _menuScene;
    private Scene _gameScene;
    private Scene _currentScene;

    public override void OnAttach()
    {
        // Create menu scene
        _menuScene = CreateMenuScene();
        
        // Create game scene
        _gameScene = CreateGameScene();
        
        // Start with menu
        LoadScene(_menuScene);
    }

    private Scene CreateMenuScene()
    {
        var scene = new Scene("Main Menu");
        
        // Create UI elements
        var titleEntity = scene.CreateEntity("Title");
        // Add UI components...
        
        return scene;
    }

    private Scene CreateGameScene()
    {
        var scene = new Scene("Game Level");
        
        // Create player
        var player = scene.CreateEntity("Player");
        var playerTransform = scene.Registry.AddComponent<TransformComponent>(player);
        playerTransform.Position = new Vector3(0, 1, 0);
        
        var health = scene.Registry.AddComponent<HealthComponent>(player);
        health.MaxHealth = 100f;
        health.CurrentHealth = 100f;
        
        // Create camera
        var camera = scene.CreateEntity("Camera");
        var cameraTransform = scene.Registry.AddComponent<TransformComponent>(camera);
        cameraTransform.Position = new Vector3(0, 5, 10);
        
        var cam3D = scene.Registry.AddComponent<Camera3DComponent>(camera);
        cam3D.IsPrimary = true;
        
        // Create enemies
        for (int i = 0; i < 5; i++)
        {
            var enemy = scene.CreateEntity($"Enemy_{i}");
            var enemyTransform = scene.Registry.AddComponent<TransformComponent>(enemy);
            enemyTransform.Position = new Vector3(i * 3, 0, 5);
        }
        
        return scene;
    }

    private void LoadScene(Scene scene)
    {
        _currentScene = scene;
        Scene.SetActiveScene(scene);
        Logger.Info($"Loaded scene: {scene.Name}");
    }

    public void StartGame()
    {
        LoadScene(_gameScene);
    }

    public void ReturnToMenu()
    {
        LoadScene(_menuScene);
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Escape)
            {
                if (_currentScene == _gameScene)
                {
                    ReturnToMenu();
                }
                else if (_currentScene == _menuScene)
                {
                    Application.Instance.Close();
                }
            }
            
            if (keyEvent.KeyCode == Key.Enter && _currentScene == _menuScene)
            {
                StartGame();
            }
        }
    }
}
```

### Example 2: Level System

```csharp
public class LevelManager : Layer
{
    private int _currentLevel = 1;
    private Scene _currentScene;

    public void LoadLevel(int levelNumber)
    {
        _currentLevel = levelNumber;
        
        // Create new scene for level
        _currentScene = new Scene($"Level {levelNumber}");
        Scene.SetActiveScene(_currentScene);
        
        // Load level data
        LoadLevelData(levelNumber);
        
        // Create player
        CreatePlayer();
        
        // Create level geometry
        CreateLevelGeometry(levelNumber);
        
        Logger.Info($"Loaded Level {levelNumber}");
    }

    private void CreatePlayer()
    {
        var player = _currentScene.CreateEntity("Player");
        var transform = _currentScene.Registry.AddComponent<TransformComponent>(player);
        transform.Position = GetSpawnPoint(_currentLevel);
        
        var health = _currentScene.Registry.AddComponent<HealthComponent>(player);
        health.MaxHealth = 100f;
        health.CurrentHealth = 100f;
    }

    private void CreateLevelGeometry(int level)
    {
        // Level-specific geometry
        switch (level)
        {
            case 1:
                CreateLevel1();
                break;
            case 2:
                CreateLevel2();
                break;
            default:
                CreateDefaultLevel();
                break;
        }
    }

    private Vector3 GetSpawnPoint(int level)
    {
        return level switch
        {
            1 => new Vector3(0, 1, 0),
            2 => new Vector3(5, 1, 5),
            _ => Vector3.Zero
        };
    }

    public void NextLevel()
    {
        LoadLevel(_currentLevel + 1);
    }

    private void LoadLevelData(int levelNumber) { }
    private void CreateLevel1() { }
    private void CreateLevel2() { }
    private void CreateDefaultLevel() { }
}
```

### Example 3: Camera Follow System

```csharp
public class CameraFollowSystem : Layer
{
    private Entity _camera;
    private Entity _target;
    private Vector3 _offset = new Vector3(0, 5, 10);
    private float _smoothSpeed = 5f;

    public override void OnAttach()
    {
        var scene = Application.Instance.ActiveScene;
        
        // Find or create camera
        _camera = scene.CreateEntity("FollowCamera");
        var cameraTransform = scene.Registry.AddComponent<TransformComponent>(_camera);
        
        var cam3D = scene.Registry.AddComponent<Camera3DComponent>(_camera);
        cam3D.IsPrimary = true;
        
        // Find target (player)
        var healthComponents = scene.Registry.GetAllComponents<HealthComponent>();
        foreach (var health in healthComponents)
        {
            if (health.MaxHealth == 100f)  // Identify player
            {
                _target = health.Entity;
                break;
            }
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_target.Id == 0 || _camera.Id == 0)
            return;
        
        var scene = Application.Instance.ActiveScene;
        var targetTransform = scene.Registry.GetComponent<TransformComponent>(_target);
        var cameraTransform = scene.Registry.GetComponent<TransformComponent>(_camera);
        
        if (targetTransform != null && cameraTransform != null)
        {
            // Calculate desired position
            var desiredPos = targetTransform.Position + _offset;
            
            // Smooth follow
            cameraTransform.Position = Vector3.Lerp(
                cameraTransform.Position,
                desiredPos,
                _smoothSpeed * deltaTime
            );
            
            // Look at target
            var direction = targetTransform.Position - cameraTransform.Position;
            var yaw = MathF.Atan2(direction.X, direction.Z) * MathHelper.Rad2Deg;
            var pitch = MathF.Asin(-direction.Y / direction.Length()) * MathHelper.Rad2Deg;
            
            cameraTransform.Rotation = new Vector3(pitch, yaw, 0);
        }
    }
}
```

## Best Practices

### ✅ Do's

- Create scenes for different game states (menu, gameplay, pause)
- Use scene switching for level transitions
- Set camera as primary (IsPrimary = true)
- Clean up entities when switching scenes
- Use meaningful names for scenes and entities

### ❌ Don'ts

- Don't forget to set an active scene
- Don't create multiple primary cameras
- Don't leave destroyed entities in memory
- Don't perform heavy operations in scene Update()

## Performance Tips

1. **Reuse scenes** - Don't create new scenes every frame
2. **Pool entities** - Reuse instead of creating/destroying
3. **Limit root entities** - Use hierarchies for organization
4. **Batch scene loading** - Load assets asynchronously

## See Also

- [ECS](../core/ecs.md) - Entity Component System
- [Application](../core/application.md) - Application and active scene
