using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
using Serilog;
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
        var shader = new ShaderAsset(
            new ShaderHandle(Renderer.Instance.Api, vertexPath, fragPath),
            name
        );
        if (!AddAsset(name, shader))
        {
            Log.Error($"Couldn't save texture {name} as a texture with that name already exists.");
        }
        return shader;
    }
}

public static class TestShaders
{
    public static ShaderAsset BasicShader() => new(
        new ShaderHandle(Renderer.Instance.Api, @"..\..\..\Resources\shader.vert", @"..\..\..\Resources\shader.frag"),
        "default");
}