using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.OpenGLAPI;

namespace LunarEngine.Scenes;

public class Scene
{
    internal int SceneId;
    public Camera Camera { get; private set; }
    public bool IsActive { get; set; }

    public List<GameObject> GameObjects = new();
    private List<ShaderHandle> _shaders = new();
    private List<TextureHandle> _textures = new();
    private LinkedList<GameObject> _addObjectBuffer = new();
    private bool _isInitialized;

    public void AddObject(GameObject gameObject)
    {
        foreach (var component in gameObject)
        {
            if (component is SpriteRenderer spriteRenderer)
            {
                AddShader(spriteRenderer.Sprite.Shader);
                AddTexture(spriteRenderer.Sprite.Texture);
            }
        }
        if (_isInitialized)
        {
            if (gameObject.Enabled)
            {
                gameObject.Awake();
                gameObject.OnEnable();
                gameObject.Start();
            }
            _addObjectBuffer.AddFirst(gameObject);

        }
        else
        {
            GameObjects.Add(gameObject);
        }
    }
    public void AddShader(ShaderHandle shaderHandle)
    {
        if (_shaders.Any(x => x.Handle == shaderHandle.Handle))
        {
            return;
        }
        shaderHandle.SetUniform("vp", Camera.ViewProjection);
        _shaders.Add(shaderHandle);
    }
    public void AddTexture(TextureHandle textureHandle)
    {
        if (_textures.Any(x => x.Handle == textureHandle.Handle))
        {
            return;
        }
        _textures.Add(textureHandle);
    }
    public void UpdateViewProjectionUniforms()
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

    public virtual void AwakeScene()
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

        _isInitialized = true;
    }
    public void UpdateScene(double dt)
    {
        foreach (var gameObject in GameObjects)
        {
            gameObject.Update(dt);
        }
        GameObjects.AddRange(_addObjectBuffer);
        _addObjectBuffer.Clear();
    }

    public Scene()
    {
        var cameraObject = GameObject.CreateGameObject("Camera", this, typeof(Camera));
        Camera = cameraObject.GetComponent<Camera>()!;
    }

    public void Render()
    {
        UpdateViewProjectionUniforms();
        UpdateDirtyShaderUniforms();
        var objectSpan = CollectionsMarshal.AsSpan(GameObjects);
        GraphicsEngine.Render(objectSpan);
    }

    public T? FindObjectOfType<T>() where T : IComponent
    {
        foreach (var obj in GameObjects)
        {
            if (obj.TryGetComponent(out T component))
            {
                return component;
            }
        }
        return default;
    }

    public void Tick(float fixedTimestamp)
    {
        foreach (var gameObject in GameObjects)
        {
            gameObject.Tick(fixedTimestamp);
        }
    }
}