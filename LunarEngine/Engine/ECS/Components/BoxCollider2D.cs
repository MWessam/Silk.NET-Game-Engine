using System.Numerics;

namespace LunarEngine.Engine.ECS.Components;

public struct BoxCollider2D : IComponent
{
    public Vector2 Position;  // The center of the box (you could also use top-left if preferred)
    public float Width;
    public float Height;

    // Gets the min and max values for the AABB
    //TODO: Figure out WHY TF DOES NOT DIVIDING IT BY 2 WORK??
    public float MinX => Position.X - Width;
    public float MaxX => Position.X + Width;
    public float MinY => Position.Y - Height;
    public float MaxY => Position.Y + Height;
}