using System.Collections;
using System.Numerics;
using LunarEngine.Assets;
using Silk.NET.OpenGL;

namespace LunarEngine.Renderer.OpenGL;

public struct ShaderHandle : IDisposable, IBindable, IShader
{
    private uint _handle;
    private GL _gl;
    private ShaderUniformLibrary _uniforms;
    private Queue<Action> _dirtyUniformQueue = new();

    public ShaderHandle(GL api, string vertexPath, string fragPath)
    {
        _gl = api;
        uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragPath);
        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            var infoLog = _gl.GetProgramInfoLog(_handle);
            throw new Exception($"Program failed to link. Vertex='{vertexPath}', Frag='{fragPath}', Error='{infoLog}'");
        }
        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
        _uniforms = new ShaderUniformLibrary(_gl, _handle);

    }


    public uint Handle => _handle;

    public void UpdateDirtyUniforms()
    {
        Bind();
        foreach (var uniform in _dirtyUniformQueue)
        {
            uniform();
        }
        _dirtyUniformQueue.Clear();
    }
    public void SetUniform(string name, int value)
    {
        //Setting a uniform on a shader using a name.
        GL? gl = _gl;
        ShaderUniformLibrary? library = _uniforms;
        _dirtyUniformQueue.Enqueue(() => {gl.Uniform1(library.GetUniform(name), value);});
    }
    public void SetUniform(string name, float value)
    {
        GL? gl = _gl;
        ShaderUniformLibrary? library = _uniforms;
        if (library.TryGetUniform(name, out var uniform))
        {
            _dirtyUniformQueue.Enqueue(() =>
            {
                gl.Uniform1(library.GetUniform(name), value);
            });
        }
    }
    public void SetUniform(string name, Vector2 vec2)
    {
        GL? gl = _gl;
        ShaderUniformLibrary? library = _uniforms;
        _dirtyUniformQueue.Enqueue(() => {gl.Uniform2(library.GetUniform(name), vec2);});
    }
    public void SetUniform(string name, Vector3 vec3)
    {
        GL? gl = _gl;
        ShaderUniformLibrary? library = _uniforms;
        _dirtyUniformQueue.Enqueue(() => {gl.Uniform3(library.GetUniform(name), vec3);});
    }
    public void SetUniform(string name, Vector4 vec4)
    {
        GL? gl = _gl;
        ShaderUniformLibrary? library = _uniforms;
        _dirtyUniformQueue.Enqueue(() => {gl.Uniform4(library.GetUniform(name), vec4);});
    }
    public unsafe void SetUniform(string name, in Matrix4x4 mat4)
    {
        ShaderHandle handle = this;
        Matrix4x4 local = mat4;
        _dirtyUniformQueue.Enqueue(() => { handle._SetUniform(name, local); });
    }

    private unsafe void _SetUniform(string name, Matrix4x4 mat4)
    {
        Bind();
        _gl.UniformMatrix4(_uniforms.GetUniform(name), 1, false, (float*)&mat4);
    }

    public void Dispose()
    {
        Unbind();
        _gl.DeleteProgram(_handle);
    }

    private uint LoadShader(ShaderType type, string path)
    {
        string src = File.ReadAllText(path);
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        string infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }
        return handle;
    }

    public void Bind()
    {
        _gl.UseProgram(_handle);

    }

    public void Unbind()
    {
        _gl.UseProgram(0);
    }

    private class ShaderUniformLibrary(GL gl, uint handle)
    {
        private Dictionary<string, int> _uniforms = new();

        internal int GetUniform(string name)
        {
            if (_uniforms.TryGetValue(name, out var uniform))
            {
                return uniform;
            }
            var uniformToGet = gl.GetUniformLocation(handle, name);
            if (uniformToGet < 0)
            {
                throw new UniformNotFoundException($"Specified uniform {name} was not found");
            }

            return uniformToGet;
        }

        internal bool TryGetUniform(string name, out int uniformToGet)
        {
            uniformToGet = -1;
            if (_uniforms.TryGetValue(name, out uniformToGet))
            {
                return true;
            }

            uniformToGet = gl.GetUniformLocation(handle, name);
            if (uniformToGet < 0)
            {
                return false;
                throw new UniformNotFoundException($"Specified uniform {name} was not found");
            }
            return true;
        }
        private class UniformNotFoundException(string message) : Exception(message);
    }
}
