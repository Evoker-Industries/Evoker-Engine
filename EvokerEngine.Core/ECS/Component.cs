using System;

namespace EvokerEngine.ECS;

/// <summary>
/// Base class for all components
/// </summary>
public abstract class Component
{
    public Entity Entity { get; internal set; }
}
