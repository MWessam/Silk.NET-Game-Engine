using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using LunarEngine.Graphics;
using LunarEngine.Scenes;
using Serilog;

namespace LunarEngine.Physics;

public static class PhysicsEngine
{
    public const double FIXED_TIMESTAMP = 0.01666667;
    public static double InterpolatedTime;
    private static bool _isInPhysicsLoop;
    public static void TickPhysics(double deltaTime)
    {
        _isInPhysicsLoop = true;
        _isInPhysicsLoop = false;
    }
}