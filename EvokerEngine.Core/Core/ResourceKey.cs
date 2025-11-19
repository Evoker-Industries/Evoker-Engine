using System;
using System.Text.RegularExpressions;

namespace EvokerEngine.Core;

/// <summary>
/// Represents a namespaced identifier in the format "namespace:key"
/// Used throughout the engine for identifying resources, blocks, items, etc.
/// </summary>
public readonly struct ResourceKey : IEquatable<ResourceKey>
{
    private static readonly Regex ValidationRegex = new(@"^[a-z0-9_.-]+:[a-z0-9_./\-]+$", RegexOptions.Compiled);
    private const string DefaultNamespace = "evoker";

    /// <summary>
    /// The namespace part (e.g., "minecraft" in "minecraft:stone")
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// The key part (e.g., "stone" in "minecraft:stone")
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Full string representation "namespace:key"
    /// </summary>
    public string FullKey => $"{Namespace}:{Key}";

    /// <summary>
    /// Create a resource key from namespace and key
    /// </summary>
    public ResourceKey(string namespacePart, string key)
    {
        if (string.IsNullOrWhiteSpace(namespacePart))
            throw new ArgumentException("Namespace cannot be empty", nameof(namespacePart));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        Namespace = namespacePart.ToLowerInvariant();
        Key = key.ToLowerInvariant();

        if (!IsValid(FullKey))
            throw new ArgumentException($"Invalid resource key format: {FullKey}. Must match pattern 'namespace:key' with lowercase letters, numbers, and _.-/ characters");
    }

    /// <summary>
    /// Create a resource key with default namespace
    /// </summary>
    public ResourceKey(string key) : this(DefaultNamespace, key)
    {
    }

    /// <summary>
    /// Parse a resource key from string "namespace:key"
    /// </summary>
    public static ResourceKey Parse(string fullKey)
    {
        if (string.IsNullOrWhiteSpace(fullKey))
            throw new ArgumentException("Resource key cannot be empty", nameof(fullKey));

        var parts = fullKey.Split(':', 2);
        
        if (parts.Length == 1)
        {
            // No namespace specified, use default
            return new ResourceKey(DefaultNamespace, parts[0]);
        }

        return new ResourceKey(parts[0], parts[1]);
    }

    /// <summary>
    /// Try to parse a resource key from string
    /// </summary>
    public static bool TryParse(string fullKey, out ResourceKey result)
    {
        try
        {
            result = Parse(fullKey);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Check if a string is a valid resource key
    /// </summary>
    public static bool IsValid(string fullKey)
    {
        if (string.IsNullOrWhiteSpace(fullKey))
            return false;

        return ValidationRegex.IsMatch(fullKey.ToLowerInvariant());
    }

    /// <summary>
    /// Check if this key belongs to a specific namespace
    /// </summary>
    public bool IsInNamespace(string namespaceName)
    {
        return Namespace.Equals(namespaceName, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(ResourceKey other)
    {
        return Namespace == other.Namespace && Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        return obj is ResourceKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Namespace, Key);
    }

    public override string ToString()
    {
        return FullKey;
    }

    public static bool operator ==(ResourceKey left, ResourceKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceKey left, ResourceKey right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Implicit conversion from string to ResourceKey
    /// </summary>
    public static implicit operator ResourceKey(string fullKey)
    {
        return Parse(fullKey);
    }

    /// <summary>
    /// Implicit conversion from ResourceKey to string
    /// </summary>
    public static implicit operator string(ResourceKey key)
    {
        return key.FullKey;
    }
}
