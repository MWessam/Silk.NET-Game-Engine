using LunarEngine.GameObjects;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public class SpriteRendererInspector : IComponentInspector<SpriteRenderer>
{
    public void OnDrawInspector(ref SpriteRenderer component)
    {
        EditorUIEngine.DrawInputDragFloat4UIElement(ref component.Color, "Color");
    }
}