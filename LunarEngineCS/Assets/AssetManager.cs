using System.Numerics;
using LunarEngineCS.GameObjects;
using LunarEngineCS.RenderingEngine;
using LunarEngineCS.RenderingEngine.Assets;
using RenderingEngine;
using Serilog;
using Silk.NET.OpenGL;

namespace LunarEngineCS.Assets;

public class AssetManager
{
    private ShaderLibrary _shaderLibrary;
    private TextureLibrary _textureLibrary;
    private GL _gl;

    public void LoadGLApi(GL gl)
    {
        _gl = gl;
    }
    public void InitializeAssetManager()
    {
        _shaderLibrary = ShaderLibrary
            .CreateLibraryBuilder<ShaderLibrary>()
            .WithAsset("default", TestShaders.BasicShader(_gl))
            .Build(_gl);
        _textureLibrary = TextureLibrary
            .CreateLibraryBuilder<TextureLibrary>()
            .WithAsset("birb", TestTextures.BirbTexture(_gl))
            .Build(_gl);
    }

    public SpriteRenderer CreateSpriteRenderer(string shaderName, string textureName, string spriteName)
    {
        var shader = _shaderLibrary.GetAsset(shaderName).Shader;
        var texture = _textureLibrary.GetAsset(textureName).Texture;
        var sprite = Sprite
            .GetSpriteBuilder()
            .WithApi(_gl)
            .WithShader(shader)
            .WithTexture(texture)
            .Build();
        return new SpriteRenderer(sprite, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
    }
    public SpriteRenderer CreateSpriteRenderer()
    {
        var shader = _shaderLibrary.DefaultAsset.Shader;
        var texture = _textureLibrary.DefaultAsset.Texture;
        var sprite = Sprite
            .GetSpriteBuilder()
            .WithApi(_gl)
            .WithShader(shader)
            .WithTexture(texture)
            .Build();
        return new SpriteRenderer(sprite, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
    }
}