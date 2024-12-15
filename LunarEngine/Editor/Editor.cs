using Arch.Buffer;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Silk.NET.Maths;

namespace LunarEngine.ECS.Systems;

public class Editor : Singleton<Editor>, ISingletonObject, IDisposable
{
    private HierarchySystem _hierarchySystem;
    private InspectorSystem _inspectorSystem;
    private SceneSystem _sceneSystem;
    public void Initialize(ECSScene scene)
    {
        _hierarchySystem = new HierarchySystem(scene.World);
        _inspectorSystem = new InspectorSystem(scene.World);
        _sceneSystem = new SceneSystem();
        _hierarchySystem.Awake();
        _inspectorSystem.Awake();
        _sceneSystem.Awake();
    }
    public void EditorLoop(ECSScene scene, in float dt)
    {
        Renderer.Instance.Clear();
        _sceneSystem.Draw(Renderer.Instance, scene, dt);
        _hierarchySystem.Update(dt);
        _inspectorSystem.Update(dt);
        UIEngine.Render();
    }
    public void InitSingleton()
    {
        
    }
    public void Dispose()
    {
        
    }

    public void OnViewportResize(Vector2D<int> viewport)
    {
    }
}