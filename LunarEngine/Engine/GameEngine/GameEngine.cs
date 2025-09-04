
using LunarEngine.ECS.Systems;
using LunarEngine.Engine.Graphics;
using LunarEngine.Scenes;
using LunarEngine.UI;

namespace LunarEngine.GameEngine;

// Composition root: creates window, renderer, assets, and runs the editor application
public class EngineHost
{
    public void Run()
    {
        // Ensure renderer is constructed and listening for GL init
        _ = Renderer.Instance;

        var app = new Editor();
        app.CreateWindow();
        app.Run();
    }
}
//