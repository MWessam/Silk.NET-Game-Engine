using System.Collections;
using System.Numerics;
using LunarEngine.Assets;
using Serilog;

namespace LunarEngine.GameObjects;

public class GameObject : IRenderer, IEnumerable<IComponent>
{
    public readonly Transform Transform = new();
    public string Name;
    private Dictionary<Type, IComponent> _cachedComponents = new(4);
    private Guid _guid;
    private GameObject() {}
    public static GameObject CreateGameObject(string name, params Type[] types)
    {
        var gameObject = new GameObject();
        gameObject.Name = name;
        foreach (var type in types)
        {
            if (gameObject._cachedComponents.ContainsKey(type))
            {
                Log.Error($"Type {type.Name} is already added to object {name}!");
                continue;
            }

            if (typeof(IComponent).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
            {
                try
                {
                    gameObject._cachedComponents.Add(type, gameObject.CreateComponent(type)!);
                }
                catch (Exception e)
                {
                    Log.Error($"Type {type.Name} does not contain a default constructor!");
                    Log.Error($"Error: {e.Message}");
                    throw;
                }
            }
            else
            {
                Log.Error($"Type {type.Name} is not a valid component. " +
                          $"Make sure that it implements {nameof(IComponent)} " +
                          $"and is not an interface/abstract class.");
            }
        }
        return gameObject;
    }
    public T AddComponent<T>() where T : IComponent
    {
        if (!_cachedComponents.ContainsKey(typeof(T)))
        {
            var createdComponent = CreateComponent<T>();
            _cachedComponents.Add(typeof(T), createdComponent);
            return createdComponent;
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
    public T? GetComponent<T>() where T : IComponent
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
        if (TryGetComponent(out SpriteRenderer renderer))
        {
            renderer.Render();
        }
    }
    public void Awake()
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Awake();
        }
    }
    public void Start()
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Start();
        }
    }
    public void Update(double delta)
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Update(delta);
        }
    }
    private T CreateComponent<T>() where T : IComponent
    {
        return (T) CreateComponent(typeof(T))!;
    }
    private IComponent? CreateComponent(Type type)
    {
        if (!typeof(IComponent).IsAssignableFrom(type))
        {
            return null;
        }
        var component = ComponentFactoryManager.CreateComponent(type, this);
        component!.AssignGameObject(this);
        return component;
    }
    public IEnumerator<IComponent> GetEnumerator()
    {
        return _cachedComponents.Select(x => x.Value).GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}