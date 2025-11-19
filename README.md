# Evoker-Engine

[![CI/CD Pipeline](https://github.com/evokerking1/Evoker-Engine/actions/workflows/ci.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/ci.yml)
[![PR Validation](https://github.com/evokerking1/Evoker-Engine/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/pr-validation.yml)
[![Security](https://github.com/evokerking1/Evoker-Engine/actions/workflows/dependency-security.yml/badge.svg)](https://github.com/evokerking1/Evoker-Engine/actions/workflows/dependency-security.yml)

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
- **Multi-platform Support**: Windows, Linux, macOS, **iOS, and Android** support with unified API
  - Automatic platform detection
  - Platform-specific Vulkan surface extensions (Win32, XCB, Metal, Android)
  - Same API across all platforms

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
  - InventoryComponent: Entity inventory management

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

### Inventory System

- **Item System**: Base item class with ResourceKey IDs
- **ItemStack**: Handle stackable items with merge/split
- **Inventory**: Full inventory with weight limits, sorting, events
- **Item Rarity**: Common, Uncommon, Rare, Epic, Legendary, Mythic

### Block System

- **Block Base Class**: namespace:key identifiers for all blocks
- **Block Properties**: Hardness, light level, transparency, solidity
- **Block States**: Custom state management
- **Block Events**: OnPlaced, OnBroken, OnInteract, OnUpdate
- **BlockRegistry**: Central block registration and management
- **Drop System**: Configurable block drops

### Crafting System

- **Recipe Types**:
  - ShapelessRecipe: Order-independent crafting
  - ShapedRecipe: Pattern-based (3x3 grid)
  - SmeltingRecipe: Furnace/smelting recipes
  - Custom recipe types: Extend Recipe base class
- **RecipeRegistry**: Register and query recipes
- **Ingredients**: Item requirements with quantities
- **Recipe Results**: Output items with properties

### Modding System

- **Mod Base Class**: Easy mod creation with lifecycle hooks
- **ModLoader**: Automatic mod discovery and loading
- **Dependency Resolution**: Topological sort for mod dependencies
- **ModAPI**: Helper API for registering content
- **Hot Loading**: Load mods from DLL files or directories
- **Mod Events**: OnLoad, OnInitialize, OnPostInitialize, OnUpdate, OnUnload

### Dimension/World System

- **Dimension Class**: Full world/dimension support
- **Built-in Dimensions**: Overworld, Nether, End
- **Custom Dimensions**: Easy creation of new dimensions
- **Dimension Properties**:
  - Height limits, coordinate scaling, gravity
  - Sky/ceiling configuration, ambient light
  - Environment settings (water evaporation, lava behavior)
  - Fog and sky colors
- **DimensionRegistry**: Manage all dimensions
- **Coordinate Conversion**: Between dimensions with different scales

### ResourceKey System

- **Namespace:Key Format**: `namespace:key` identifiers everywhere
- **Automatic Parsing**: "modname:itemname" → ResourceKey
- **Validation**: Ensures valid format
- **Used Throughout**: Blocks, items, recipes, dimensions, mods

## Architecture

```
EvokerEngine.Core/
├── Core/              # Core engine systems
│   ├── Application.cs # Main application class
│   ├── Layer.cs       # Layer base class
│   ├── LayerStack.cs  # Layer management
│   ├── Logger.cs      # Logging system
│   ├── Time.cs        # Time management
│   └── Platform.cs    # Cross-platform utilities
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
├── Resources/         # Resource management
│   └── ResourceManager.cs # Resource loading
├── Inventory/         # Inventory system
│   ├── Item.cs        # Base item class
│   ├── ItemStack.cs   # Stackable items
│   └── Inventory.cs   # Inventory management
├── Blocks/            # Block system
│   ├── Block.cs       # Base block class
│   └── BlockRegistry.cs # Block management
├── Crafting/          # Recipe system
│   ├── Recipe.cs      # Recipe types
│   └── RecipeRegistry.cs # Recipe management
├── Modding/           # Mod support
│   ├── Mod.cs         # Base mod class
│   ├── ModLoader.cs   # Mod loading
│   └── ModAPI.cs      # Helper API
└── World/             # Dimensions
    ├── Dimension.cs   # Dimension class
    └── DimensionRegistry.cs # Dimension management

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
- **For iOS/Android builds**: .NET MAUI workloads (optional)
  ```bash
  dotnet workload install ios android
  ```

### Build Steps

#### Desktop Platforms (Windows, Linux, macOS)

```bash
dotnet build
```

#### Mobile Platforms (iOS, Android)

To build for mobile platforms with workloads installed:

```bash
dotnet build -p:BuildForMobile=true
```

Or build specific mobile targets:

```bash
# Build for Android
dotnet build -f net9.0-android

# Build for iOS
dotnet build -f net9.0-ios
```

### Run Tests

The project includes 149 comprehensive unit tests covering all core engine features.

```bash
# Run all tests (automatically generates HTML report)
dotnet test

# The HTML report is generated at: EvokerEngine.Tests/TestResults/TestResults.html

# Run tests with detailed console output
dotnet test --logger "console;verbosity=detailed"

# Run tests for a specific project
cd EvokerEngine.Tests
dotnet test
```

#### HTML Test Reports

When you run `dotnet test`, an HTML test report is **automatically generated** with:
- ✅ Beautiful, interactive test results dashboard
- 📊 Test statistics and success rate
- 🔍 Filterable test list (All/Passed/Failed)
- ⏱️ Individual test execution times
- 🐛 Detailed error messages for failed tests
- 📱 Responsive design

The report is saved to: `EvokerEngine.Tests/TestResults/TestResults.html`

Open it in your browser:
```bash
# Linux
xdg-open EvokerEngine.Tests/TestResults/TestResults.html

# macOS  
open EvokerEngine.Tests/TestResults/TestResults.html

# Windows
start EvokerEngine.Tests/TestResults/TestResults.html
```

**Alternative test scripts:**
- `./run-tests.sh` (Linux/macOS) - Runs tests with colorized output
- `run-tests.bat` (Windows) - Runs tests with HTML generation

**Test Coverage:**
- ResourceKey system (10 tests)
- Inventory system (7 tests)
- Block system (6 tests)
- Recipe/Crafting system (9 tests)
- Dimension system (11 tests)
- Modding system (10 tests)
- Logger system (2 tests)
- Time management (5 tests)
- Layer stack (4 tests)
- Event system (10 tests)
- ECS (Entity Component System) (8 tests)
- Components (Transform, Camera, MeshRenderer, Inventory) (7 tests)
- Scene management and Camera (13 tests)
- Resource management (17 tests)
- Platform detection (8 tests)
- Math utilities (22 tests - MathHelper, VectorHelper, QuaternionHelper, etc.)

**Total: 149 tests** ✅

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

### Platform Detection

```csharp
// Check current platform
if (Platform.IsMobile)
{
    Logger.Info($"Running on mobile: {Platform.PlatformName}");
}

// Platform-specific code
if (Platform.IsAndroid)
{
    // Android-specific initialization
}
else if (Platform.IsIOS)
{
    // iOS-specific initialization
}

// Get Vulkan surface extension for current platform
var surfaceExtension = Platform.GetVulkanSurfaceExtension();
// Returns: VK_KHR_win32_surface, VK_KHR_xcb_surface, VK_EXT_metal_surface, or VK_KHR_android_surface
```

## Technical Details

- **Graphics API**: Vulkan 1.2
- **Windowing**: Silk.NET.Windowing with GLFW backend
- **Input**: Silk.NET.Input
- **Math**: Silk.NET.Maths and System.Numerics
- **Platforms**: Windows, Linux, macOS, iOS, Android

## CI/CD & Testing

The project uses GitHub Actions for continuous integration and automated testing:

### Automated Workflows

1. **CI/CD Pipeline** - Runs on every push and PR
   - Multi-platform builds (Linux, Windows, macOS)
   - Runs all 66 unit tests
   - Code quality checks with coverage reporting
   - CodeQL security scanning
   - Documentation validation

2. **Pull Request Validation** - Enhanced checks for PRs
   - PR title validation
   - Merge conflict detection
   - Comprehensive test execution
   - Performance checks
   - Automatic status comments

3. **Dependency & Security** - Daily security audits
   - Checks for outdated packages
   - Scans for vulnerable dependencies
   - Security code analysis
   - Build health monitoring

### Branch Protection

The `main` branch is protected with required status checks:
- All tests must pass (66/66)
- Builds must succeed on all platforms
- Code quality checks must pass
- Security scans must complete
- PR must be up to date with base branch

See [.github/workflows/README.md](.github/workflows/README.md) for detailed workflow documentation.

## Dependencies

- Silk.NET.Vulkan (2.22.0)
- Silk.NET.Vulkan.Extensions.KHR (2.22.0)
- Silk.NET.Windowing (2.22.0)
- Silk.NET.Input (2.22.0)
- Silk.NET.Maths (2.22.0)

## License

This project is open source.

## Documentation

📖 **[Full Documentation](https://evokerking1.github.io/Evoker-Engine/)** - Complete guides and API reference

- **Getting Started** - Installation, quick start, and project structure
- **Game Development Guides** - 2D and 3D game creation tutorials
- **API Reference** - Complete API documentation with DocFX
- **Modding Guide** - Create mods and extend the engine

### Building the Documentation Locally

```bash
# Install dependencies
pip install -r requirements.txt

# Build MkDocs documentation
mkdocs serve

# Build API documentation (requires DocFX)
dotnet tool install -g docfx
docfx docfx.json --serve
```

## Future Enhancements

### In Progress / Framework Ready
- ✅ **Documentation System** - MkDocs + DocFX deployed to GitHub Pages
- ✅ **Model Loading Framework** - GLTF, GLB, OBJ support (stub implementation)
- ✅ **Texture Loading Framework** - PNG, JPG support (stub implementation)
- ✅ **Shader Compilation Framework** - SPIR-V compilation (stub implementation)
- ✅ **Physics System Framework** - Rigidbody, colliders, raycast (stub implementation)
- ✅ **Audio System Framework** - Audio clips, sources, 3D audio (stub implementation)
- ✅ **Scripting Support Framework** - Script base class and manager (stub implementation)

### Planned
- Complete Vulkan rendering pipeline implementation
- Full model loading implementation (GLTF, GLB, OBJ parsers)
- Full texture loading implementation (PNG, JPG, BMP, TGA)
- Shader compilation to SPIR-V
- Physics engine integration (Bullet, Jolt, or custom)
- Audio engine integration (OpenAL or FMOD)
- C# scripting hot reload
- Visual editor tools
