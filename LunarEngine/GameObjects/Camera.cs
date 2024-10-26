using System.Numerics;

namespace LunarEngine.GameObjects;

public class Camera : Component
{
    private Matrix4x4 _view;
    private Matrix4x4 _projection;
    public float Width { get; set; }
    public float Height { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }
    public Matrix4x4 View
    {
        get
        {
            _view = Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);
            if (Transform.IsDirty)
            {
            }
            return _view;
        }
    }

    public Matrix4x4 Projection
    {
        get
        {
            _projection = Matrix4x4.CreateOrthographic(Width, Height, Near, Far);
            return _projection;
        }
    }
    public Matrix4x4 ViewProjection => View * Projection;
}