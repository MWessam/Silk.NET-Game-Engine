using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
using Serilog;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace LunarEngine.GameObjects;
public struct SpriteRenderer : IComponent
{
    public Sprite? Sprite;
    public Vector4 Color;
}
public struct LGShader : IComponent
{
    public ShaderHandle Value;
}

public class LGTexture : IDisposable
{
    private TextureHandle _handle;
    public byte[] Pixels;
    public int Width;
    public int Height;
    public static LGTexture CreateTexture(GL api, string path)
    {
        try
        {
            // Load the image from memory.
            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
            var texture = new LGTexture();
            texture.Width = result.Width;
            texture.Height = result.Height;
            texture.Pixels = result.Data;
            texture._handle = new TextureHandle(api, texture.Pixels.AsSpan(), (uint)texture.Width, (uint)texture.Height);
            return texture;
        }
        catch (Exception e)
        {
            Log.Error($"Couldn't create an image from path {path}");
            var texture = new LGTexture();
            texture.Width = 1;
            texture.Height = 1;
            texture.Pixels = [255, 255, 255, 255];
            texture._handle = new TextureHandle(api, texture.Pixels.AsSpan(), (uint)texture.Width, (uint)texture.Height);
            return texture;
        }
    }

    public void Bind()
    {
        _handle.Bind();
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}