using Serilog;

namespace LunarEngine.GameObjects;
internal interface IComponentFactory
{
    IComponent? Produce(GameObject gameObject);
}

internal abstract class BaseComponentFactory : IComponentFactory 
{
    public abstract IComponent? Produce(GameObject gameObject);

    public static T? ProduceDefault<T>(GameObject gameObject) where T : class, IComponent
    {
        var args = new object[] { gameObject };
        var componentInstance = Activator.CreateInstance(typeof(T), args);
        if (componentInstance is null)
        {
            Log.Error($"Couldn't create instance of type {typeof(T).Name}");
            return null;
        }
        return (T)componentInstance;
    }
    public static IComponent? ProduceDefault(Type type, GameObject gameObject)
    {
        if (!typeof(IComponent).IsAssignableFrom(type))
        {
            Log.Error($"Type {type.Name} is not a component!");
            return null;
        }
        var args = new object[] { gameObject };
        var componentInstance = Activator.CreateInstance(type, args);
        if (componentInstance is null)
        {
            Log.Error($"Couldn't create instance of type {type.Name}");
            return null;
        }
        return (IComponent) componentInstance;
    }
}
internal static class ComponentFactoryManager
{
    internal static Dictionary<Type, IComponentFactory> ComponentFactories = new();
    static ComponentFactoryManager()
    {
        ComponentFactories.Add(typeof(SpriteRenderer), new SpriteRendererFactory());
    }

    internal static T? CreateComponent<T>(GameObject gameObject) where T : class, IComponent
    {
        return (T) CreateComponent(typeof(T), gameObject)!;
    }
    internal static IComponent? CreateComponent(Type componentType, GameObject gameObject)
    {
        if (ComponentFactories.TryGetValue(componentType, out var factory))
        {
            return factory.Produce(gameObject)!;
        }
        // Log.Debug($"No explicit factories for component {componentType.Name} was found. Using default factory.");
        return BaseComponentFactory.ProduceDefault(componentType, gameObject);
    }
}