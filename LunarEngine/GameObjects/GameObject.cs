using System.Collections;
using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Scenes;
using Serilog;

namespace LunarEngine.GameObjects;

public class GameObject : IEnumerable<IComponent>
{
    public readonly Transform Transform = new();
    public Scene ParentScene { get; private set; }
    public string Name;
    public bool Enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            if (_enabled && value == false)
            {
                _enabled = value;
                OnDisable();
                return;
            }

            if (!_enabled && value == true)
            {
                _enabled = value;
                OnEnable();
                return;
            }
        }
    }
    private Dictionary<Type, IComponent> _cachedComponents = new(4);
    private Guid _guid;
    private bool _enabled;
    private bool _initialized;
    private GameObject() {}
    public static GameObject CreateGameObject(string name, Scene scene, params Type[] types)
    {
        var gameObject = new GameObject();
        gameObject.Name = name;
        gameObject.ParentScene = scene;
        gameObject.Transform.LocalScale = new Vector3(1.0f, 1.0f, 1.0f);
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
        gameObject.Enabled = true;
        scene.AddObject(gameObject);
        return gameObject;
    }
    public T AddComponent<T>() where T : IComponent
    {
        if (!_cachedComponents.ContainsKey(typeof(T)))
        {
            var createdComponent = CreateComponent<T>();
            return AddComponent(createdComponent);
        }
        var component = (T)_cachedComponents[typeof(T)];
        return component;
    }
    public T AddComponent<T>(T component) where T : IComponent
    {
        if (_cachedComponents.TryAdd(typeof(T), component))
        {
            component.AssignGameObject(this);
            if (_initialized)
            {
                component.Awake();
                component.OnEnable();
                component.Start();
            }
            return component;
        }
        _cachedComponents[typeof(T)] = component;
        component.AssignGameObject(this);
        return component;
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
    public void OnEnable()
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.OnEnable();
        }
    }
    public void Start()
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Start();
        }
        _initialized = true;
    }
    public void Update(double delta)
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Update(delta);
        }
    }

    public void Tick(float fixedDt)
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.Tick(fixedDt);
        }
    }
    public void OnDisable()
    {
        foreach (var component in _cachedComponents)
        {
            component.Value.OnDisable();
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

    public GameObject Instantiate(GameObject gameObject)
    {
        List<Type> components = new();
        foreach (var component in gameObject._cachedComponents)
        {
            components.Add(component.Key);
        }
        GameObject obj = CreateGameObject(gameObject.Name, ParentScene, components.ToArray());
        foreach (var componentType in components)
        {
            var component = obj._cachedComponents[componentType];
            component.Clone(gameObject._cachedComponents[componentType]);
        }
        return obj;
    }
}