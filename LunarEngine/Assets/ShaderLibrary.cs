using LunarEngine.OpenGLAPI;
using Silk.NET.OpenGL;

namespace LunarEngine.Assets;

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
        new ShaderHandle(gl, @"..\..\..\Resources\shader.vert", @"..\..\..\Resources\shader.frag"),
        "default");
}