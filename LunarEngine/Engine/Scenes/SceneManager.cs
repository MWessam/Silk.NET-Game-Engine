using Serilog;
using LunarEngine.Core;

namespace LunarEngine.Scenes;

public class SceneManager
{
    private readonly List<IScene> _scenes = new();
    private readonly ServiceContainer _services;

    public SceneManager(ServiceContainer services)
    {
        _services = services;
    }

    public IScene? ActiveScene { get; private set; }
    public IReadOnlyList<IScene> Scenes => _scenes;

    public void AddScene(IScene scene)
    {
        _scenes.Add(scene);
        ActiveScene = scene;
    }

    public IScene CreateScene(string name)
    {
        var scene = new TestEcsScene(_services) { Name = name };
        AddScene(scene);
        return scene;
    }

    public bool RemoveScene(IScene scene)
    {
        if (!_scenes.Remove(scene))
        {
            Log.Error("Scene not found in manager. Cannot remove.");
            return false;
        }

        if (ActiveScene == scene)
        {
            ActiveScene = _scenes.Count > 0 ? _scenes[_scenes.Count - 1] : null;
        }

        scene.Dispose();
        return true;
    }
}
