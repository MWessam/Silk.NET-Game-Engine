using LunarEngineCS.OpenGLAPI;

namespace RenderingEngine;

public class ShaderAsset : IAsset
{
    public ShaderHandle Shader;
    public string ShaderName;
    public ShaderAsset(ShaderHandle shader, string shaderName)
    {
        Shader = shader;
        ShaderName = shaderName;
    }

    public string Key => ShaderName;
}