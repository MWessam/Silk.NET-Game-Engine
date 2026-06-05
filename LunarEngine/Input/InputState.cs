using System.Numerics;
using System.Collections.Generic;
using Silk.NET.Input;

namespace LunarEngine.Input;

public class AxisMapping
{
    public Key PositiveX { get; set; }
    public Key NegativeX { get; set; }
    public Key PositiveY { get; set; }
    public Key NegativeY { get; set; }

    public Vector2 Evaluate(IReadOnlySet<Key> keysDown)
    {
        Vector2 axis = Vector2.Zero;
        if (keysDown.Contains(PositiveX)) axis.X += 1.0f;
        if (keysDown.Contains(NegativeX)) axis.X -= 1.0f;
        if (keysDown.Contains(PositiveY)) axis.Y += 1.0f;
        if (keysDown.Contains(NegativeY)) axis.Y -= 1.0f;
        return axis;
    }
}

public readonly struct InputState
{
    private readonly IReadOnlySet<Key> _keysDown;
    private readonly IReadOnlySet<Key> _keysPressed;
    private readonly IReadOnlySet<Key> _keysReleased;
    private readonly IReadOnlyDictionary<string, Vector2> _axes;

    public InputState(
        IReadOnlySet<Key> keysDown,
        IReadOnlySet<Key> keysPressed,
        IReadOnlySet<Key> keysReleased,
        Vector2 mousePosition,
        Vector2 mouseDelta,
        float scrollDelta,
        IReadOnlyDictionary<string, Vector2> axes)
    {
        _keysDown = keysDown;
        _keysPressed = keysPressed;
        _keysReleased = keysReleased;
        MousePosition = mousePosition;
        MouseDelta = mouseDelta;
        ScrollDelta = scrollDelta;
        _axes = axes;
    }

    public Vector2 MousePosition { get; }
    public Vector2 MouseDelta { get; }
    public float ScrollDelta { get; }

    public bool IsKeyDown(Key key) => _keysDown?.Contains(key) ?? false;
    public bool IsKeyPressed(Key key) => _keysPressed?.Contains(key) ?? false;
    public bool IsKeyReleased(Key key) => _keysReleased?.Contains(key) ?? false;

    public Vector2 GetAxis(string name)
    {
        if (_axes != null && _axes.TryGetValue(name, out var axis))
            return axis;
        return Vector2.Zero;
    }
}
