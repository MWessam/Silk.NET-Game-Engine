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
            LGTexture.CreateTexture(Renderer.Instance.Api, path),
            name
        );
        if (!AddAsset(name, texture))
        {
            Log.Error($"Couldn't save texture {name} as a texture with that name already exists.");
        }
        
        return texture;
    }
}

public static class TestTextures
{
    public static TextureAsset BirbTexture() =>
        new(
            LGTexture.CreateTexture(Renderer.Instance.Api, @"..\..\..\Resources\birb.jpg"),
            "birb"
            );
}