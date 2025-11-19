using Xunit;
using EvokerEngine.Modding;
using EvokerEngine.Core;
using System.Collections.Generic;

namespace EvokerEngine.Tests;

public class ModdingTests
{
    private class TestMod : Mod
    {
        public bool LoadCalled { get; private set; }
        public bool InitializeCalled { get; private set; }
        public bool PostInitializeCalled { get; private set; }
        public bool UnloadCalled { get; private set; }
        public int UpdateCount { get; private set; }

        public TestMod(string modId = "testmod")
        {
            Info = new ModInfo
            {
                ModId = modId,
                Name = "Test Mod",
                Version = "1.0.0",
                Author = "Test Author"
            };
        }

        public override void OnLoad()
        {
            base.OnLoad();
            LoadCalled = true;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            InitializeCalled = true;
        }

        public override void OnPostInitialize()
        {
            base.OnPostInitialize();
            PostInitializeCalled = true;
        }

        public override void OnUnload()
        {
            base.OnUnload();
            UnloadCalled = true;
        }

        public override void OnUpdate(float deltaTime)
        {
            UpdateCount++;
        }
    }

    [Fact]
    public void Mod_HasCorrectInfo()
    {
        // Arrange & Act
        var mod = new TestMod();

        // Assert
        Assert.Equal("testmod", mod.Info.ModId);
        Assert.Equal("Test Mod", mod.Info.Name);
        Assert.Equal("1.0.0", mod.Info.Version);
        Assert.Equal("Test Author", mod.Info.Author);
    }

    [Fact]
    public void ModLoader_LoadsMod()
    {
        // Arrange
        var loader = ModLoader.Instance;
        var mod = new TestMod("loadtest");

        // Act
        loader.LoadMod(mod);

        // Assert
        Assert.True(mod.IsLoaded);
        Assert.True(mod.LoadCalled);
        Assert.True(loader.IsModLoaded("loadtest"));
    }

    [Fact]
    public void ModLoader_InitializesAllMods()
    {
        // Arrange
        var loader = ModLoader.Instance;
        var mod1 = new TestMod("initmod1");
        var mod2 = new TestMod("initmod2");
        
        loader.LoadMod(mod1);
        loader.LoadMod(mod2);

        // Act
        loader.InitializeAll();

        // Assert
        Assert.True(mod1.InitializeCalled);
        Assert.True(mod1.PostInitializeCalled);
        Assert.True(mod2.InitializeCalled);
        Assert.True(mod2.PostInitializeCalled);
    }

    [Fact]
    public void ModLoader_UpdatesAllMods()
    {
        // Arrange
        var loader = ModLoader.Instance;
        var mod = new TestMod("updatemod");
        loader.LoadMod(mod);

        // Act
        loader.UpdateAll(0.016f);
        loader.UpdateAll(0.016f);

        // Assert
        Assert.Equal(2, mod.UpdateCount);
    }

    [Fact]
    public void ModLoader_UnloadsMod()
    {
        // Arrange
        var loader = ModLoader.Instance;
        var mod = new TestMod("unloadmod");
        loader.LoadMod(mod);

        // Act
        var success = loader.UnloadMod("unloadmod");

        // Assert
        Assert.True(success);
        Assert.True(mod.UnloadCalled);
        Assert.False(mod.IsLoaded);
        Assert.False(loader.IsModLoaded("unloadmod"));
    }

    [Fact]
    public void ModLoader_RespectsModDependencies()
    {
        // Arrange
        var loader = ModLoader.Instance;
        var baseMod = new TestMod("basemod");
        var dependentMod = new TestMod("depmod");
        dependentMod.Info.Dependencies.Add("basemod");

        // Act - Load dependent mod first (should fail without base)
        loader.LoadMod(dependentMod);
        var loadedBefore = loader.IsModLoaded("depmod");

        // Load base mod
        loader.LoadMod(baseMod);
        
        // Try loading dependent mod again
        loader.LoadMod(dependentMod);
        var loadedAfter = loader.IsModLoaded("depmod");

        // Assert
        Assert.False(loadedBefore);  // Should not load without dependency
        Assert.True(loadedAfter);    // Should load with dependency
    }

    [Fact]
    public void ModLoader_GetModReturnsCorrectMod()
    {
        // Arrange
        var loader = ModLoader.Instance;
        var mod = new TestMod("getmod");
        loader.LoadMod(mod);

        // Act
        var retrieved = loader.GetMod("getmod");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(mod, retrieved);
    }

    [Fact]
    public void ModLoader_CountsLoadedMods()
    {
        // Arrange
        var loader = ModLoader.Instance;
        var initialCount = loader.ModCount;
        
        var mod1 = new TestMod("countmod1");
        var mod2 = new TestMod("countmod2");

        // Act
        loader.LoadMod(mod1);
        loader.LoadMod(mod2);

        // Assert
        Assert.Equal(initialCount + 2, loader.ModCount);
    }

    [Fact]
    public void Mod_GetResourceKeyUsesModNamespace()
    {
        // Arrange
        var mod = new TestMod("mymod");

        // Act
        var key = mod.GetAPI().Key("custom_item");

        // Assert
        Assert.Equal("mymod:custom_item", key.FullKey);
    }
}
