using System.Numerics;
using LunarEngine.Graphics;
using Serilog;

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

    public Camera(GameObject gameObject) : base(gameObject)
    {
        Transform.LocalPosition = new Vector3(0.0f, 0.0f, -1.0f);
        Width = 5;
        Height = 5;
        Near = 0.1f;
        Far = 1000.0f;
    }

    public Vector3 ScreenPointToWorldPoint(Vector2 screenPoint)
    {
        // Convert screen point to normalized device coordinates (NDC)
        float x = (2.0f * screenPoint.X) / GraphicsEngine.WindowResolution.X - 1.0f;
        float y = 1.0f - (2.0f * screenPoint.Y) / GraphicsEngine.WindowResolution.Y; // Flip y-coordinate

        // Create clip coordinates
        Vector4 clipCoords = new Vector4(x, y, -1.0f, 1.0f);

        // Convert to view space
        if (!(Matrix4x4.Invert(Projection, out var invertedProjection) && Matrix4x4.Invert(View, out var invertedView)))
        {
            Log.Error($"Projection or view matrix were NOT a square matrix. Couldn't invert.");
            return Vector3.Zero;
        }
        Vector4 viewCoords = Vector4.Transform(clipCoords, invertedProjection);

        // Convert to world space
        Vector4 worldCoords = Vector4.Transform(viewCoords, invertedView);

        // Return the world position, ignoring the w component
        return new Vector3(worldCoords.X, worldCoords.Y, worldCoords.Z);
    }
    
}