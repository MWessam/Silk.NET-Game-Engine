using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;

namespace LunarEngine.Assets;

public class TextureAsset : IAsset
{
    public LGTexture Texture;
    public string TextureName;
    public TextureAsset(LGTexture texture, string textureName)
    {
        Texture = texture;
        TextureName = textureName;
    }

    public string Key => TextureName;
}