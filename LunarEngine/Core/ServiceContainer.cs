using System.Diagnostics.CodeAnalysis;

namespace LunarEngine.Core;

public readonly struct ServiceDescriptor
{
    public Type ServiceType { get; }
    public Type? ImplementationType { get; }
    public object? Instance { get; }
    public Func<object>? Factory { get; }

    public ServiceDescriptor(Type serviceType, object instance)
    {
        ServiceType = serviceType;
        Instance = instance;
    }

    public ServiceDescriptor(Type serviceType, Func<object> factory)
    {
        ServiceType = serviceType;
        Factory = factory;
    }

    public ServiceDescriptor(Type serviceType, Type implementationType)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
    }
}

public sealed class ServiceContainer
{
    private readonly Dictionary<Type, object> _services = new();
    private readonly Dictionary<Type, Func<object>> _lazyFactories = new();

    public void Register<TService>(TService instance) where TService : class
    {
        _services[typeof(TService)] = instance;
    }

    public void RegisterLazy<TService>(Func<TService> factory) where TService : class
    {
        _lazyFactories[typeof(TService)] = () => factory();
    }

    public TService Get<TService>()
    {
        var type = typeof(TService);
        if (_services.TryGetValue(type, out var instance))
            return (TService)instance;

        if (_lazyFactories.TryGetValue(type, out var factory))
        {
            instance = factory();
            _services[type] = instance;
            _lazyFactories.Remove(type);
            return (TService)instance;
        }

        throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
    }

    public bool TryGet<TService>([NotNullWhen(true)] out TService? service)
    {
        var type = typeof(TService);
        if (_services.TryGetValue(type, out var instance))
        {
            service = (TService)instance;
            return true;
        }

        if (_lazyFactories.TryGetValue(type, out var factory))
        {
            instance = factory();
            _services[type] = instance;
            _lazyFactories.Remove(type);
            service = (TService)instance;
            return true;
        }

        service = default;
        return false;
    }

    public void Reset()
    {
        _services.Clear();
        _lazyFactories.Clear();
    }
}
