using LunarEngine.ECS.Systems;
using LunarEngine.Application;

namespace LunarEngine;
// Graphics Engine:
// Camera, Window
public class Program
{
    static void Main(string[] args)
    {
        var host = new EngineHost();
        host.Run();
    }
}