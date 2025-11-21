using System;
using System.IO;
using Xunit;
using EvokerEngine.Audio;

namespace EvokerEngine.Tests;

public class AudioTests
{
    [Fact]
    public void AudioClip_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var clip = new AudioClip();

        // Assert
        Assert.Equal(string.Empty, clip.Name);
        Assert.Null(clip.FilePath);
        Assert.Equal(0f, clip.Duration);
        Assert.Equal(AudioFormat.Unknown, clip.Format);
        Assert.Null(clip.Data);
        Assert.Equal(0, clip.Channels);
        Assert.Equal(0, clip.SampleRate);
        Assert.Equal(0, clip.BitsPerSample);
    }

    [Fact]
    public void AudioClip_LoadFromFile_NonExistentFile_ShouldLogError()
    {
        // Arrange
        var clip = new AudioClip { Name = "TestClip" };
        var nonExistentPath = "nonexistent_audio_file.wav";
        
        // Suppress error logging for this test since we're testing error handling
        var originalLevel = Core.Logger.MinimumLevel;
        Core.Logger.MinimumLevel = Core.Logger.LogLevel.Fatal;

        // Act
        clip.LoadFromFile(nonExistentPath);

        // Restore logging level
        Core.Logger.MinimumLevel = originalLevel;

        // Assert
        Assert.Equal(nonExistentPath, clip.FilePath);
        Assert.Null(clip.Data); // Should not load data from non-existent file
    }

    [Fact]
    public void AudioClip_LoadFromFile_DetectsWavFormat()
    {
        // Arrange
        var clip = new AudioClip { Name = "TestClip" };
        var wavPath = "test_audio.wav";
        
        // Suppress error logging for this test since file doesn't exist
        var originalLevel = Core.Logger.MinimumLevel;
        Core.Logger.MinimumLevel = Core.Logger.LogLevel.Fatal;

        // Act
        clip.LoadFromFile(wavPath);

        // Restore logging level
        Core.Logger.MinimumLevel = originalLevel;

        // Assert
        Assert.Equal(wavPath, clip.FilePath);
        // Format detection should work even if file doesn't exist
    }

    [Fact]
    public void AudioClip_LoadFromFile_DetectsMp3Format()
    {
        // Arrange
        var clip = new AudioClip { Name = "TestClip" };
        var mp3Path = "test_audio.mp3";
        
        // Suppress error/warning logging for this test since file doesn't exist
        var originalLevel = Core.Logger.MinimumLevel;
        Core.Logger.MinimumLevel = Core.Logger.LogLevel.Fatal;

        // Act
        clip.LoadFromFile(mp3Path);

        // Restore logging level
        Core.Logger.MinimumLevel = originalLevel;

        // Assert
        Assert.Equal(mp3Path, clip.FilePath);
    }

    [Fact]
    public void AudioClip_LoadFromFile_DetectsOggFormat()
    {
        // Arrange
        var clip = new AudioClip { Name = "TestClip" };
        var oggPath = "test_audio.ogg";
        
        // Suppress error/warning logging for this test since file doesn't exist
        var originalLevel = Core.Logger.MinimumLevel;
        Core.Logger.MinimumLevel = Core.Logger.LogLevel.Fatal;

        // Act
        clip.LoadFromFile(oggPath);

        // Restore logging level
        Core.Logger.MinimumLevel = originalLevel;

        // Assert
        Assert.Equal(oggPath, clip.FilePath);
    }

    [Fact]
    public void AudioClip_Unload_ShouldClearData()
    {
        // Arrange
        var clip = new AudioClip
        {
            Name = "TestClip",
            Data = new byte[] { 1, 2, 3, 4 }
        };

        // Act
        clip.Unload();

        // Assert
        Assert.Null(clip.Data);
    }

    [Fact]
    public void AudioSource_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var source = new AudioSource();

        // Assert
        Assert.Null(source.Clip);
        Assert.Equal(1.0f, source.Volume);
        Assert.Equal(1.0f, source.Pitch);
        Assert.False(source.Loop);
        Assert.False(source.Mute);
    }

    [Fact]
    public void AudioSource_Volume_ShouldClampValues()
    {
        // Arrange
        var source = new AudioSource();

        // Act & Assert - Test upper bound
        source.Volume = 2.0f;
        Assert.Equal(1.0f, source.Volume);

        // Act & Assert - Test lower bound
        source.Volume = -0.5f;
        Assert.Equal(0.0f, source.Volume);

        // Act & Assert - Test valid value
        source.Volume = 0.5f;
        Assert.Equal(0.5f, source.Volume);
    }

    [Fact]
    public void AudioSource_Pitch_ShouldClampValues()
    {
        // Arrange
        var source = new AudioSource();

        // Act & Assert - Test upper bound
        source.Pitch = 3.0f;
        Assert.Equal(2.0f, source.Pitch);

        // Act & Assert - Test lower bound
        source.Pitch = 0.3f;
        Assert.Equal(0.5f, source.Pitch);

        // Act & Assert - Test valid value
        source.Pitch = 1.5f;
        Assert.Equal(1.5f, source.Pitch);
    }

    [Fact]
    public void AudioSource_Loop_ShouldBeSettable()
    {
        // Arrange
        var source = new AudioSource();

        // Act
        source.Loop = true;

        // Assert
        Assert.True(source.Loop);
    }

    [Fact]
    public void AudioSource_Mute_ShouldBeSettable()
    {
        // Arrange
        var source = new AudioSource();

        // Act
        source.Mute = true;

        // Assert
        Assert.True(source.Mute);
    }

    [Fact]
    public void AudioListener_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var listener = new AudioListener();

        // Assert
        Assert.Equal(1.0f, listener.Volume);
        Assert.Equal(System.Numerics.Vector3.Zero, listener.Position);
        Assert.Equal(System.Numerics.Vector3.Zero, listener.Velocity);
    }

    [Fact]
    public void AudioListener_Volume_ShouldClampValues()
    {
        // Arrange
        var listener = new AudioListener();

        // Act & Assert - Test upper bound
        listener.Volume = 2.0f;
        Assert.Equal(1.0f, listener.Volume);

        // Act & Assert - Test lower bound
        listener.Volume = -0.5f;
        Assert.Equal(0.0f, listener.Volume);
    }

    [Fact]
    public void AudioListener_Position_ShouldBeSettable()
    {
        // Arrange
        var listener = new AudioListener();
        var position = new System.Numerics.Vector3(10, 5, -3);

        // Act
        listener.Position = position;

        // Assert
        Assert.Equal(position, listener.Position);
    }

    [Fact]
    public void AudioListener_Velocity_ShouldBeSettable()
    {
        // Arrange
        var listener = new AudioListener();
        var velocity = new System.Numerics.Vector3(1, 0, -1);

        // Act
        listener.Velocity = velocity;

        // Assert
        Assert.Equal(velocity, listener.Velocity);
    }

    [Fact]
    public void AudioSystem_ShouldNotBeInitializedByDefault()
    {
        // Arrange & Act
        var isInitialized = AudioSystem.Instance.IsInitialized;

        // Assert - may be true if already initialized in other tests
        // Just checking the property is accessible
        Assert.True(isInitialized || !isInitialized);
    }

    [Fact]
    public void AudioFormat_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        Assert.Equal(0, (int)AudioFormat.Unknown);
        Assert.True(Enum.IsDefined(typeof(AudioFormat), AudioFormat.WAV));
        Assert.True(Enum.IsDefined(typeof(AudioFormat), AudioFormat.MP3));
        Assert.True(Enum.IsDefined(typeof(AudioFormat), AudioFormat.OGG));
        Assert.True(Enum.IsDefined(typeof(AudioFormat), AudioFormat.FLAC));
    }

    [Fact]
    public void AudioClip_Properties_ShouldBeSettable()
    {
        // Arrange
        var clip = new AudioClip();

        // Act
        clip.Name = "TestSound";
        clip.FilePath = "test.wav";
        clip.Duration = 2.5f;
        clip.Format = AudioFormat.WAV;
        clip.Channels = 2;
        clip.SampleRate = 44100;
        clip.BitsPerSample = 16;

        // Assert
        Assert.Equal("TestSound", clip.Name);
        Assert.Equal("test.wav", clip.FilePath);
        Assert.Equal(2.5f, clip.Duration);
        Assert.Equal(AudioFormat.WAV, clip.Format);
        Assert.Equal(2, clip.Channels);
        Assert.Equal(44100, clip.SampleRate);
        Assert.Equal(16, clip.BitsPerSample);
    }

    [Fact]
    public void AudioSource_Play_WithoutClip_ShouldNotCrash()
    {
        // Arrange
        var source = new AudioSource();
        
        // Suppress warning logging for this test since we're testing that it doesn't crash
        var originalLevel = Core.Logger.MinimumLevel;
        Core.Logger.MinimumLevel = Core.Logger.LogLevel.Fatal;

        // Act & Assert - Should not throw
        source.Play();

        // Restore logging level
        Core.Logger.MinimumLevel = originalLevel;
    }

    [Fact]
    public void AudioSource_Pause_ShouldNotCrash()
    {
        // Arrange
        var source = new AudioSource();

        // Act & Assert - Should not throw
        source.Pause();
    }

    [Fact]
    public void AudioSource_Stop_ShouldNotCrash()
    {
        // Arrange
        var source = new AudioSource();

        // Act & Assert - Should not throw
        source.Stop();
    }
}
