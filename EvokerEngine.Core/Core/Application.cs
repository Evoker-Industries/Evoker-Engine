using System;
using System.Numerics;
using Silk.NET.Windowing;
using Silk.NET.Input;
using EvokerEngine.Graphics;
using EvokerEngine.Scene;
using EvokerEngine.Resources;

namespace EvokerEngine.Core;

/// <summary>
/// Main application class for the Evoker Engine
/// </summary>
public class Application
{
    private static Application? _instance;
    
    private IWindow? _window;
    private VulkanContext? _vulkanContext;
    private VulkanSwapchain? _swapchain;
    private LayerStack _layerStack = new();
    private Scene.Scene _activeScene;
    private ResourceManager _resourceManager = new();
    private bool _running = false;
    private bool _minimized = false;

    public static Application Instance => _instance ?? throw new InvalidOperationException("Application not created");
    public IWindow Window => _window ?? throw new InvalidOperationException("Window not initialized");
    public VulkanContext VulkanContext => _vulkanContext ?? throw new InvalidOperationException("Vulkan context not initialized");
    public Scene.Scene ActiveScene => _activeScene;
    public ResourceManager ResourceManager => _resourceManager;

    public Application(string title = "Evoker Engine", int width = 1280, int height = 720)
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("Application already exists");
        }
        
        _instance = this;
        _activeScene = new Scene.Scene("Main Scene");

        Logger.Info("Initializing Evoker Engine...");
        
        // Create window
        var options = WindowOptions.DefaultVulkan with
        {
            Title = title,
            Size = new Silk.NET.Maths.Vector2D<int>(width, height),
            API = GraphicsAPI.DefaultVulkan
        };

        _window = Silk.NET.Windowing.Window.Create(options);
        
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;
        _window.Resize += OnResize;

        Logger.Info("Application initialized");
    }

    /// <summary>
    /// Run the application
    /// </summary>
    public void Run()
    {
        _running = true;
        _window!.Run();
    }

    /// <summary>
    /// Close the application
    /// </summary>
    public void Close()
    {
        _running = false;
        _window?.Close();
    }

    /// <summary>
    /// Push a layer to the layer stack
    /// </summary>
    public void PushLayer(Layer layer)
    {
        _layerStack.PushLayer(layer);
    }

    /// <summary>
    /// Push an overlay to the layer stack
    /// </summary>
    public void PushOverlay(Layer overlay)
    {
        _layerStack.PushOverlay(overlay);
    }

    private void OnLoad()
    {
        Logger.Info("Loading application...");

        // Initialize input
        var inputContext = _window!.CreateInput();
        Input.Initialize(inputContext);
        GamepadInput.Initialize(inputContext);

        // Setup input callbacks
        foreach (var keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp += OnKeyUp;
        }

        foreach (var mouse in inputContext.Mice)
        {
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseScroll;
        }

        // Setup gamepad callbacks
        foreach (var gamepad in inputContext.Gamepads)
        {
            gamepad.ButtonDown += OnGamepadButtonDown;
            gamepad.ButtonUp += OnGamepadButtonUp;
        }

        // Initialize Vulkan
        _vulkanContext = new VulkanContext();
        _vulkanContext.Initialize(_window);

        // Create swapchain
        _swapchain = new VulkanSwapchain(_vulkanContext);
        _swapchain.Create((uint)_window.Size.X, (uint)_window.Size.Y);

        // Reset time
        Time.Reset();

        Logger.Info("Application loaded");
    }

    private void OnUpdate(double deltaTime)
    {
        if (_minimized)
            return;

        Time.Update();

        // Update layers
        foreach (var layer in _layerStack.GetLayers())
        {
            layer.OnUpdate(Time.DeltaTime);
        }

        // Update active scene
        _activeScene.Update(Time.DeltaTime);
    }

    private void OnRender(double deltaTime)
    {
        if (_minimized)
            return;

        // Render layers
        foreach (var layer in _layerStack.GetLayers())
        {
            layer.OnRender();
        }
    }

    private void OnClose()
    {
        Logger.Info("Closing application...");

        // Cleanup swapchain
        _swapchain?.Cleanup();

        // Cleanup Vulkan context
        _vulkanContext?.Cleanup();

        // Cleanup resources
        _resourceManager.UnloadAll();

        // Detach layers
        foreach (var layer in _layerStack.GetLayers())
        {
            layer.OnDetach();
        }

        Logger.Info("Application closed");
    }

    private void OnResize(Silk.NET.Maths.Vector2D<int> size)
    {
        _minimized = size.X == 0 || size.Y == 0;
        
        if (!_minimized)
        {
            var evt = new WindowResizeEvent(size.X, size.Y);
            DispatchEvent(evt);
        }
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        var evt = new KeyPressedEvent(key, keyboard.IsKeyPressed(key));
        DispatchEvent(evt);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
    {
        var evt = new KeyReleasedEvent(key);
        DispatchEvent(evt);
    }

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        var evt = new MouseButtonPressedEvent(button);
        DispatchEvent(evt);
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        var evt = new MouseButtonReleasedEvent(button);
        DispatchEvent(evt);
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        var evt = new MouseMovedEvent(position.X, position.Y);
        DispatchEvent(evt);
    }

    private void OnMouseScroll(IMouse mouse, Silk.NET.Input.ScrollWheel scrollWheel)
    {
        var evt = new MouseScrolledEvent(scrollWheel.X, scrollWheel.Y);
        DispatchEvent(evt);
    }

    private void OnGamepadButtonDown(IGamepad gamepad, Silk.NET.Input.Button button)
    {
        var evt = new GamepadButtonPressedEvent(button.Name, gamepad.Index);
        DispatchEvent(evt);
    }

    private void OnGamepadButtonUp(IGamepad gamepad, Silk.NET.Input.Button button)
    {
        var evt = new GamepadButtonReleasedEvent(button.Name, gamepad.Index);
        DispatchEvent(evt);
    }

    private void DispatchEvent(Event evt)
    {
        // Dispatch to layers in reverse order
        var layers = new System.Collections.Generic.List<Layer>(_layerStack.GetLayers());
        layers.Reverse();
        
        foreach (var layer in layers)
        {
            if (evt.Handled)
                break;
            
            layer.OnEvent(evt);
        }
    }
}
