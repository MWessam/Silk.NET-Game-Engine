using Serilog;

namespace LunarEngine.Scenes;

public class SceneManager
{
    // Max scene count 16.
    private const int MAX_SCENE_COUNT = 16;
    private Scene?[] _scenes = new Scene[MAX_SCENE_COUNT];
    private int _lastSceneIndex = -1;
    
    public void AddScene(Scene scene)
    {
        if (_lastSceneIndex >= 15)
        {
            Log.Error($"Scenes are already full! Can't add more scenes.");
            return;
        }
        _scenes[++_lastSceneIndex] = scene;
        scene.SceneId = _lastSceneIndex;
    }

    public Scene? RemoveScene(int sceneId)
    {
        if (sceneId < 0 || sceneId > _lastSceneIndex)
        {
            Log.Error($"Scene Id is invalid. Make sure that you specified the correct id for removal.");
            return null;
        }

        _scenes[sceneId] = null;
        // TODO: Shift array to remove scenes and update their ids.
        return null;
    }
    public void RemoveScene(Scene scene)
    {
        
    }

    public void OnViewportResized()
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            Scene scene = _scenes[i]!;
            if (!scene.IsActive) return;
            scene.UpdateViewProjectionUniforms();
        }
    }

    public void RenderScenes()
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            Scene scene = _scenes[i]!;
            if (!scene.IsActive) return;
            scene.Render();
        }
    }
    public void AwakeScenes()
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            Scene scene = _scenes[i]!;
            if (!scene.IsActive) return;
            
            scene.AwakeScene();
        }
    }
    public void StartScenes()
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            Scene scene = _scenes[i]!;
            if (!scene.IsActive) return;

            scene.StartScene();
        }
    }
    public void UpdateScenes(double dt)
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            Scene scene = _scenes[i]!;
            if (!scene.IsActive) return;

            scene.UpdateScene(dt);
        }
    }

    public void TickScenes(float fixedTimestamp)
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            Scene scene = _scenes[i]!;
            if (!scene.IsActive) return;

            scene.Tick(fixedTimestamp);
        }
    }
}