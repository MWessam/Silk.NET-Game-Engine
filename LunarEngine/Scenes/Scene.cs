using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.OpenGLAPI;
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
            scene.ResetShadersVP();
        }
    }

    public void RenderScenes()
    {
        for (var i = 0; i <= _lastSceneIndex; i++)
        {
            Scene scene = _scenes[i]!;
            if (!scene.IsActive) return;
            scene.UpdateDirtyShaderUniforms();
            var objectSpan = CollectionsMarshal.AsSpan(scene.GameObjects);
            GraphicsEngine.Render(objectSpan);
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
}
public class Scene
{
    internal int SceneId;
    public Camera Camera { get; private set; }
    public bool IsActive { get; set; }

    public List<GameObject> GameObjects = new();
    private List<ShaderHandle> _shaders = new();
    private List<TextureHandle> _textures = new();

    public static Scene CreateScene()
    {
        var scene = new Scene();
        var cameraObject = GameObject.CreateGameObject("Camera", typeof(Camera));
        scene.Camera = cameraObject.GetComponent<Camera>();
        scene.AddObject(cameraObject);
        return scene;
    }

    public void AddObject(GameObject gameObject)
    {
        GameObjects.Add(gameObject);
        foreach (var component in gameObject)
        {
            if (component is SpriteRenderer spriteRenderer)
            {
                AddShader(spriteRenderer.Sprite.Shader);
                AddTexture(spriteRenderer.Sprite.Texture);
            }
        }
    }
    public void AddShader(ShaderHandle shaderHandle)
    {
        shaderHandle.SetUniform("vp", Camera.ViewProjection);
        _shaders.Add(shaderHandle);
    }
    public void AddTexture(TextureHandle textureHandle)
    {
        _textures.Add(textureHandle);
    }
    public void ResetShadersVP()
    {
        foreach (var shader in _shaders)
        {
            shader.SetUniform("vp", Camera.ViewProjection);
        }
    }
    public void UpdateDirtyShaderUniforms()
    {
        foreach (var shader in _shaders)
        {
            shader.UpdateDirtyUniforms();
        }
    }

    public void AwakeScene()
    {
        foreach (var gameObject in GameObjects)
        {
            gameObject.Awake();
        }
    }
    public void StartScene()
    {
        foreach (var gameObject in GameObjects)
        {
            gameObject.Start();
        }
    }
    public void UpdateScene(double dt)
    {
        foreach (var gameObject in GameObjects)
        {
            gameObject.Update(dt);
        }
    }
    private Scene() {}
}