using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
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
            asset = TestShaders.BasicShader();
            AddAsset("default", asset);
            return asset;
        }
    }
    public ShaderAsset CreateShader(string name, string vertexPath, string fragPath)
    {
        var texture = new ShaderAsset(
            new ShaderHandle(GraphicsEngine.Api, vertexPath, fragPath),
            name
        );
        return texture;
    }
}

public static class TestShaders
{
    public static ShaderAsset BasicShader() => new(
        new ShaderHandle(GraphicsEngine.Api, @"..\..\..\Resources\shader.vert", @"..\..\..\Resources\shader.frag"),
        "default");
}