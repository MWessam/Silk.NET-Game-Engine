using Silk.NET.OpenGL;
using StbImageSharp;

namespace LunarEngine.OpenGLAPI;

public class TextureHandle : IDisposable
    {
        private uint _handle;
        private GL _gl;

        public unsafe TextureHandle(GL gl, string path)
        {
            _gl = gl;
            _handle = _gl.GenTexture();
            Bind();
            
            // Load the image from memory.
            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
            
            fixed (byte* ptr = result.Data)
            {
                // Create our texture and upload the image data.
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) result.Width, 
                    (uint) result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }

            SetParameters();
        }

        public unsafe TextureHandle(GL gl, Span<byte> data, uint width, uint height)
        {
            //Saving the gl instance.
            _gl = gl;

            //Generating the opengl handle;
            _handle = _gl.GenTexture();
            Bind();

            //We want the ability to create a texture using data generated from code aswell.
            fixed (void* d = &data[0])
            {
                //Setting the data of a texture.
                _gl.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                SetParameters();
            }
        }

        private void SetParameters()
        {
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.LinearMipmapLinear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public void Dispose()
        {
            _gl.DeleteTexture(_handle);
        }

    }