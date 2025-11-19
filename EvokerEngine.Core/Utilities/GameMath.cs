using System;
using System.Numerics;

namespace EvokerEngine.Utilities;

/// <summary>
/// Math utilities for game development
/// </summary>
public static class MathHelper
{
    public const float PI = (float)Math.PI;
    public const float TwoPI = PI * 2f;
    public const float HalfPI = PI / 2f;
    public const float Deg2Rad = PI / 180f;
    public const float Rad2Deg = 180f / PI;

    /// <summary>
    /// Clamp a value between min and max
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// Clamp a value between 0 and 1
    /// </summary>
    public static float Clamp01(float value)
    {
        return Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Linear interpolation
    /// </summary>
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Clamp01(t);
    }

    /// <summary>
    /// Inverse linear interpolation
    /// </summary>
    public static float InverseLerp(float a, float b, float value)
    {
        return (value - a) / (b - a);
    }

    /// <summary>
    /// Smooth interpolation (ease in/out)
    /// </summary>
    public static float SmoothStep(float a, float b, float t)
    {
        t = Clamp01(t);
        t = t * t * (3f - 2f * t);
        return Lerp(a, b, t);
    }

    /// <summary>
    /// Move towards a target value
    /// </summary>
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;
        return current + Math.Sign(target - current) * maxDelta;
    }

    /// <summary>
    /// Repeat a value within a range
    /// </summary>
    public static float Repeat(float value, float length)
    {
        return value - (float)Math.Floor(value / length) * length;
    }

    /// <summary>
    /// Ping pong a value between 0 and length
    /// </summary>
    public static float PingPong(float value, float length)
    {
        value = Repeat(value, length * 2f);
        return length - Math.Abs(value - length);
    }

    /// <summary>
    /// Check if two floats are approximately equal
    /// </summary>
    public static bool Approximately(float a, float b, float epsilon = 0.0001f)
    {
        return Math.Abs(a - b) < epsilon;
    }
}

/// <summary>
/// Vector math utilities
/// </summary>
public static class VectorHelper
{
    /// <summary>
    /// Linear interpolation between two vectors
    /// </summary>
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return Vector2.Lerp(a, b, MathHelper.Clamp01(t));
    }

    /// <summary>
    /// Linear interpolation between two vectors
    /// </summary>
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return Vector3.Lerp(a, b, MathHelper.Clamp01(t));
    }

    /// <summary>
    /// Move towards a target vector
    /// </summary>
    public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        var direction = target - current;
        var distance = direction.Length();
        
        if (distance <= maxDistanceDelta || distance == 0f)
            return target;
        
        return current + direction / distance * maxDistanceDelta;
    }

    /// <summary>
    /// Rotate a vector around an axis
    /// </summary>
    public static Vector3 RotateAround(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
    {
        var rotation = Quaternion.CreateFromAxisAngle(axis, angle * MathHelper.Deg2Rad);
        var direction = point - pivot;
        direction = Vector3.Transform(direction, rotation);
        return pivot + direction;
    }

    /// <summary>
    /// Get direction from one point to another
    /// </summary>
    public static Vector3 Direction(Vector3 from, Vector3 to)
    {
        var direction = to - from;
        return Vector3.Normalize(direction);
    }

    /// <summary>
    /// Project a vector onto another
    /// </summary>
    public static Vector3 Project(Vector3 vector, Vector3 onNormal)
    {
        var dot = Vector3.Dot(onNormal, onNormal);
        if (dot < float.Epsilon)
            return Vector3.Zero;
        
        return onNormal * Vector3.Dot(vector, onNormal) / dot;
    }

    /// <summary>
    /// Reflect a vector off a surface
    /// </summary>
    public static Vector3 Reflect(Vector3 direction, Vector3 normal)
    {
        return Vector3.Reflect(direction, normal);
    }
}

/// <summary>
/// Transform utilities
/// </summary>
public static class TransformHelper
{
    /// <summary>
    /// Create a look-at matrix
    /// </summary>
    public static Matrix4x4 LookAt(Vector3 position, Vector3 target, Vector3 up)
    {
        return Matrix4x4.CreateLookAt(position, target, up);
    }

    /// <summary>
    /// Create TRS (Translation, Rotation, Scale) matrix
    /// </summary>
    public static Matrix4x4 CreateTRS(Vector3 translation, Quaternion rotation, Vector3 scale)
    {
        return Matrix4x4.CreateScale(scale) *
               Matrix4x4.CreateFromQuaternion(rotation) *
               Matrix4x4.CreateTranslation(translation);
    }

    /// <summary>
    /// Create TRS matrix from Euler angles
    /// </summary>
    public static Matrix4x4 CreateTRS(Vector3 translation, Vector3 eulerAngles, Vector3 scale)
    {
        var rotation = QuaternionHelper.FromEuler(eulerAngles);
        return CreateTRS(translation, rotation, scale);
    }

    /// <summary>
    /// Decompose a matrix into TRS components
    /// </summary>
    public static bool Decompose(Matrix4x4 matrix, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
    {
        return Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);
    }
}

/// <summary>
/// Quaternion utilities
/// </summary>
public static class QuaternionHelper
{
    /// <summary>
    /// Create quaternion from Euler angles (in degrees)
    /// </summary>
    public static Quaternion FromEuler(Vector3 eulerAngles)
    {
        return FromEuler(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
    }

    /// <summary>
    /// Create quaternion from Euler angles (in degrees)
    /// </summary>
    public static Quaternion FromEuler(float x, float y, float z)
    {
        x *= MathHelper.Deg2Rad;
        y *= MathHelper.Deg2Rad;
        z *= MathHelper.Deg2Rad;
        
        return Quaternion.CreateFromYawPitchRoll(y, x, z);
    }

    /// <summary>
    /// Convert quaternion to Euler angles (in degrees)
    /// </summary>
    public static Vector3 ToEuler(Quaternion rotation)
    {
        // Extract Euler angles
        var sinX = 2 * (rotation.W * rotation.X + rotation.Y * rotation.Z);
        var cosX = 1 - 2 * (rotation.X * rotation.X + rotation.Y * rotation.Y);
        var x = (float)Math.Atan2(sinX, cosX);

        var sinY = 2 * (rotation.W * rotation.Y - rotation.Z * rotation.X);
        var y = (float)Math.Asin(Math.Max(-1, Math.Min(1, sinY)));

        var sinZ = 2 * (rotation.W * rotation.Z + rotation.X * rotation.Y);
        var cosZ = 1 - 2 * (rotation.Y * rotation.Y + rotation.Z * rotation.Z);
        var z = (float)Math.Atan2(sinZ, cosZ);

        return new Vector3(x, y, z) * MathHelper.Rad2Deg;
    }

    /// <summary>
    /// Spherical linear interpolation between quaternions
    /// </summary>
    public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
    {
        return Quaternion.Slerp(a, b, MathHelper.Clamp01(t));
    }
}

/// <summary>
/// Random utilities for game development
/// </summary>
public static class RandomHelper
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Get a random float between 0 and 1
    /// </summary>
    public static float Value => (float)_random.NextDouble();

    /// <summary>
    /// Get a random float between min and max
    /// </summary>
    public static float Range(float min, float max)
    {
        return min + (max - min) * Value;
    }

    /// <summary>
    /// Get a random int between min (inclusive) and max (exclusive)
    /// </summary>
    public static int Range(int min, int max)
    {
        return _random.Next(min, max);
    }

    /// <summary>
    /// Get a random point inside a unit circle
    /// </summary>
    public static Vector2 InsideUnitCircle()
    {
        var angle = Range(0f, MathHelper.TwoPI);
        var radius = (float)Math.Sqrt(Value);
        return new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
    }

    /// <summary>
    /// Get a random point inside a unit sphere
    /// </summary>
    public static Vector3 InsideUnitSphere()
    {
        var theta = Range(0f, MathHelper.TwoPI);
        var phi = (float)Math.Acos(Range(-1f, 1f));
        var r = (float)Math.Pow(Value, 1f / 3f);
        
        var x = r * (float)Math.Sin(phi) * (float)Math.Cos(theta);
        var y = r * (float)Math.Sin(phi) * (float)Math.Sin(theta);
        var z = r * (float)Math.Cos(phi);
        
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Get a random unit vector (on unit sphere surface)
    /// </summary>
    public static Vector3 OnUnitSphere()
    {
        var theta = Range(0f, MathHelper.TwoPI);
        var phi = (float)Math.Acos(Range(-1f, 1f));
        
        var x = (float)Math.Sin(phi) * (float)Math.Cos(theta);
        var y = (float)Math.Sin(phi) * (float)Math.Sin(theta);
        var z = (float)Math.Cos(phi);
        
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Get a random color
    /// </summary>
    public static Vector4 ColorRGBA()
    {
        return new Vector4(Value, Value, Value, 1f);
    }

    /// <summary>
    /// Get a random color with alpha
    /// </summary>
    public static Vector4 ColorRGBA(bool randomAlpha)
    {
        return new Vector4(Value, Value, Value, randomAlpha ? Value : 1f);
    }
}
