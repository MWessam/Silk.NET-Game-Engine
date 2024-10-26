using System.Numerics;
using LunarEngineCS.OpenGLAPI;

namespace LunarEngineCS.GameObjects;

public class Scene
{
    public Camera Camera { get; private set; }
    public List<GameObject> GameObjects = new();
    private List<ShaderHandle> _shaders = new();
    private List<TextureHandle> _textures = new();

    public static Scene CreateScene()
    {
        var scene = new Scene();
        var cameraObject = GameObject.CreateGameObject("Camera");
        scene.Camera = new Camera();
        cameraObject.AddComponent(scene.Camera);
        scene.Camera.Transform.LocalPosition = new Vector3(0.0f, 0.0f, -1.0f);
        scene.Camera.Width = 5;
        scene.Camera.Height = 5;
        scene.Camera.Near = 0.1f;
        scene.Camera.Far = 1000.0f;
        scene.GameObjects.Add(cameraObject);
        return scene;
    }

    public void AddObject(GameObject gameObject)
    {
        GameObjects.Add(gameObject);
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
    private Scene() {}
}