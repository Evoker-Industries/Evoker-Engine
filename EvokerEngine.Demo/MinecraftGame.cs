using System;
using System.Collections.Generic;
using System.Numerics;
using EvokerEngine.Core;
using EvokerEngine.ECS;
using EvokerEngine.Scene;
using EvokerEngine.Rendering;
using EvokerEngine.Blocks;
using Silk.NET.Input;

namespace EvokerEngine.Demo;

/// <summary>
/// Minecraft-like 3D demo showcasing the Evoker Engine
/// </summary>
class MinecraftGame
{
    public static void RunMinecraftDemo()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("   Evoker Engine - Minecraft-like Demo   ");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        Console.WriteLine("Controls:");
        Console.WriteLine("  WASD       - Move");
        Console.WriteLine("  Mouse      - Look around");
        Console.WriteLine("  Space      - Jump");
        Console.WriteLine("  Left Shift - Sprint");
        Console.WriteLine("  Left Click - Break block");
        Console.WriteLine("  Right Click- Place block");
        Console.WriteLine("  1-5        - Select block type");
        Console.WriteLine("  E          - Toggle particle effects");
        Console.WriteLine("  F          - Toggle flying mode");
        Console.WriteLine("  ESC        - Exit");
        Console.WriteLine();
        
        // Create and configure the application
        var app = new Application("Evoker Engine - Minecraft Demo", 1920, 1080);

        // Add the game layer
        app.PushLayer(new MinecraftGameLayer());

        // Run the application
        app.Run();
    }
}

/// <summary>
/// Main game layer for the Minecraft-like demo
/// </summary>
class MinecraftGameLayer : Layer
{
    // World
    private VoxelWorld? _world;
    private ChunkManager? _chunkManager;
    
    // Player
    private Entity _player;
    private Vector3 _playerPosition = new Vector3(0, 50, 0);
    private Vector3 _playerVelocity = Vector3.Zero;
    private Vector2 _playerRotation = Vector2.Zero; // X = pitch, Y = yaw
    private bool _isFlying = false;
    private bool _isOnGround = false;
    private float _moveSpeed = 5f;
    private float _sprintSpeed = 10f;
    private float _jumpForce = 8f;
    
    // Camera
    private Entity _camera;
    private Camera3DComponent? _cameraComponent;
    
    // Input
    private Vector2 _lastMousePos;
    private bool _mouseLocked = false;
    private float _mouseSensitivity = 0.002f;
    
    // Building
    private BlockType _selectedBlockType = BlockType.Grass;
    private float _blockBreakProgress = 0f;
    private Vector3? _targetBlock = null;
    private Vector3? _placeBlock = null;
    
    // Particles
    private ParticleSystemManager _particleManager = new ParticleSystemManager();
    private bool _particlesEnabled = true;
    
    // UI
    private float _fpsTimer = 0f;
    private int _frameCount = 0;
    private int _lastFps = 0;

    public MinecraftGameLayer() : base("Minecraft Game Layer")
    {
    }

    public override void OnAttach()
    {
        Logger.Info("=== Minecraft Demo Started ===");
        
        var scene = Application.Instance.ActiveScene;
        
        // Create player entity
        _player = scene.CreateEntity("Player");
        var playerTransform = scene.Registry.AddComponent<TransformComponent>(_player);
        playerTransform.Position = _playerPosition;
        
        // Create camera entity
        _camera = scene.CreateEntity("Camera");
        var cameraTransform = scene.Registry.AddComponent<TransformComponent>(_camera);
        cameraTransform.Position = _playerPosition + new Vector3(0, 1.6f, 0); // Eye level
        
        _cameraComponent = scene.Registry.AddComponent<Camera3DComponent>(_camera);
        _cameraComponent.IsPrimary = true;
        _cameraComponent.FieldOfView = 70f;
        _cameraComponent.NearPlane = 0.1f;
        _cameraComponent.FarPlane = 500f;
        _cameraComponent.ClearColor = new Vector4(0.5f, 0.7f, 1.0f, 1.0f); // Sky blue
        
        // Initialize world
        _world = new VoxelWorld();
        _chunkManager = new ChunkManager(_world);
        
        // Generate initial chunks around player
        GenerateChunksAroundPlayer();
        
        Logger.Info($"World initialized with {_chunkManager.ChunkCount} chunks");
        Logger.Info($"Starting position: {_playerPosition}");
        Logger.Info("Demo ready! Use WASD to move, mouse to look, Space to jump");
    }

    public override void OnDetach()
    {
        Logger.Info("=== Minecraft Demo Ended ===");
    }

    public override void OnUpdate(float deltaTime)
    {
        // Update FPS counter
        UpdateFPS(deltaTime);
        
        // Update player physics
        UpdatePlayerPhysics(deltaTime);
        
        // Update camera
        UpdateCamera();
        
        // Update input
        HandleInput(deltaTime);
        
        // Update particle systems
        var scene = Application.Instance.ActiveScene;
        _particleManager.Update(deltaTime, scene.Registry);
        
        // Generate/unload chunks based on player position
        if (_chunkManager != null)
        {
            _chunkManager.UpdateChunks(_playerPosition);
        }
    }

    public override void OnRender()
    {
        // In a real implementation, this would render the voxel world
        // For this demo, we're focusing on the game logic
    }

    public override void OnEvent(Event e)
    {
        if (e is KeyPressedEvent keyEvent)
        {
            HandleKeyPress(keyEvent);
        }
        else if (e is MouseButtonPressedEvent mouseEvent)
        {
            HandleMouseClick(mouseEvent);
        }
        else if (e is MouseMovedEvent mouseMoveEvent)
        {
            HandleMouseMove(mouseMoveEvent);
        }
    }

    private void UpdateFPS(float deltaTime)
    {
        _frameCount++;
        _fpsTimer += deltaTime;
        
        if (_fpsTimer >= 1.0f)
        {
            _lastFps = _frameCount;
            Logger.Debug($"FPS: {_lastFps} | Position: ({_playerPosition.X:F1}, {_playerPosition.Y:F1}, {_playerPosition.Z:F1}) | Chunks: {_chunkManager?.ChunkCount ?? 0}");
            _frameCount = 0;
            _fpsTimer = 0f;
        }
    }

    private void UpdatePlayerPhysics(float deltaTime)
    {
        if (_isFlying)
        {
            // Flying mode - no gravity
            _playerVelocity *= 0.9f; // Air resistance
        }
        else
        {
            // Apply gravity
            _playerVelocity.Y -= 20f * deltaTime;
            
            // Check if on ground
            _isOnGround = CheckGround();
            
            if (_isOnGround && _playerVelocity.Y < 0)
            {
                _playerVelocity.Y = 0;
                
                // Snap to ground
                int groundY = (int)Math.Floor(_playerPosition.Y);
                _playerPosition.Y = groundY + 1.8f;
            }
            
            // Apply ground friction
            if (_isOnGround)
            {
                _playerVelocity.X *= 0.8f;
                _playerVelocity.Z *= 0.8f;
            }
        }
        
        // Apply velocity
        _playerPosition += _playerVelocity * deltaTime;
        
        // Update player entity transform
        var scene = Application.Instance.ActiveScene;
        var playerTransform = scene.Registry.GetComponent<TransformComponent>(_player);
        if (playerTransform != null)
        {
            playerTransform.Position = _playerPosition;
        }
    }

    private void UpdateCamera()
    {
        var scene = Application.Instance.ActiveScene;
        var cameraTransform = scene.Registry.GetComponent<TransformComponent>(_camera);
        
        if (cameraTransform != null)
        {
            // Position camera at player eye level
            cameraTransform.Position = _playerPosition + new Vector3(0, 1.6f, 0);
            
            // Apply rotation
            cameraTransform.Rotation = new Vector3(_playerRotation.X, _playerRotation.Y, 0);
        }
    }

    private void HandleInput(float deltaTime)
    {
        // Movement
        var moveDir = Vector3.Zero;
        var speed = Input.IsKeyPressed(Key.ShiftLeft) ? _sprintSpeed : _moveSpeed;
        
        // Calculate movement direction relative to camera
        float yaw = _playerRotation.Y;
        Vector3 forward = new Vector3(MathF.Sin(yaw), 0, MathF.Cos(yaw));
        Vector3 right = new Vector3(MathF.Cos(yaw), 0, -MathF.Sin(yaw));
        
        if (Input.IsKeyPressed(Key.W))
            moveDir += forward;
        if (Input.IsKeyPressed(Key.S))
            moveDir -= forward;
        if (Input.IsKeyPressed(Key.A))
            moveDir -= right;
        if (Input.IsKeyPressed(Key.D))
            moveDir += right;
        
        if (moveDir.LengthSquared() > 0)
        {
            moveDir = Vector3.Normalize(moveDir);
            
            if (_isFlying)
            {
                // Direct movement in flying mode
                _playerPosition += moveDir * speed * deltaTime;
            }
            else
            {
                // Add to velocity for ground movement
                _playerVelocity.X = moveDir.X * speed;
                _playerVelocity.Z = moveDir.Z * speed;
            }
        }
        
        // Up/down in flying mode
        if (_isFlying)
        {
            if (Input.IsKeyPressed(Key.Space))
                _playerPosition.Y += speed * deltaTime;
            if (Input.IsKeyPressed(Key.ShiftLeft))
                _playerPosition.Y -= speed * deltaTime;
        }
        
        // Update target block for mining/placing
        UpdateTargetBlock();
        
        // Mining
        if (Input.IsMouseButtonPressed(MouseButton.Left) && _targetBlock.HasValue)
        {
            _blockBreakProgress += deltaTime;
            
            if (_blockBreakProgress >= 0.3f) // 0.3 seconds to break
            {
                BreakBlock(_targetBlock.Value);
                _blockBreakProgress = 0f;
            }
        }
        else
        {
            _blockBreakProgress = 0f;
        }
    }

    private void HandleKeyPress(KeyPressedEvent e)
    {
        if (e.IsRepeat)
            return;
        
        switch (e.KeyCode)
        {
            case Key.Escape:
                Logger.Info("Exiting demo...");
                Application.Instance.Close();
                break;
            
            case Key.F:
                _isFlying = !_isFlying;
                Logger.Info($"Flying mode: {(_isFlying ? "ON" : "OFF")}");
                if (_isFlying)
                {
                    _playerVelocity = Vector3.Zero;
                }
                break;
            
            case Key.E:
                _particlesEnabled = !_particlesEnabled;
                Logger.Info($"Particles: {(_particlesEnabled ? "ON" : "OFF")}");
                break;
            
            case Key.Space:
                if (!_isFlying && _isOnGround)
                {
                    _playerVelocity.Y = _jumpForce;
                    Logger.Debug("Jump!");
                }
                break;
            
            case Key.Number1:
                _selectedBlockType = BlockType.Grass;
                Logger.Info("Selected: Grass");
                break;
            
            case Key.Number2:
                _selectedBlockType = BlockType.Dirt;
                Logger.Info("Selected: Dirt");
                break;
            
            case Key.Number3:
                _selectedBlockType = BlockType.Stone;
                Logger.Info("Selected: Stone");
                break;
            
            case Key.Number4:
                _selectedBlockType = BlockType.Wood;
                Logger.Info("Selected: Wood");
                break;
            
            case Key.Number5:
                _selectedBlockType = BlockType.Leaves;
                Logger.Info("Selected: Leaves");
                break;
        }
    }

    private void HandleMouseClick(MouseButtonPressedEvent e)
    {
        if (e.Button == MouseButton.Right && _placeBlock.HasValue)
        {
            PlaceBlock(_placeBlock.Value, _selectedBlockType);
        }
    }

    private void HandleMouseMove(MouseMovedEvent e)
    {
        if (!_mouseLocked)
        {
            _mouseLocked = true;
            _lastMousePos = new Vector2(e.X, e.Y);
            return;
        }
        
        var delta = new Vector2(e.X - _lastMousePos.X, e.Y - _lastMousePos.Y);
        _lastMousePos = new Vector2(e.X, e.Y);
        
        // Update rotation
        _playerRotation.Y -= delta.X * _mouseSensitivity; // Yaw
        _playerRotation.X -= delta.Y * _mouseSensitivity; // Pitch
        
        // Clamp pitch
        _playerRotation.X = Math.Clamp(_playerRotation.X, -MathF.PI / 2f + 0.01f, MathF.PI / 2f - 0.01f);
    }

    private bool CheckGround()
    {
        if (_world == null) return false;
        
        // Check if there's a block below the player
        Vector3 feetPos = _playerPosition - new Vector3(0, 1.8f, 0);
        int x = (int)Math.Floor(feetPos.X);
        int y = (int)Math.Floor(feetPos.Y) - 1;
        int z = (int)Math.Floor(feetPos.Z);
        
        return _world.GetBlock(x, y, z) != BlockType.Air;
    }

    private void UpdateTargetBlock()
    {
        if (_world == null) return;
        
        // Raycast from camera
        Vector3 rayStart = _playerPosition + new Vector3(0, 1.6f, 0);
        float yaw = _playerRotation.Y;
        float pitch = _playerRotation.X;
        
        Vector3 rayDir = new Vector3(
            MathF.Sin(yaw) * MathF.Cos(pitch),
            -MathF.Sin(pitch),
            MathF.Cos(yaw) * MathF.Cos(pitch)
        );
        
        float maxDistance = 5f;
        float step = 0.1f;
        
        _targetBlock = null;
        _placeBlock = null;
        
        Vector3 prevPos = rayStart;
        
        for (float dist = 0; dist < maxDistance; dist += step)
        {
            Vector3 pos = rayStart + rayDir * dist;
            int x = (int)Math.Floor(pos.X);
            int y = (int)Math.Floor(pos.Y);
            int z = (int)Math.Floor(pos.Z);
            
            if (_world.GetBlock(x, y, z) != BlockType.Air)
            {
                _targetBlock = new Vector3(x, y, z);
                
                // Place position is the previous position
                int px = (int)Math.Floor(prevPos.X);
                int py = (int)Math.Floor(prevPos.Y);
                int pz = (int)Math.Floor(prevPos.Z);
                _placeBlock = new Vector3(px, py, pz);
                
                break;
            }
            
            prevPos = pos;
        }
    }

    private void BreakBlock(Vector3 position)
    {
        if (_world == null) return;
        
        int x = (int)position.X;
        int y = (int)position.Y;
        int z = (int)position.Z;
        
        var blockType = _world.GetBlock(x, y, z);
        if (blockType == BlockType.Air) return;
        
        _world.SetBlock(x, y, z, BlockType.Air);
        
        Logger.Info($"Broke {blockType} at ({x}, {y}, {z})");
        
        // Spawn particles
        if (_particlesEnabled)
        {
            SpawnBreakParticles(position, blockType);
        }
    }

    private void PlaceBlock(Vector3 position, BlockType blockType)
    {
        if (_world == null) return;
        
        int x = (int)position.X;
        int y = (int)position.Y;
        int z = (int)position.Z;
        
        // Don't place if it would intersect player
        Vector3 blockCenter = position + new Vector3(0.5f, 0.5f, 0.5f);
        if (Vector3.Distance(blockCenter, _playerPosition) < 1.5f)
            return;
        
        if (_world.GetBlock(x, y, z) != BlockType.Air) return;
        
        _world.SetBlock(x, y, z, blockType);
        
        Logger.Info($"Placed {blockType} at ({x}, {y}, {z})");
    }

    private void SpawnBreakParticles(Vector3 position, BlockType blockType)
    {
        var scene = Application.Instance.ActiveScene;
        
        // Create particle entity
        var particleEntity = scene.CreateEntity($"BreakParticles_{position}");
        var transform = scene.Registry.AddComponent<TransformComponent>(particleEntity);
        transform.Position = position + new Vector3(0.5f, 0.5f, 0.5f);
        
        var particleComponent = scene.Registry.AddComponent<ParticleSystemComponent>(particleEntity);
        particleComponent.MaxParticles = 50;
        particleComponent.EmissionRate = 0f; // Burst only
        particleComponent.Lifetime = 0.5f;
        particleComponent.StartSize = 0.1f;
        particleComponent.EndSize = 0.05f;
        particleComponent.Velocity = Vector3.Zero;
        particleComponent.VelocityVariation = new Vector3(3, 3, 3);
        particleComponent.Gravity = new Vector3(0, -15f, 0);
        particleComponent.IsPlaying = false;
        particleComponent.Loop = false;
        
        // Color based on block type
        particleComponent.StartColor = GetBlockColor(blockType);
        particleComponent.EndColor = particleComponent.StartColor * 0.5f;
        
        var particleSystem = _particleManager.CreateParticleSystem(particleEntity, particleComponent);
        
        // Burst particles
        particleSystem.Burst(30, transform.Position);
    }

    private Vector4 GetBlockColor(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Grass => new Vector4(0.3f, 0.8f, 0.3f, 1f),
            BlockType.Dirt => new Vector4(0.6f, 0.4f, 0.2f, 1f),
            BlockType.Stone => new Vector4(0.5f, 0.5f, 0.5f, 1f),
            BlockType.Wood => new Vector4(0.6f, 0.4f, 0.2f, 1f),
            BlockType.Leaves => new Vector4(0.2f, 0.6f, 0.2f, 1f),
            _ => new Vector4(1f, 1f, 1f, 1f)
        };
    }

    private void GenerateChunksAroundPlayer()
    {
        if (_chunkManager == null) return;
        
        int chunkX = (int)Math.Floor(_playerPosition.X / 16f);
        int chunkZ = (int)Math.Floor(_playerPosition.Z / 16f);
        
        int renderDistance = 4;
        
        for (int x = chunkX - renderDistance; x <= chunkX + renderDistance; x++)
        {
            for (int z = chunkZ - renderDistance; z <= chunkZ + renderDistance; z++)
            {
                _chunkManager.GenerateChunk(x, z);
            }
        }
    }
}

/// <summary>
/// Block types available in the demo
/// </summary>
public enum BlockType
{
    Air = 0,
    Grass = 1,
    Dirt = 2,
    Stone = 3,
    Wood = 4,
    Leaves = 5
}

/// <summary>
/// Voxel world managing all blocks
/// </summary>
public class VoxelWorld
{
    private Dictionary<(int x, int y, int z), BlockType> _blocks = new Dictionary<(int, int, int), BlockType>();
    
    public BlockType GetBlock(int x, int y, int z)
    {
        if (_blocks.TryGetValue((x, y, z), out var block))
            return block;
        return BlockType.Air;
    }
    
    public void SetBlock(int x, int y, int z, BlockType blockType)
    {
        if (blockType == BlockType.Air)
            _blocks.Remove((x, y, z));
        else
            _blocks[(x, y, z)] = blockType;
    }
    
    public int BlockCount => _blocks.Count;
}

/// <summary>
/// Manages chunk generation and loading
/// </summary>
public class ChunkManager
{
    private VoxelWorld _world;
    private HashSet<(int x, int z)> _generatedChunks = new HashSet<(int, int)>();
    private Random _random = new Random();
    
    public ChunkManager(VoxelWorld world)
    {
        _world = world;
    }
    
    public int ChunkCount => _generatedChunks.Count;
    
    public void GenerateChunk(int chunkX, int chunkZ)
    {
        if (_generatedChunks.Contains((chunkX, chunkZ)))
            return;
        
        _generatedChunks.Add((chunkX, chunkZ));
        
        // Generate terrain
        int startX = chunkX * 16;
        int startZ = chunkZ * 16;
        
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                int worldX = startX + x;
                int worldZ = startZ + z;
                
                // Simple terrain generation
                int height = 40 + (int)(10 * MathF.Sin(worldX * 0.1f) * MathF.Cos(worldZ * 0.1f));
                
                for (int y = 0; y < height; y++)
                {
                    BlockType blockType;
                    if (y == height - 1)
                        blockType = BlockType.Grass;
                    else if (y > height - 4)
                        blockType = BlockType.Dirt;
                    else
                        blockType = BlockType.Stone;
                    
                    _world.SetBlock(worldX, y, worldZ, blockType);
                }
                
                // Add some trees
                if (_random.Next(0, 20) == 0 && height > 30)
                {
                    GenerateTree(worldX, height, worldZ);
                }
            }
        }
    }
    
    private void GenerateTree(int x, int y, int z)
    {
        // Trunk
        for (int i = 0; i < 5; i++)
        {
            _world.SetBlock(x, y + i, z, BlockType.Wood);
        }
        
        // Leaves
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = 3; dy <= 6; dy++)
            {
                for (int dz = -2; dz <= 2; dz++)
                {
                    if (Math.Abs(dx) == 2 && Math.Abs(dz) == 2 && dy > 4)
                        continue; // Skip corners
                    
                    _world.SetBlock(x + dx, y + dy, z + dz, BlockType.Leaves);
                }
            }
        }
    }
    
    public void UpdateChunks(Vector3 playerPosition)
    {
        // In a full implementation, this would unload far chunks
        // and generate new ones as the player moves
    }
}
