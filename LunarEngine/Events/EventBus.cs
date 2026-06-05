namespace LunarEngine.Events;

public interface IEvent { }

public sealed class EventBus<T> where T : struct, IEvent
{
    private readonly HashSet<Action<T>> _bindings = new();
    private readonly HashSet<Action> _noArgsBindings = new();

    public void Subscribe(Action<T> binding) => _bindings.Add(binding);
    public void Subscribe(Action binding) => _noArgsBindings.Add(binding);
    public void Unsubscribe(Action<T> binding) => _bindings.Remove(binding);
    public void Unsubscribe(Action binding) => _noArgsBindings.Remove(binding);
    public void Publish(T evt) 
    {
        foreach (var binding in _bindings) {
            binding?.Invoke(evt);
        }
        foreach (var binding in _noArgsBindings)
        {
            binding?.Invoke();
        }
    }
    public void Publish()
    {
        foreach (var binding in _noArgsBindings)
        {
            binding.Invoke();
        }
    }
    public void Clear() {
        _bindings.Clear();
        _noArgsBindings.Clear();
    }
}
