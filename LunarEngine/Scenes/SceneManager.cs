using System.Collections;
using Serilog;

namespace LunarEngine.Scenes;

public class SceneManager
{
    // Max scene count 16.
    private const int MAX_SCENE_COUNT = 16;
    private ECSScene?[] _scenes = new ECSScene[MAX_SCENE_COUNT];
    private int _lastSceneIndex = -1;
    private int _activeSceneIndex = -1;
    public ECSScene ActiveScenes => _scenes[_activeSceneIndex];
    public void AddScene(ECSScene scene)
    {
        if (_lastSceneIndex >= 15)
        {
            Log.Error($"Scenes are already full! Can't add more scenes.");
            return;
        }
        _scenes[++_lastSceneIndex] = scene;
        scene.SceneId = _lastSceneIndex;
        _activeSceneIndex = _lastSceneIndex;
    }
    public ECSScene? RemoveScene(int sceneId)
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
    public void RemoveScene(ECSScene scene)
    {
        
    }
    public void AwakeScenes()
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            var scene = _scenes[i]!;
            if (!scene.IsActive) return;
            
            scene.Awake();
        }
    }
    public void StartScenes()
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            var scene = _scenes[i]!;
            if (!scene.IsActive) return;

            scene.Start();
        }
    }
    public void UpdateScenes(double dt)
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            var scene = _scenes[i]!;
            if (!scene.IsActive) return;

            // scene.Update((float)dt);
        }
    }
    public void TickScenes(double fixedTimestamp)
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            var scene = _scenes[i]!;
            if (!scene.IsActive) return;

            scene.Tick(fixedTimestamp);
        }
    }
}