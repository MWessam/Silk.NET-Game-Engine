namespace LunarEngine.Events;

public interface IEvent { }

public static class EventBus<T> where T : struct, IEvent
{
    static readonly HashSet<Action<T>> _bindings = new();
    private static readonly HashSet<Action> _noArgsBindings = new();
    public static void Register(Action<T> binding) => _bindings.Add(binding);
    public static void Register(Action binding) => _noArgsBindings.Add(binding);
    public static void Deregister(Action<T> binding) => _bindings.Remove(binding);
    public static void Deregister(Action binding) => _noArgsBindings.Remove(binding);
    public static void Raise(T evt) 
    {
        foreach (var binding in _bindings) {
            binding?.Invoke(evt);
        }
        foreach (var binding in _noArgsBindings)
        {
            binding?.Invoke();
        }
    }
    public static void Raise()
    {
        foreach (var binding in _noArgsBindings)
        {
            binding.Invoke();
        }
    }
    static void Clear() {
        _bindings.Clear();
    }
}