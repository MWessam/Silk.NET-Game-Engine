using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using Serilog;
using Silk.NET.OpenGL;

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
            AddAsset("birb", asset);
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
            @"..\..\..\Resources\birb.jpg",
            "birb"
        );
    #endregion
}

public static class TestTextures
{
    
}