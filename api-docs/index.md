# Evoker Engine API Documentation

Welcome to the Evoker Engine API reference documentation.

## Overview

This documentation provides detailed API reference for all public types, methods, and properties in Evoker Engine.

## Namespaces

### Core Engine

- **EvokerEngine.Core** - Application framework, layers, logging, time management
- **EvokerEngine.Events** - Event system for input and window events
- **EvokerEngine.Input** - Keyboard, mouse, and gamepad input handling

### Graphics & Rendering

- **EvokerEngine.Graphics** - Vulkan rendering context and swapchain management
- **EvokerEngine.Rendering** - Rendering components and systems

### Entity Component System

- **EvokerEngine.ECS** - Entity management and component system
- **EvokerEngine.Scene** - Scene management and camera system

### Game Systems

- **EvokerEngine.Inventory** - Item and inventory management
- **EvokerEngine.Blocks** - Block system for voxel-based games
- **EvokerEngine.Crafting** - Recipe and crafting system
- **EvokerEngine.World** - Dimension and world management

### Modding

- **EvokerEngine.Modding** - Mod creation and loading system

### Resources & Utilities

- **EvokerEngine.Resources** - Resource management and loading
- **EvokerEngine.Utilities** - Math helpers, random utilities

## Getting Started

For tutorials and guides, see the [main documentation](https://evokerking1.github.io/Evoker-Engine/).

## Common Classes

### Application

The main application class that manages the window, rendering, and game loop.

```csharp
var app = new Application("My Game", 1280, 720);
app.PushLayer(new GameLayer());
app.Run();
```

### Layer

Base class for organizing game logic into layers.

```csharp
public class GameLayer : Layer
{
    public override void OnAttach() { }
    public override void OnUpdate(float deltaTime) { }
    public override void OnRender() { }
    public override void OnEvent(Event e) { }
}
```

### Entity & Components

Entity Component System for game objects.

```csharp
var scene = Application.Instance.ActiveScene;
var entity = scene.CreateEntity("Player");
var transform = scene.Registry.AddComponent<TransformComponent>(entity);
```

### Input

Input handling for keyboard, mouse, and gamepad.

```csharp
if (Input.IsKeyPressed(Key.W))
    MoveForward();

var mousePos = Input.GetMousePosition();
```

## Browse API

Use the navigation on the left to browse the complete API reference.
