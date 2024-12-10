using ImGuiNET;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.Physics;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public class RigidBody2DInspector : IComponentInspector<RigidBody2D>
{
    public void OnDrawInspector(ref RigidBody2D component)
    {
        EditorUIEngine.DrawInputDragFloat2UIElement(ref component.Velocity, "Velocity");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Mass, "Mass");
        EditorUIEngine.DrawInputFloatUIElement(ref component.GravityScale, "GravityScale");
        EditorUIEngine.DrawBoolUIElement(ref component.IsInterpolating, "Interpolation");
        var bodyTypeIndex = -1;
        Enum[] bodyTypes = [EBodyType.Static, EBodyType.Kinematic, EBodyType.Dynamic];
        for (int i = 0; i < bodyTypes.Length; ++i)
        {
            if (Equals(bodyTypes[i], component.BodyType))
            {
                bodyTypeIndex = i;
                break;
            }
        }
        if (EditorUIEngine.DrawEnumUIElement(ref bodyTypeIndex, "BodyType", bodyTypes))
        {
            component.BodyType = (EBodyType)bodyTypes[bodyTypeIndex];
        }
    }
}
public class BoxCollider2DInspector : IComponentInspector<BoxCollider2D>
{
    public void OnDrawInspector(ref BoxCollider2D component)
    {
        EditorUIEngine.DrawInputDragFloat2UIElement(ref component.Position, "Position");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Width, "Width");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Height, "Height");
    }
}