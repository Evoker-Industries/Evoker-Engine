using System;

namespace EvokerEngine.ECS;

/// <summary>
/// Entity identifier
/// </summary>
public struct Entity : IEquatable<Entity>
{
    public readonly uint Id;

    public Entity(uint id)
    {
        Id = id;
    }

    public bool Equals(Entity other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"Entity({Id})";

    public static bool operator ==(Entity left, Entity right) => left.Equals(right);
    public static bool operator !=(Entity left, Entity right) => !left.Equals(right);
}
