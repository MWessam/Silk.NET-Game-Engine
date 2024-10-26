using LunarEngineCS.OpenGLAPI;

namespace RenderingEngine;

public class TextureAsset : IAsset
{
    public TextureHandle Texture;
    public string TextureName;
    public TextureAsset(TextureHandle texture, string textureName)
    {
        Texture = texture;
        TextureName = textureName;
    }

    public string Key => TextureName;
}