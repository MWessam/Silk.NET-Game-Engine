
using LunarEngine.ECS.Systems;
using LunarEngine.Renderer;
using LunarEngine.Scenes;
using LunarEngine.UI;

namespace LunarEngine.Application;

// Composition root: creates window, renderer, assets, and runs the editor application
public class EngineHost
{
    public void Run()
    {
        var app = new LunarEngine.Editor.Editor();
        app.Run();
    }
}
//