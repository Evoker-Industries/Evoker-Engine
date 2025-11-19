using System;
using System.Collections.Generic;

namespace EvokerEngine.Core;

/// <summary>
/// Manages a stack of layers
/// </summary>
public class LayerStack
{
    private readonly List<Layer> _layers = new();
    private int _layerInsertIndex = 0;

    /// <summary>
    /// Push a layer to the stack
    /// </summary>
    public void PushLayer(Layer layer)
    {
        _layers.Insert(_layerInsertIndex, layer);
        _layerInsertIndex++;
        layer.OnAttach();
    }

    /// <summary>
    /// Push an overlay (always on top)
    /// </summary>
    public void PushOverlay(Layer overlay)
    {
        _layers.Add(overlay);
        overlay.OnAttach();
    }

    /// <summary>
    /// Pop a layer from the stack
    /// </summary>
    public void PopLayer(Layer layer)
    {
        var index = _layers.IndexOf(layer);
        if (index >= 0 && index < _layerInsertIndex)
        {
            layer.OnDetach();
            _layers.RemoveAt(index);
            _layerInsertIndex--;
        }
    }

    /// <summary>
    /// Pop an overlay
    /// </summary>
    public void PopOverlay(Layer overlay)
    {
        var index = _layers.IndexOf(overlay);
        if (index >= _layerInsertIndex)
        {
            overlay.OnDetach();
            _layers.RemoveAt(index);
        }
    }

    /// <summary>
    /// Get all layers
    /// </summary>
    public IEnumerable<Layer> GetLayers() => _layers;
}
