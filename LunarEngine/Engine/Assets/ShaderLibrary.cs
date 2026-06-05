using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
using Serilog;

namespace LunarEngine.Assets;

public class ShaderLibrary : BaseAssetLibrary<ShaderAsset>
{
    private ShaderLibrary() : base()
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
            asset = BasicShader();
            return asset;
        }
    }
    public ShaderAsset CreateShader(string name, string vertexPath, string fragPath)
    {
        var shader = new ShaderAsset(
            vertexPath, 
            fragPath,
            name
        );
        if (!AddAsset(name, shader))
        {
            Log.Error($"Couldn't save shader {name}. A shader with that name already exists.");
        }
        return shader;
    }

    #region TEST

    public ShaderAsset BasicShader() => new(
        AssetProvider!.ResolvePath(new AssetKey("shader", "shader.vert")),
        AssetProvider!.ResolvePath(new AssetKey("shader", "shader.frag")),
        "default");

    #endregion
}