# Evoker-Engine

A complete C# game engine with Vulkan rendering support using Silk.NET.

## Features

### Core Engine Features

- **Application Framework**: Window management, event system, and main loop
- **Layer System**: Modular layer-based architecture for organizing game logic
- **Event System**: Comprehensive event handling for input and window events
- **Time Management**: Delta time, time scaling, and FPS tracking
- **Logging System**: Multi-level logging with color-coded output

### Graphics System

- **Vulkan Rendering**: Full Vulkan integration using Silk.NET.Vulkan
- **Vulkan Context**: Device initialization, queue management, and surface creation
- **Swapchain Management**: Automatic swapchain creation and management
- **Multi-platform Support**: Windows, Linux, and macOS support

### Input System

- **Keyboard Input**: Key press and release events with polling support
- **Mouse Input**: Mouse movement, button clicks, and scroll wheel support
- **Controller/Gamepad Support**: 
  - Full gamepad support with button and axis input
  - Thumbstick (left/right stick) input with deadzone handling
  - Trigger input (left/right triggers)
  - D-pad support
  - Vibration/rumble support
  - Hot-plug support (connect/disconnect detection)
  - Multiple controller support
- **Input Polling**: Query input state at any time

### Entity Component System (ECS)

- **Entity Management**: Create and destroy entities with unique IDs
- **Component System**: Add, remove, and query components on entities
- **Built-in Components**:
  - TransformComponent: Position, rotation, and scale
  - MeshRendererComponent: Mesh and material assignment
  - CameraComponent: Camera properties and projection matrices

### Scene Management

- **Scene System**: Organize entities into scenes
- **Camera System**: 3D camera with view and projection matrices
- **Camera Controls**: Movement and look-at functionality

### Resource Management

- **Resource Manager**: Load and manage game resources
- **Resource Types**:
  - Textures: Image data management
  - Shaders: Vertex and fragment shader programs
  - Meshes: Vertex and index data

## Architecture

```
EvokerEngine.Core/
├── Core/              # Core engine systems
│   ├── Application.cs # Main application class
│   ├── Layer.cs       # Layer base class
│   ├── LayerStack.cs  # Layer management
│   ├── Logger.cs      # Logging system
│   └── Time.cs        # Time management
├── Graphics/          # Rendering system
│   ├── VulkanContext.cs   # Vulkan initialization
│   └── VulkanSwapchain.cs # Swapchain management
├── Input/             # Input system
│   ├── Input.cs       # Keyboard/Mouse input polling
│   └── GamepadInput.cs # Controller/Gamepad input
├── Events/            # Event system
│   └── Event.cs       # Event definitions
├── ECS/               # Entity Component System
│   ├── Entity.cs      # Entity definition
│   ├── Component.cs   # Component base class
│   ├── Components.cs  # Built-in components
│   └── ECSRegistry.cs # ECS management
├── Scene/             # Scene management
│   ├── Scene.cs       # Scene definition
│   └── Camera.cs      # Camera system
└── Resources/         # Resource management
    └── ResourceManager.cs # Resource loading

EvokerEngine.Tests/    # Unit tests
├── LoggerTests.cs
├── TimeTests.cs
├── LayerStackTests.cs
├── EventTests.cs
├── ECSTests.cs
├── ComponentTests.cs
├── SceneTests.cs
└── ResourceManagerTests.cs
```

## Building

### Prerequisites

- .NET 9.0 SDK or later
- Vulkan SDK (runtime only)

### Build Steps

```bash
dotnet build
```

### Run Tests

The project includes comprehensive unit tests covering all core engine features.

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests for a specific project
cd EvokerEngine.Tests
dotnet test
```

**Test Coverage:**
- Logger system (2 tests)
- Time management (5 tests)
- Layer stack (4 tests)
- Event system (10 tests)
- ECS (Entity Component System) (8 tests)
- Components (Transform, Camera, MeshRenderer) (7 tests)
- Scene management and Camera (13 tests)
- Resource management (17 tests)

**Total: 66 tests** ✅

### Run the Demo

```bash
cd EvokerEngine.Demo
dotnet run
```

## Usage Example

```csharp
using EvokerEngine.Core;
using Silk.NET.Input;

// Create application
var app = new Application("My Game", 1280, 720);

// Create and add a custom layer
app.PushLayer(new MyGameLayer());

// Run the application
app.Run();

// Custom layer implementation
class MyGameLayer : Layer
{
    public MyGameLayer() : base("Game Layer") { }

    public override void OnAttach()
    {
        Logger.Info("Game layer initialized");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Update game logic
    }

    public override void OnRender()
    {
        // Render game objects
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            if (keyEvent.KeyCode == Key.Escape)
            {
                Application.Instance.Close();
            }
        }
        else if (e is GamepadButtonPressedEvent gamepadEvent)
        {
            if (gamepadEvent.Button == ButtonName.Start)
            {
                Logger.Info("Pause menu");
            }
        }
    }
}
```

## Demo Controls

### Keyboard
- **ESC** - Exit application
- **F1** - Toggle VSync (logged only)
- **WASD** - Camera movement (logged only)

### Gamepad/Controller
- **A/Cross** - Action button
- **B/Circle** - Back button
- **X/Square** - X button
- **Y/Triangle** - Y button
- **Start** - Pause
- **Back/Select** - Back
- **Left/Right Bumpers** - Bumper buttons
- **Left Stick** - Movement (logged)
- **Right Stick** - Camera (logged)
- **Left/Right Triggers** - Trigger input (logged)
- **D-Pad** - Directional input (logged)

## API Overview

### Creating Entities

```csharp
var scene = Application.Instance.ActiveScene;
var entity = scene.CreateEntity("My Entity");

// Add components
var transform = scene.Registry.AddComponent<TransformComponent>(entity);
transform.Position = new Vector3(0, 0, 0);

var renderer = scene.Registry.AddComponent<MeshRendererComponent>(entity);
renderer.MeshId = 0;
renderer.MaterialId = 0;
```

### Input Handling

```csharp
// Keyboard input
if (Input.IsKeyPressed(Key.W))
{
    // Move forward
}

var mousePos = Input.GetMousePosition();

// Gamepad input
if (GamepadInput.IsGamepadConnected(0))
{
    // Check buttons
    if (GamepadInput.IsButtonPressed(ButtonName.A))
    {
        // A button pressed
    }

    // Get stick input
    var leftStick = GamepadInput.GetLeftStick();
    var rightStick = GamepadInput.GetRightStick();

    // Get trigger input
    var leftTrigger = GamepadInput.GetLeftTrigger();
    var rightTrigger = GamepadInput.GetRightTrigger();

    // Set vibration
    GamepadInput.SetVibration(0.5f, 0.5f); // 50% on both motors
    GamepadInput.StopVibration(); // Stop vibration
}

var mousePos = Input.GetMousePosition();
```

### Resource Management

```csharp
var resourceManager = Application.Instance.ResourceManager;

// Load resources
var texture = resourceManager.Load<Texture>("MyTexture");
var shader = resourceManager.Load<Shader>("MyShader");
var mesh = resourceManager.Load<Mesh>("MyMesh");
```

## Technical Details

- **Graphics API**: Vulkan 1.2
- **Windowing**: Silk.NET.Windowing with GLFW backend
- **Input**: Silk.NET.Input
- **Math**: Silk.NET.Maths and System.Numerics

## Dependencies

- Silk.NET.Vulkan (2.22.0)
- Silk.NET.Vulkan.Extensions.KHR (2.22.0)
- Silk.NET.Windowing (2.22.0)
- Silk.NET.Input (2.22.0)
- Silk.NET.Maths (2.22.0)

## License

This project is open source.

## Future Enhancements

- Complete Vulkan rendering pipeline implementation
- Model loading (GLTF, OBJ)
- Texture loading (PNG, JPG)
- Shader compilation
- Physics system integration
- Audio system
- Scripting support
- Editor tools
