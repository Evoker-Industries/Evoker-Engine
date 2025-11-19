using Xunit;
using EvokerEngine.Core;

namespace EvokerEngine.Tests;

public class LayerStackTests
{
    private class TestLayer : Layer
    {
        public bool WasAttached { get; private set; }
        public bool WasDetached { get; private set; }
        public int UpdateCount { get; private set; }

        public TestLayer(string name) : base(name) { }

        public override void OnAttach() => WasAttached = true;
        public override void OnDetach() => WasDetached = true;
        public override void OnUpdate(float deltaTime) => UpdateCount++;
    }

    [Fact]
    public void LayerStack_PushLayer_AddsLayer()
    {
        // Arrange
        var stack = new LayerStack();
        var layer = new TestLayer("Test");

        // Act
        stack.PushLayer(layer);

        // Assert
        Assert.Contains(layer, stack.GetLayers());
        Assert.True(layer.WasAttached);
    }

    [Fact]
    public void LayerStack_PopLayer_RemovesLayer()
    {
        // Arrange
        var stack = new LayerStack();
        var layer = new TestLayer("Test");
        stack.PushLayer(layer);

        // Act
        stack.PopLayer(layer);

        // Assert
        Assert.DoesNotContain(layer, stack.GetLayers());
        Assert.True(layer.WasDetached);
    }

    [Fact]
    public void LayerStack_PushOverlay_AddsToEnd()
    {
        // Arrange
        var stack = new LayerStack();
        var layer1 = new TestLayer("Layer1");
        var overlay = new TestLayer("Overlay");

        // Act
        stack.PushLayer(layer1);
        stack.PushOverlay(overlay);

        // Assert
        var layers = stack.GetLayers().ToList();
        Assert.Equal(2, layers.Count);
        Assert.Equal(layer1, layers[0]);
        Assert.Equal(overlay, layers[1]);
    }

    [Fact]
    public void LayerStack_MultipleLayersInOrder()
    {
        // Arrange
        var stack = new LayerStack();
        var layer1 = new TestLayer("Layer1");
        var layer2 = new TestLayer("Layer2");
        var layer3 = new TestLayer("Layer3");

        // Act
        stack.PushLayer(layer1);
        stack.PushLayer(layer2);
        stack.PushLayer(layer3);

        // Assert
        var layers = stack.GetLayers().ToList();
        Assert.Equal(3, layers.Count);
    }
}
