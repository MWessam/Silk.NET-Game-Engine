namespace LunarEngine.GameEngine;

public abstract class BaseLayer
{
    public string Name { get; init; }

    protected BaseLayer(string name)
    {
        Name = name;
    }

    public virtual void OnAttach() {}
    public virtual void OnDetach() {}
    public virtual void OnInitialize() {}
    public virtual void OnUpdate(TimeStep timeStep) {}
    public virtual void OnImguiRender(TimeStep timeStep) {}
}

public class LayerStack
{
    private readonly List<BaseLayer> _layers = new();
    private int _layerInsertIndex = 0;

    public void PushLayer(BaseLayer layer)
    {
        _layers.Insert(_layerInsertIndex, layer);
        _layerInsertIndex++;
    }

    public void PushOverlay(BaseLayer overlay)
    {
        _layers.Add(overlay);
    }

    public void PopLayer(BaseLayer layer)
    {
        int index = _layers.IndexOf(layer);
        if (index != -1 && index < _layerInsertIndex)
        {
            layer.OnDetach();
            _layers.RemoveAt(index);
            _layerInsertIndex--;
        }
    }

    public void PopOverlay(BaseLayer overlay)
    {
        int index = _layers.IndexOf(overlay);
        if (index != -1 && index >= _layerInsertIndex)
        {
            overlay.OnDetach();
            _layers.RemoveAt(index);
        }
    }

    public IEnumerator<BaseLayer> GetEnumerator() => _layers.GetEnumerator();
    public IEnumerable<BaseLayer> Reverse() => ((IEnumerable<BaseLayer>)_layers).Reverse();

    ~LayerStack()
    {
        foreach (var layer in this)
        {
            layer.OnDetach();
        }
    }
}
