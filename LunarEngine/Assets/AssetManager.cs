using System.Numerics;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using Silk.NET.OpenGL;

namespace LunarEngine.Assets;
public static class AssetManager
{
    public static ShaderLibrary ShaderLibrary;
    public static TextureLibrary TextureLibrary;
    public static void InitializeAssetManager()
    {
        ShaderLibrary = ShaderLibrary
            .CreateLibraryBuilder<ShaderLibrary>()
            .WithAsset("default", TestShaders.BasicShader())
            .Build();
        TextureLibrary = TextureLibrary
            .CreateLibraryBuilder<TextureLibrary>()
            .WithAsset("birb", TestTextures.BirbTexture())
            .Build();
    }
}