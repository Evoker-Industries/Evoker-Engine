using System;

namespace EvokerEngine.Core;

/// <summary>
/// Base class for all layers in the engine
/// </summary>
public abstract class Layer
{
    public string Name { get; protected set; }

    protected Layer(string name = "Layer")
    {
        Name = name;
    }

    /// <summary>
    /// Called when the layer is attached to the application
    /// </summary>
    public virtual void OnAttach() { }

    /// <summary>
    /// Called when the layer is detached from the application
    /// </summary>
    public virtual void OnDetach() { }

    /// <summary>
    /// Called every frame to update the layer
    /// </summary>
    /// <param name="deltaTime">Time since last frame in seconds</param>
    public virtual void OnUpdate(float deltaTime) { }

    /// <summary>
    /// Called every frame to render the layer
    /// </summary>
    public virtual void OnRender() { }

    /// <summary>
    /// Called when an event occurs
    /// </summary>
    public virtual void OnEvent(Event e) { }
}
