using System.Numerics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using Silk.NET.OpenGL;

namespace LunarEngine.Assets;
public class AssetManager : Singleton<AssetManager>, ISingletonObject, IDisposable
{
    public ShaderLibrary ShaderLibrary;
    public TextureLibrary TextureLibrary;
    public void InitializeAssetManager()
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
    
    public void InitSingleton()
    {
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}