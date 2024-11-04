using System.Numerics;
using LunarEngine.Graphics;
using Serilog;

namespace LunarEngine.GameObjects;

public struct View
{
}
public struct Projection
{
}
public struct Camera
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;
    public float Width;
    public float Height;
    public float Near;
    public float Far;
    
    // TODO: Remove from here.
    // public Vector3 ScreenPointToWorldPoint(Vector2 screenPoint)
    // {
    //     // Convert screen point to normalized device coordinates (NDC)
    //     float x = (2.0f * screenPoint.X) / GraphicsEngine.WindowResolution.X - 1.0f;
    //     float y = 1.0f - (2.0f * screenPoint.Y) / GraphicsEngine.WindowResolution.Y; // Flip y-coordinate
    //
    //     // Create clip coordinates
    //     Vector4 clipCoords = new Vector4(x, y, -1.0f, 1.0f);
    //
    //     // Convert to view space
    //     if (!(Matrix4x4.Invert(Projection, out var invertedProjection) && Matrix4x4.Invert(View, out var invertedView)))
    //     {
    //         Log.Error($"Projection or view matrix were NOT a square matrix. Couldn't invert.");
    //         return Vector3.Zero;
    //     }
    //     Vector4 viewCoords = Vector4.Transform(clipCoords, invertedProjection);
    //
    //     // Convert to world space
    //     Vector4 worldCoords = Vector4.Transform(viewCoords, invertedView);
    //
    //     // Return the world position, ignoring the w component
    //     return new Vector3(worldCoords.X, worldCoords.Y, worldCoords.Z);
    // }
}