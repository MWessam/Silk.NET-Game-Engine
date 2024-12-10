using ImGuiNET;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public class SpriteRendererInspector : IComponentInspector<SpriteRenderer>
{
    public void OnDrawInspector(ref SpriteRenderer component)
    {
        EditorUIEngine.DrawInputDragFloat4UIElement(ref component.Color, "Color");
        var textures = AssetManager.Instance.TextureLibrary.GetAllAssets();
        var shaders = AssetManager.Instance.ShaderLibrary.GetAllAssets();
        if (ImGui.BeginListBox("Texture"))
        {
            for (var i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];
                if (ImGui.Selectable($"{texture.TextureName}##{i}"))
                {
                    component.Sprite.ChangeTexture(texture.Texture);
                }
            }
            ImGui.EndListBox();
        }
        if (ImGui.BeginListBox("Shader"))
        {
            for (var i = 0; i < shaders.Count; i++)
            {
                var shader = shaders[i];
                if (ImGui.Selectable($"{shader.ShaderName}##{i}"))
                {
                    component.Sprite.ChangeShader(shader.Shader);
                }
            }
            ImGui.EndListBox();
        }
    }
}