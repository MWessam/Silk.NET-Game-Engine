using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
using Silk.NET.OpenGL;

namespace LunarEngine.Assets;

public class TextureLibrary : BaseAssetLibrary<TextureAsset>
{
    private TextureLibrary() : base()
    {
        
    }

    private TextureLibrary(GL gl) : base(gl)
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
            asset = TestTextures.BirbTexture(); 
            AddAsset("birb", asset);
            return asset;
        }
    }

    public TextureAsset CreateTexture(string name, string path)
    {
        var texture = new TextureAsset(
            new TextureHandle(GraphicsEngine.Api, path),
            name
        );
        return texture;
    }
}

public static class TestTextures
{
    public static TextureAsset BirbTexture() =>
        new(
            new TextureHandle(GraphicsEngine.Api, @"..\..\..\Resources\birb.jpg"),
            "birb"
            );
}