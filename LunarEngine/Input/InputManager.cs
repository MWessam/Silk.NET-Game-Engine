using System;
using System.Collections.Generic;
using System.Numerics;
using LunarEngine.Platform;
using Silk.NET.Input;

namespace LunarEngine.Input;

public sealed class InputManager : IDisposable
{
    private readonly LunarEngine.Platform.IInputContext _context;
    private readonly HashSet<Key> _keysDown = new();
    private readonly HashSet<Key> _keysPressedThisFrame = new();
    private readonly HashSet<Key> _keysReleasedThisFrame = new();
    private readonly HashSet<MouseButton> _mouseDown = new();
    private Vector2 _mousePosition;
    private Vector2 _mouseDelta;
    private float _scrollDelta;
    private readonly Dictionary<string, AxisMapping> _axisMappings = new();

    public InputState State { get; private set; }

    public event Action<Key>? KeyDown;
    public event Action<Key>? KeyUp;
    public event Action<MouseButton>? MouseDown;
    public event Action<MouseButton>? MouseUp;
    public event Action<Vector2>? MouseMoved;
    public event Action<float>? MouseScrolled;

    public InputManager(LunarEngine.Platform.IInputContext context)
    {
        _context = context;

        foreach (var keyboard in context.Keyboards)
        {
            keyboard.KeyDown += OnKeyboardKeyDown;
            keyboard.KeyUp += OnKeyboardKeyUp;
        }

        foreach (var mouse in context.Mice)
        {
            mouse.MouseDown += OnMouseButtonDown;
            mouse.MouseUp += OnMouseButtonUp;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseScroll;
        }
    }

    public void AddAxis(string name, AxisMapping mapping)
    {
        _axisMappings[name] = mapping;
    }

    public void Update()
    {
        var axes = new Dictionary<string, Vector2>();
        foreach (var kvp in _axisMappings)
        {
            axes[kvp.Key] = kvp.Value.Evaluate(_keysDown);
        }

        State = new InputState(
            new HashSet<Key>(_keysDown),
            new HashSet<Key>(_keysPressedThisFrame),
            new HashSet<Key>(_keysReleasedThisFrame),
            _mousePosition,
            _mouseDelta,
            _scrollDelta,
            axes);

        _keysPressedThisFrame.Clear();
        _keysReleasedThisFrame.Clear();
        _mouseDelta = Vector2.Zero;
        _scrollDelta = 0f;
    }

    public void SetCursorLock(bool locked)
    {
        foreach (var mouse in _context.Mice)
        {
            if (mouse.Cursor != null)
                mouse.Cursor.CursorMode = locked ? CursorMode.Raw : CursorMode.Normal;
        }
    }

    private void OnKeyboardKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (_keysDown.Add(key))
        {
            _keysPressedThisFrame.Add(key);
        }
        KeyDown?.Invoke(key);
    }

    private void OnKeyboardKeyUp(IKeyboard keyboard, Key key, int keyCode)
    {
        _keysDown.Remove(key);
        _keysReleasedThisFrame.Add(key);
        KeyUp?.Invoke(key);
    }

    private void OnMouseButtonDown(IMouse mouse, MouseButton button)
    {
        if (_mouseDown.Add(button))
        {
            MouseDown?.Invoke(button);
        }
    }

    private void OnMouseButtonUp(IMouse mouse, MouseButton button)
    {
        _mouseDown.Remove(button);
        MouseUp?.Invoke(button);
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        var delta = position - _mousePosition;
        delta.Y = -delta.Y;
        _mouseDelta += delta;
        _mousePosition = position;
        MouseMoved?.Invoke(delta);
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        _scrollDelta += scroll.Y;
        MouseScrolled?.Invoke(scroll.Y);
    }

    public void Dispose()
    {
        foreach (var keyboard in _context.Keyboards)
        {
            keyboard.KeyDown -= OnKeyboardKeyDown;
            keyboard.KeyUp -= OnKeyboardKeyUp;
        }

        foreach (var mouse in _context.Mice)
        {
            mouse.MouseDown -= OnMouseButtonDown;
            mouse.MouseUp -= OnMouseButtonUp;
            mouse.MouseMove -= OnMouseMove;
            mouse.Scroll -= OnMouseScroll;
        }
    }
}
