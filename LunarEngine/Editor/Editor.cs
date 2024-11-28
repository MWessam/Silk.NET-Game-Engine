using LunarEngine.Engine.Graphics;
using LunarEngine.Scenes;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public class Editor
{
    public static Editor? Instance;
    private HierarchySystem _hierarchySystem;
    private InspectorSystem _inspectorSystem;
    private SceneSystem _sceneSystem;

    public static Editor Create()
    {
        if (Instance is not null) return Instance;
        var editor = new Editor();
        Instance = editor;
        return editor;
    }

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
        _hierarchySystem.Update(dt);
        _inspectorSystem.Update(dt);
        _sceneSystem.Draw(Renderer.Instance, scene, dt);
        UIEngine.Render();
    }
    private Editor(){}
}