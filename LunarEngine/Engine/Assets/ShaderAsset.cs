using LunarEngine.Engine.Graphics;

namespace LunarEngine.Assets;

public class ShaderAsset : IAsset
{
    private readonly string _vertexPath;
    private readonly string _fragPath;
    private ShaderHandle? _handle;

    private string _name;
    public string Key => _name;
    public string VertexPath => _vertexPath;
    public string FragPath => _fragPath;
    
    public ShaderAsset(string vertexPath, string fragPath, string name)
    {
        _vertexPath = vertexPath;
        _fragPath = fragPath;
        _name = name;
    }
}