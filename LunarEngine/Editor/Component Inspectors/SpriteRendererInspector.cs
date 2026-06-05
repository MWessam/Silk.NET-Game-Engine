using ImGuiNET;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public class SpriteRendererInspector : IComponentInspector<SpriteRenderer>
{
    private AssetManager _assetManager;

    public SpriteRendererInspector(AssetManager assetManager)
    {
        _assetManager = assetManager;
    }

    public void OnDrawInspector(ref SpriteRenderer component)
    {
        EditorUIEngine.DrawInputDragFloat4UIElement(ref component.Color, "Color");
        EditorUIEngine.DrawInputIntUIElement(ref component.Sprite.PPU, "PPU");
        var textures = _assetManager.TextureLibrary.GetAllAssets();
        var shaders = _assetManager.ShaderLibrary.GetAllAssets();
        if (ImGui.BeginListBox("Texture"))
        {
            for (var i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];
                var textureHandle = _assetManager.GetTextureHandle(texture);
                if (ImGui.Selectable($"{texture.Key}##{i}"))
                {
                    component.Sprite.ChangeTexture(textureHandle);
                }
            }
            ImGui.EndListBox();
        }
        if (ImGui.BeginListBox("Shader"))
        {
            for (var i = 0; i < shaders.Count; i++)
            {
                var shader = shaders[i];
                var shaderHandle = _assetManager.GetShaderHandle(shader);
                if (ImGui.Selectable($"{shader.Key}##{i}"))
                {
                    component.Sprite.ChangeShader(shaderHandle);
                }
            }
            ImGui.EndListBox();
        }
    }
}
