using System.Numerics;
using Serilog;
using Silk.NET.Input;

namespace LunarEngine.InputEngine;

public class Input
{
    private Dictionary<Key, Action<Key>> _onKeyPressedMap = new();
    private Dictionary<Key, Action<Key>> _onKeyReleasedMap = new();
    private Dictionary<Key, Action<Key>> _onKeyHeldMap = new();
    private Dictionary<MouseButton, Action<MouseButton>> _onMousePressedMap = new();
    private Dictionary<MouseButton, Action<MouseButton>> _onMouseReleasedMap = new();
    private Dictionary<MouseButton, Action<MouseButton>> _onMouseHeldMap = new();
    private static Input? s_instance;
    private HashSet<Key> _heldKeys = new();
    private HashSet<MouseButton> _heldMouseButtons = new();
    public event Action<Vector2> OnMouseMoved;
    public Vector2 KeyboardAxis;
    public static Input Instance
    {
        get
        {
            if (s_instance is null)
            {
                s_instance = new Input();
                return s_instance;
            }
            return s_instance;
        }
    }
    private Input()
    {
        AddKeyHeldListener(Key.W, OnWPressed); 
        AddKeyHeldListener(Key.A, OnAPressed); 
        AddKeyHeldListener(Key.S, OnSPressed); 
        AddKeyHeldListener(Key.D, OnDPressed); 
        AddKeyUpListener(Key.W, OnWReleased); 
        AddKeyUpListener(Key.A, OnAReleased);
        AddKeyUpListener(Key.S, OnSReleased); 
        AddKeyUpListener(Key.D, OnDReleased); 
    }
    public static Input Create()
    {
        if (s_instance is null)
        {
            s_instance = new Input();
            return s_instance;
        }
        return s_instance;
    }

    #region LISTENERS

    public void AddKeyDownListener(Key key, Action<Key> action)
    {
        AddKeyListener(key, action, _onKeyPressedMap);
    }
    public void AddKeyUpListener(Key key, Action<Key> action)
    {
        AddKeyListener(key, action, _onKeyReleasedMap);
    }
    public void AddKeyHeldListener(Key key, Action<Key> action)
    {
        AddKeyListener(key, action, _onKeyHeldMap);
    }
    public void AddMouseDownListener(MouseButton mouseButton, Action<MouseButton> action)
    {
        AddMouseButtonListener(mouseButton, action, _onMousePressedMap);
    }
    public void AddMouseUpListener(MouseButton mouseButton, Action<MouseButton> action)
    {
        AddMouseButtonListener(mouseButton, action, _onMouseReleasedMap);
    }
    public void AddMouseHeldListener(MouseButton mouseButton, Action<MouseButton> action)
    {
        AddMouseButtonListener(mouseButton, action, _onMouseHeldMap);
    }
    public void RemoveMouseDownListener(MouseButton mouseButton, Action<MouseButton> action)
    {
        RemoveMouseButtonListener(mouseButton, action, _onMousePressedMap);
    }
    public void RemoveMouseUpListener(MouseButton mouseButton, Action<MouseButton> action)
    {
        RemoveMouseButtonListener(mouseButton, action, _onMouseReleasedMap);
    }
    public void RemoveMouseHeldListener(MouseButton mouseButton, Action<MouseButton> action)
    {
        RemoveMouseButtonListener(mouseButton, action, _onMouseHeldMap);
    }
    public void RemoveKeyDownListener(Key key, Action<Key> action)
    {
        RemoveKeyListener(key, action, _onKeyPressedMap);
    }
    public void RemoveKeyUpListener(Key key, Action<Key> action)
    {
        RemoveKeyListener(key, action, _onKeyReleasedMap);
    }
    public void RemoveKeyHeldListener(Key key, Action<Key> action)
    {
        RemoveKeyListener(key, action, _onKeyHeldMap);
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

        foreach (var mouseButton in _heldMouseButtons)
        {
            if (_onMouseHeldMap.TryGetValue(mouseButton, out var action))
            {
                action?.Invoke(mouseButton);
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
    public void OnMouseDown(IMouse mouse, MouseButton mouseButton)
    {
        _heldMouseButtons.Add(mouseButton);
        if (_onMousePressedMap.TryGetValue(mouseButton, out var action))
        {
            action?.Invoke(mouseButton);
        }
    }
    public void OnMouseUp(IMouse mouse, MouseButton mouseButton)
    {
        _heldMouseButtons.Remove(mouseButton);
        if (_onMouseReleasedMap.TryGetValue(mouseButton, out var action))
        {
            action?.Invoke(mouseButton);
        }
    }
    public void OnMouseMove(IMouse mouse, Vector2 move)
    {
        OnMouseMoved?.Invoke(move);
    }
    public void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        
    }
    private void AddKeyListener(Key key, Action<Key> action, Dictionary<Key, Action<Key>> actionMap)
    {
        if (actionMap.TryGetValue(key, out var actions))
        {
            actions += action;
            actionMap[key] = actions;
            return;
        }
        actionMap.Add(key, action);
    }
    private void AddMouseButtonListener(MouseButton key, Action<MouseButton> action, Dictionary<MouseButton, Action<MouseButton>> actionMap)
    {
        if (actionMap.TryGetValue(key, out var actions))
        {
            actions += action;
            actionMap[key] = actions;
            return;
        }
        actionMap.Add(key, action);
    }
    private void RemoveKeyListener(Key key, Action<Key> action, Dictionary<Key, Action<Key>> actionMap)
    {
        if (actionMap.TryGetValue(key, out var actions))
        {
            try
            {
                actions -= action;
                actionMap[key] = actions;
            }
            catch (Exception e)
            {
                Log.Error($"Action {action.Method.Name} of {action.Target?.GetType().Name} is not subscribed. Couldn't remove listener.");
            }
            return;
        }
        actionMap.Add(key, action);
    }
    private void RemoveMouseButtonListener(MouseButton key, Action<MouseButton> action, Dictionary<MouseButton, Action<MouseButton>> actionMap)
    {
        if (actionMap.TryGetValue(key, out var actions))
        {
            try
            {
                actions -= action;
                actionMap[key] = actions;
            }
            catch (Exception e)
            {
                Log.Error($"Action {action.Method.Name} of {action.Target?.GetType().Name} is not subscribed. Couldn't remove listener.");
            }
            return;
        }
        actionMap.Add(key, action);
    }

    #endregion

    #region Calculations

    private void OnDReleased(Key obj)
    {
        KeyboardAxis.X = 0;
    }
    private void OnSReleased(Key obj)
    {
        KeyboardAxis.Y = 0;
    }
    private void OnAReleased(Key obj)
    {
        KeyboardAxis.X = 0;
    }
    private void OnWReleased(Key obj)
    {
        KeyboardAxis.Y = 0;
    }
    private void OnWPressed(Key obj)
    {
        KeyboardAxis.Y = 1.0f;
    }
    private void OnAPressed(Key obj)
    {
        KeyboardAxis.X = 1.0f;
    }
    private void OnSPressed(Key obj)
    {
        KeyboardAxis.Y = -1.0f;
    }
    private void OnDPressed(Key obj)
    {
        KeyboardAxis.X = -1.0f;
    }

    #endregion
}