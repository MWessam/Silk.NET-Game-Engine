using LunarEngine.Renderer.OpenGL;
using Serilog;

namespace LunarEngine.Assets;

public class TextureLibrary : BaseAssetLibrary<TextureAsset>
{
    private TextureLibrary() : base()
    {
        
    }
    public override TextureAsset DefaultAsset
    {
        get
        {
            if (TryGetAsset("birb", out var asset))
            {
                return asset;
            }
            asset = BirbTexture();
            return asset;
        }
    }
    public TextureAsset CreateTexture(string name, string path)
    {
        var texture = new TextureAsset(
            path,
            name
        );
        if (!AddAsset(name, texture))
        {
            Log.Error($"Couldn't save texture {name} as a texture with that name already exists.");
        }
        
        return texture;
    }
    #region TEST
    public TextureAsset BirbTexture() =>
        new(
            AssetProvider!.ResolvePath(new AssetKey("texture", "birb.jpg")),
            "birb"
        );
    #endregion
}

public static class TestTextures
{
    
}