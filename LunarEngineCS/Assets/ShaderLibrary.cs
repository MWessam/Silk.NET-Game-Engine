using System.Collections;
using LunarEngineCS.OpenGLAPI;
using RenderingEngine;
using Silk.NET.OpenGL;

namespace LunarEngineCS.RenderingEngine.Assets;

public class ShaderLibrary : BaseAssetLibrary<ShaderAsset>
{
    private ShaderLibrary() : base()
    {
        
    }

    private ShaderLibrary(GL gl) : base(gl)
    {
        
    }

    public override ShaderAsset DefaultAsset
    {
        get
        {
            if (TryGetAsset("default", out var asset))
            {
                return asset;
            }
            asset = TestShaders.BasicShader(Gl);
            AddAsset("default", asset);
            return asset;
        }
    }
}

public static class TestShaders
{
    public static ShaderAsset BasicShader(GL gl) => new(
        new ShaderHandle(gl, @"D:\_Keep\Active Projects\LunarEngineCS\LunarEngineCS\Resources\shader.vert", @"D:\_Keep\Active Projects\LunarEngineCS\LunarEngineCS\Resources\shader.frag"),
        "default");
}