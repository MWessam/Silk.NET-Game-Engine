using System.Numerics;

namespace LunarEngine.Engine.Graphics;

public interface IShader : IDisposable
{
    void Bind();
    void Unbind();
    void SetUniform(string name, int value);
    void SetUniform(string name, float value);
    void SetUniform(string name, Vector2 value);
    void SetUniform(string name, Vector3 value);
    void SetUniform(string name, Vector4 value);
    void SetUniform(string name, in Matrix4x4 value);
    void UpdateDirtyUniforms();
}
