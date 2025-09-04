using LunarEngine.ECS.Systems;

namespace LunarEngine.GameEngine;
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