using ImGuiNET;
using LunarEngine.Assets;
using LunarEngine.Engine.AssetHandleCache;
using LunarEngine.GameObjects;
using LunarEngine.UI;
using Silk.NET.OpenGL;

namespace LunarEngine.ECS.Systems;

public class SpriteRendererInspector : IComponentInspector<SpriteRenderer>
{
    private AssetManager _assetManager;
    private IAssetHandleCache _assetHandleCache;
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
                var textureHandle = _assetHandleCache.GetTextureHandle(texture);
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
                var shaderHandle = _assetHandleCache.GetShaderHandle(shader);
                if (ImGui.Selectable($"{shader.Key}##{i}"))
                {
                    component.Sprite.ChangeShader(shaderHandle);
                }
            }
            ImGui.EndListBox();
        }
    }
}