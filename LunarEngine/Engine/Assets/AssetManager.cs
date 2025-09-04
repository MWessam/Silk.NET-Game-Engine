using System.Numerics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using Silk.NET.OpenGL;

namespace LunarEngine.Assets;
public class AssetManager : IDisposable
{
    public ShaderLibrary ShaderLibrary;
    public TextureLibrary TextureLibrary;

    public void OnRendererInitialized()
    {
        
    }
    public void Initialize()
    {
        ShaderLibrary = ShaderLibrary
            .CreateLibraryBuilder<ShaderLibrary>()
            .Build();
        TextureLibrary = TextureLibrary
            .CreateLibraryBuilder<TextureLibrary>()
            .Build();
    }
    public void Dispose()
    {
        // TODO release managed resources here
    }
}