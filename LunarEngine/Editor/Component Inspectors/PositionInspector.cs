using System.Numerics;
using LunarEngine.ECS.Components;
using LunarEngine.UI;
using LunarEngine.Utilities;

namespace LunarEngine.Editor.Systems;

public class PositionInspector : IComponentInspector<Position>
{
    public void OnDrawInspector(ref Position component)
    {
        var value = component.Value;
        EditorUIEngine.DrawInputDragFloat3UIElement(ref value, "##position");
        component.Value = value;

    }
}
public class RotationInspector : IComponentInspector<Rotation>
{
    public void OnDrawInspector(ref Rotation component)
    {
        var rotationAngles = component.Value.ToEulerAngles().RadianToDegree();
        EditorUIEngine.DrawInputDragFloat3UIElement(ref rotationAngles, "##rotation");
        component.Value = rotationAngles.DegreeToRadian().ToQuaternion();
    }
}

public class ScaleInspector : IComponentInspector<Scale>
{
    public void OnDrawInspector(ref Scale component)
    {
        var value = component.UserValue;
        EditorUIEngine.DrawInputDragFloat3UIElement(ref value, "##scale");
        component.UserValue = value;
    }
}