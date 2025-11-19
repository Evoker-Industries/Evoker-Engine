using System;
using System.IO;
using Silk.NET.OpenAL;

namespace EvokerEngine.Audio;

/// <summary>
/// Audio clip resource
/// </summary>
public class AudioClip
{
    public string Name { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public float Duration { get; set; }
    public AudioFormat Format { get; set; } = AudioFormat.Unknown;
    public byte[]? Data { get; set; }
    public int Channels { get; set; }
    public int SampleRate { get; set; }
    public int BitsPerSample { get; set; }

    /// <summary>
    /// Load audio from file
    /// </summary>
    public void LoadFromFile(string path)
    {
        FilePath = path;
        
        if (!File.Exists(path))
        {
            Core.Logger.Error($"Audio file not found: {path}");
            return;
        }

        // Detect format from extension
        if (path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            Format = AudioFormat.WAV;
            LoadWavFile(path);
        }
        else if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            Format = AudioFormat.MP3;
            Core.Logger.Warning($"MP3 format not yet supported: {path}");
        }
        else if (path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            Format = AudioFormat.OGG;
            Core.Logger.Warning($"OGG format not yet supported: {path}");
        }
        else
        {
            Core.Logger.Error($"Unsupported audio format: {path}");
        }
    }

    private void LoadWavFile(string path)
    {
        try
        {
            using var fileStream = File.OpenRead(path);
            using var reader = new BinaryReader(fileStream);

            // Read RIFF header
            string chunkID = new string(reader.ReadChars(4));
            if (chunkID != "RIFF")
            {
                Core.Logger.Error($"Invalid WAV file (missing RIFF): {path}");
                return;
            }

            reader.ReadInt32(); // Chunk size
            string format = new string(reader.ReadChars(4));
            if (format != "WAVE")
            {
                Core.Logger.Error($"Invalid WAV file (not WAVE): {path}");
                return;
            }

            // Read fmt chunk
            string fmtChunkID = new string(reader.ReadChars(4));
            if (fmtChunkID != "fmt ")
            {
                Core.Logger.Error($"Invalid WAV file (missing fmt): {path}");
                return;
            }

            int fmtChunkSize = reader.ReadInt32();
            int audioFormat = reader.ReadInt16();
            Channels = reader.ReadInt16();
            SampleRate = reader.ReadInt32();
            reader.ReadInt32(); // Byte rate
            reader.ReadInt16(); // Block align
            BitsPerSample = reader.ReadInt16();

            // Skip extra format bytes if any
            if (fmtChunkSize > 16)
            {
                reader.ReadBytes(fmtChunkSize - 16);
            }

            // Find data chunk
            string dataChunkID = new string(reader.ReadChars(4));
            while (dataChunkID != "data" && fileStream.Position < fileStream.Length)
            {
                int chunkSize = reader.ReadInt32();
                reader.ReadBytes(chunkSize);
                if (fileStream.Position >= fileStream.Length) break;
                dataChunkID = new string(reader.ReadChars(4));
            }

            if (dataChunkID != "data")
            {
                Core.Logger.Error($"Invalid WAV file (missing data chunk): {path}");
                return;
            }

            int dataSize = reader.ReadInt32();
            Data = reader.ReadBytes(dataSize);
            Duration = (float)dataSize / (SampleRate * Channels * (BitsPerSample / 8));

            Core.Logger.Info($"Loaded WAV audio: {Name} ({SampleRate}Hz, {Channels}ch, {BitsPerSample}bit, {Duration:F2}s)");
        }
        catch (Exception ex)
        {
            Core.Logger.Error($"Failed to load WAV file {path}: {ex.Message}");
        }
    }

    /// <summary>
    /// Unload audio data
    /// </summary>
    public void Unload()
    {
        Data = null;
    }
}

/// <summary>
/// Audio format
/// </summary>
public enum AudioFormat
{
    Unknown,
    WAV,
    MP3,
    OGG,
    FLAC
}

/// <summary>
/// Audio source for playing sounds
/// </summary>
public class AudioSource
{
    internal uint SourceId { get; set; }
    public AudioClip? Clip { get; set; }
    private uint _bufferId;
    
    public float Volume 
    { 
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (AudioSystem.Instance.IsInitialized)
            {
                AudioSystem.Instance.AL.SetSourceProperty(SourceId, SourceFloat.Gain, _volume);
            }
        }
    }
    private float _volume = 1.0f;

    public float Pitch 
    { 
        get => _pitch;
        set
        {
            _pitch = Math.Clamp(value, 0.5f, 2.0f);
            if (AudioSystem.Instance.IsInitialized)
            {
                AudioSystem.Instance.AL.SetSourceProperty(SourceId, SourceFloat.Pitch, _pitch);
            }
        }
    }
    private float _pitch = 1.0f;

    public bool Loop 
    { 
        get => _loop;
        set
        {
            _loop = value;
            if (AudioSystem.Instance.IsInitialized)
            {
                AudioSystem.Instance.AL.SetSourceProperty(SourceId, SourceBoolean.Looping, _loop);
            }
        }
    }
    private bool _loop;

    public bool IsPlaying 
    { 
        get
        {
            if (!AudioSystem.Instance.IsInitialized) return false;
            AudioSystem.Instance.AL.GetSourceProperty(SourceId, GetSourceInteger.SourceState, out int state);
            return state == (int)SourceState.Playing;
        }
    }

    public bool Mute { get; set; }

    internal void SetBuffer(uint bufferId)
    {
        _bufferId = bufferId;
        if (AudioSystem.Instance.IsInitialized)
        {
            AudioSystem.Instance.AL.SetSourceProperty(SourceId, SourceInteger.Buffer, (int)bufferId);
        }
    }

    /// <summary>
    /// Play the audio clip
    /// </summary>
    public void Play()
    {
        if (Clip == null)
        {
            Core.Logger.Warning("Cannot play AudioSource: No clip assigned");
            return;
        }

        if (!AudioSystem.Instance.IsInitialized)
        {
            Core.Logger.Warning("Cannot play AudioSource: Audio system not initialized");
            return;
        }

        if (Mute) return;

        AudioSystem.Instance.AL.SourcePlay(SourceId);
    }

    /// <summary>
    /// Pause playback
    /// </summary>
    public void Pause()
    {
        if (AudioSystem.Instance.IsInitialized)
        {
            AudioSystem.Instance.AL.SourcePause(SourceId);
        }
    }

    /// <summary>
    /// Stop playback
    /// </summary>
    public void Stop()
    {
        if (AudioSystem.Instance.IsInitialized)
        {
            AudioSystem.Instance.AL.SourceStop(SourceId);
        }
    }
}

/// <summary>
/// Audio listener (typically attached to camera)
/// </summary>
public class AudioListener
{
    public float Volume 
    { 
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (AudioSystem.Instance.IsInitialized)
            {
                AudioSystem.Instance.AL.SetListenerProperty(ListenerFloat.Gain, _volume);
            }
        }
    }
    private float _volume = 1.0f;

    public System.Numerics.Vector3 Position 
    { 
        get => _position;
        set
        {
            _position = value;
            if (AudioSystem.Instance.IsInitialized)
            {
                AudioSystem.Instance.AL.SetListenerProperty(ListenerVector3.Position, _position.X, _position.Y, _position.Z);
            }
        }
    }
    private System.Numerics.Vector3 _position;

    public System.Numerics.Vector3 Velocity 
    { 
        get => _velocity;
        set
        {
            _velocity = value;
            if (AudioSystem.Instance.IsInitialized)
            {
                AudioSystem.Instance.AL.SetListenerProperty(ListenerVector3.Velocity, _velocity.X, _velocity.Y, _velocity.Z);
            }
        }
    }
    private System.Numerics.Vector3 _velocity;
}

/// <summary>
/// Audio system manager
/// </summary>
public class AudioSystem
{
    private static AudioSystem? _instance;
    public static AudioSystem Instance => _instance ??= new AudioSystem();

    internal AL AL { get; private set; } = null!;
    internal ALContext ALContext { get; private set; } = null!;
    private unsafe Device* _device;
    private unsafe Context* _context;
    private readonly Dictionary<string, uint> _buffers = new();
    private readonly List<AudioSource> _sources = new();
    public bool IsInitialized { get; private set; }

    private AudioSystem() { }

    /// <summary>
    /// Initialize the audio system
    /// </summary>
    public unsafe void Initialize()
    {
        try
        {
            AL = AL.GetApi();
            ALContext = ALContext.GetApi();
            
            // Open default device
            _device = ALContext.OpenDevice("");
            if (_device == null)
            {
                Core.Logger.Error("Failed to open audio device");
                return;
            }

            // Create context
            _context = ALContext.CreateContext(_device, null);
            if (_context == null)
            {
                Core.Logger.Error("Failed to create audio context");
                ALContext.CloseDevice(_device);
                return;
            }

            ALContext.MakeContextCurrent(_context);
            IsInitialized = true;

            Core.Logger.Info("Audio system initialized successfully");
        }
        catch (Exception ex)
        {
            Core.Logger.Error($"Failed to initialize audio system: {ex.Message}");
        }
    }

    /// <summary>
    /// Shutdown the audio system
    /// </summary>
    public unsafe void Shutdown()
    {
        if (!IsInitialized) return;

        // Delete all buffers
        foreach (var bufferId in _buffers.Values)
        {
            AL.DeleteBuffer(bufferId);
        }
        _buffers.Clear();

        // Delete all sources
        foreach (var source in _sources)
        {
            AL.DeleteSource(source.SourceId);
        }
        _sources.Clear();

        // Cleanup context and device
        if (_context != null)
        {
            ALContext.MakeContextCurrent(null);
            ALContext.DestroyContext(_context);
        }

        if (_device != null)
        {
            ALContext.CloseDevice(_device);
        }

        IsInitialized = false;
        Core.Logger.Info("Audio system shutdown");
    }

    /// <summary>
    /// Create an audio source
    /// </summary>
    public AudioSource CreateSource()
    {
        if (!IsInitialized)
        {
            Core.Logger.Warning("Cannot create audio source: Audio system not initialized");
            return new AudioSource();
        }

        uint sourceId = AL.GenSource();
        var source = new AudioSource { SourceId = sourceId };
        _sources.Add(source);
        return source;
    }

    /// <summary>
    /// Load audio clip and create buffer
    /// </summary>
    public uint LoadClip(AudioClip clip)
    {
        if (!IsInitialized || clip.Data == null)
        {
            return 0;
        }

        // Check if already loaded
        if (_buffers.TryGetValue(clip.Name, out uint existingBuffer))
        {
            return existingBuffer;
        }

        // Create buffer
        uint buffer = AL.GenBuffer();

        // Determine OpenAL format
        BufferFormat format = clip.Channels == 1 
            ? (clip.BitsPerSample == 16 ? BufferFormat.Mono16 : BufferFormat.Mono8)
            : (clip.BitsPerSample == 16 ? BufferFormat.Stereo16 : BufferFormat.Stereo8);

        // Upload audio data
        AL.BufferData(buffer, format, clip.Data, clip.SampleRate);

        _buffers[clip.Name] = buffer;
        Core.Logger.Info($"Loaded audio buffer for: {clip.Name}");

        return buffer;
    }

    /// <summary>
    /// Play a one-shot sound effect
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1.0f)
    {
        if (!IsInitialized || clip.Data == null)
        {
            Core.Logger.Warning($"Cannot play one-shot audio: {clip.Name}");
            return;
        }

        var source = CreateSource();
        uint buffer = LoadClip(clip);
        source.SetBuffer(buffer);
        source.Volume = volume;
        source.Clip = clip;
        source.Play();

        // Note: In a real implementation, you'd want to clean up one-shot sources after they finish
    }
}

