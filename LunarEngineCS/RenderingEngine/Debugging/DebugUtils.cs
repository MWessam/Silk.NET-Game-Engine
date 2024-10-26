using System.Diagnostics;
using Serilog;

namespace LunarEngineCS.RenderingEngine.Debugging;

public static class DebugUtils
{
    public static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Log.Error("Assertion failed: {Message}", message);
            Debug.Fail(message);
        }
    }
}