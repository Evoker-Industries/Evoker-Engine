using Xunit;
using EvokerEngine.Resources;

namespace EvokerEngine.Tests;

public class ResourceManagerTests
{
    [Fact]
    public void ResourceManager_Load_CreatesResource()
    {
        // Arrange
        var manager = new ResourceManager();

        // Act
        var texture = manager.Load<Texture>("TestTexture");

        // Assert
        Assert.NotNull(texture);
        Assert.Equal("TestTexture", texture.Name);
        Assert.True(texture.Id > 0);
    }

    [Fact]
    public void ResourceManager_Load_AssignsUniqueIds()
    {
        // Arrange
        var manager = new ResourceManager();

        // Act
        var texture1 = manager.Load<Texture>("Texture1");
        var texture2 = manager.Load<Texture>("Texture2");

        // Assert
        Assert.NotEqual(texture1.Id, texture2.Id);
    }

    [Fact]
    public void ResourceManager_Get_ReturnsLoadedResource()
    {
        // Arrange
        var manager = new ResourceManager();
        var texture = manager.Load<Texture>("TestTexture");

        // Act
        var retrieved = manager.Get<Texture>(texture.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(texture.Id, retrieved.Id);
        Assert.Equal(texture.Name, retrieved.Name);
    }

    [Fact]
    public void ResourceManager_Get_ReturnsNullForNonExistent()
    {
        // Arrange
        var manager = new ResourceManager();

        // Act
        var retrieved = manager.Get<Texture>(999);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void ResourceManager_Unload_RemovesResource()
    {
        // Arrange
        var manager = new ResourceManager();
        var texture = manager.Load<Texture>("TestTexture");
        var textureId = texture.Id;

        // Act
        manager.Unload(textureId);

        // Assert
        Assert.Null(manager.Get<Texture>(textureId));
    }

    [Fact]
    public void ResourceManager_UnloadAll_RemovesAllResources()
    {
        // Arrange
        var manager = new ResourceManager();
        var texture1 = manager.Load<Texture>("Texture1");
        var texture2 = manager.Load<Texture>("Texture2");
        var shader = manager.Load<Shader>("Shader1");

        // Act
        manager.UnloadAll();

        // Assert
        Assert.Null(manager.Get<Texture>(texture1.Id));
        Assert.Null(manager.Get<Texture>(texture2.Id));
        Assert.Null(manager.Get<Shader>(shader.Id));
    }

    [Fact]
    public void ResourceManager_LoadMultipleTypes()
    {
        // Arrange
        var manager = new ResourceManager();

        // Act
        var texture = manager.Load<Texture>("TestTexture");
        var shader = manager.Load<Shader>("TestShader");
        var mesh = manager.Load<Mesh>("TestMesh");

        // Assert
        Assert.NotNull(texture);
        Assert.NotNull(shader);
        Assert.NotNull(mesh);
        Assert.IsType<Texture>(texture);
        Assert.IsType<Shader>(shader);
        Assert.IsType<Mesh>(mesh);
    }
}

public class TextureTests
{
    [Fact]
    public void Texture_DefaultProperties()
    {
        // Arrange & Act
        var texture = new Texture();

        // Assert
        Assert.Equal(0, texture.Width);
        Assert.Equal(0, texture.Height);
        Assert.Null(texture.Data);
    }

    [Fact]
    public void Texture_CanSetProperties()
    {
        // Arrange
        var texture = new Texture();
        var data = new byte[] { 255, 0, 0, 255 };

        // Act
        texture.Width = 256;
        texture.Height = 256;
        texture.Data = data;

        // Assert
        Assert.Equal(256, texture.Width);
        Assert.Equal(256, texture.Height);
        Assert.Equal(data, texture.Data);
    }

    [Fact]
    public void Texture_Unload_ClearsData()
    {
        // Arrange
        var texture = new Texture
        {
            Data = new byte[] { 255, 0, 0, 255 }
        };

        // Act
        texture.Unload();

        // Assert
        Assert.Null(texture.Data);
    }
}

public class ShaderTests
{
    [Fact]
    public void Shader_DefaultProperties()
    {
        // Arrange & Act
        var shader = new Shader();

        // Assert
        Assert.Equal(string.Empty, shader.VertexShaderCode);
        Assert.Equal(string.Empty, shader.FragmentShaderCode);
    }

    [Fact]
    public void Shader_CanSetShaderCode()
    {
        // Arrange
        var shader = new Shader();

        // Act
        shader.VertexShaderCode = "vertex shader code";
        shader.FragmentShaderCode = "fragment shader code";

        // Assert
        Assert.Equal("vertex shader code", shader.VertexShaderCode);
        Assert.Equal("fragment shader code", shader.FragmentShaderCode);
    }
}

public class MeshTests
{
    [Fact]
    public void Mesh_DefaultProperties()
    {
        // Arrange & Act
        var mesh = new Mesh();

        // Assert
        Assert.Null(mesh.Vertices);
        Assert.Null(mesh.Indices);
    }

    [Fact]
    public void Mesh_CanSetData()
    {
        // Arrange
        var mesh = new Mesh();
        var vertices = new float[] { 0f, 0f, 0f, 1f, 1f, 1f };
        var indices = new uint[] { 0, 1, 2 };

        // Act
        mesh.Vertices = vertices;
        mesh.Indices = indices;

        // Assert
        Assert.Equal(vertices, mesh.Vertices);
        Assert.Equal(indices, mesh.Indices);
    }

    [Fact]
    public void Mesh_Unload_ClearsData()
    {
        // Arrange
        var mesh = new Mesh
        {
            Vertices = new float[] { 0f, 0f, 0f },
            Indices = new uint[] { 0, 1, 2 }
        };

        // Act
        mesh.Unload();

        // Assert
        Assert.Null(mesh.Vertices);
        Assert.Null(mesh.Indices);
    }
}
