using LunarEngine.GameEngine;
using Silk.NET.Maths;

namespace LunarEngine.Events;

public struct ViewportResizedEvent(Vector2D<int> size) : IEvent
{
    public Vector2D<int> Size { get; } = size;
}


