using System.Runtime.CompilerServices;

namespace LunarEngine.GameEngine;

public struct TimeStep
{
    private float _time;

    public TimeStep(double time)
    {
        _time = (float)time;
    }
    public TimeStep(float time)
    {
        _time = time;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float(TimeStep timeStep)
    {
        return timeStep._time;
    }
    public float InSeconds => _time;
    public float InMilliSeconds => _time * 1000.0f;
}