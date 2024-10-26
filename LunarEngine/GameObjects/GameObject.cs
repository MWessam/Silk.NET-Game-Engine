using Serilog;

namespace LunarEngine.GameObjects;

public class GameObject : Component, IRenderer
{
    public readonly Transform Transform = new();
    public string Name;
    private Dictionary<Type, IComponent> _cachedComponents = new(4);
    private Guid _guid;
    
    private GameObject() {}
    public static GameObject CreateGameObject(string name)
    {
        var gameObject = new GameObject();
        gameObject.Name = name;
        return gameObject;
    }
    public T AddComponent<T>() where T : IComponent
    {
        if (!_cachedComponents.ContainsKey(typeof(T)))
        {
            // var createdComponent = CreateComponent<T>();
            // return createdComponent;
        }
        var component = (T)_cachedComponents[typeof(T)];
        return component;
    }
    public void AddComponent<T>(T component) where T : IComponent
    {
        if (_cachedComponents.TryAdd(typeof(T), component))
        {
            component.AssignGameObject(this);
            return;
        }
        _cachedComponents[typeof(T)] = component;
        component.AssignGameObject(this);
    }
    public T GetComponent<T>() where T : IComponent
    {
        if (_cachedComponents.ContainsKey(typeof(T))) return (T) _cachedComponents[typeof(T)];
        Log.Error($"GameObject {Name} doesn't contain the component of type {typeof(T).Name}");
        return default;
    }

    public bool TryGetComponent<T>(out T component) where T : IComponent
    {
        component = default;
        if (_cachedComponents.TryGetValue(typeof(T), out var cachedComponent))
        {
            component = (T)cachedComponent;
            return true;
        }
        return false;
    }
    public void AssignGuid(Guid guid)
    {
        _guid = guid;
    }

    public void Render()
    {
        if (TryGetComponent(out IRenderer renderer))
        {
            renderer.Render();
        }
    }

    public override void Awake()
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Awake();
        }
    }
    public override void Start()
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Start();
        }
    }
    public override void Update(double delta)
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Update(delta);
        }
    }

    public void IMGUIPropertyEditor()
    {
        
    }

    protected virtual void IMGUIExtraPropertiesEditor()
    {
        
    }
    private T CreateComponent<T>() where T : IComponent, new()
    {
        var component = new T();
        component.AssignGameObject(this);
        _cachedComponents.Add(typeof(T), component);
        return component;
    }

}