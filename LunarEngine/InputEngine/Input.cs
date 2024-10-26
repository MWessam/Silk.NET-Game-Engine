using Silk.NET.Input;

namespace LunarEngine.InputEngine;


public class InputEngine
{
    private Dictionary<Key, Action<Key>> _onKeyPressedMap = new();
    private Dictionary<Key, Action<Key>> _onKeyReleasedMap = new();
    private Dictionary<Key, Action<Key>> _onKeyHeldMap = new();
    private static InputEngine? s_instance;
    public static InputEngine Instance
    {
        get
        {
            if (s_instance is null)
            {
                s_instance = new InputEngine();
                return s_instance;
            }
            return s_instance;
        }
    }
    private HashSet<Key> _heldKeys = new();
    private InputEngine()
    {
    }
    public static InputEngine Create()
    {
        if (s_instance is null)
        {
            s_instance = new InputEngine();
            return s_instance;
        }
        return s_instance;
    }
    public void AddKeyDownListener(Key key, Action<Key> action)
    {
        if (_onKeyPressedMap.TryGetValue(key, out var actions))
        {
            action += actions;
            return;
        }
        _onKeyPressedMap.Add(key, action);
    }
    public void AddKeyUpListener(Key key, Action<Key> action)
    {
        if (_onKeyReleasedMap.TryGetValue(key, out var actions))
        {
            action += actions;
            return;
        }
        _onKeyReleasedMap.Add(key, action);
    }
    public void AddKeyHeldListener(Key key, Action<Key> action)
    {
        if (_onKeyHeldMap.TryGetValue(key, out var actions))
        {
            action += actions;
            return;
        }
        _onKeyHeldMap.Add(key, action);
    }
    internal void Update(double delta)
    {
        foreach (var key in _heldKeys)
        {
            if (_onKeyHeldMap.TryGetValue(key, out var action))
            {
                action?.Invoke(key);
            }
        }
    }
    internal void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        _heldKeys.Add(key);
        if (_onKeyPressedMap.TryGetValue(key, out var action))
        {
            action?.Invoke(key);
        }
    }
    internal void OnKeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        _heldKeys.Remove(key);
        if (_onKeyReleasedMap.TryGetValue(key, out var action))
        {
            action?.Invoke(key);
        }
    }

}