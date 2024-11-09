using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;

namespace LunarEngine.GameEngine;

public partial class ShaderSystem : ScriptableSystem
{
    public ShaderSystem(World world) : base(world)
    {
        Hook();
    }
    public override void Awake()
    {
        InitShaderQuery(World);
    }
    [Query]
    [All<ShaderComponent, TagComponent, NeedsInitialization>]
    public void InitShader(ref ShaderComponent shaderComponent, ref TagComponent tagComponent)
    {
        shaderComponent.Value = AssetManager.ShaderLibrary.DefaultAsset.Shader;
    }
    [Event(order:0)]
    public void OnViewProjectionUpdated(ViewProjectionEvent evt)
    {
        var shaderQueryDescription = new QueryDescription().WithAll<ShaderComponent>();
        ViewProjectionEvent @event = evt;
        World.Query(shaderQueryDescription, (ref ShaderComponent shaderComponent) =>
        {
            shaderComponent.Value.SetUniform("vp", @event.ViewProjection);
        });
    }
    [Event(order:0)]
    public void AssignShader(AssignShaderEvent evt)
    {
        var shaderQueryDescription = new QueryDescription().WithAll<ShaderComponent, TagComponent>();
        AssignShaderEvent evtCopy = evt;
        World.Query(shaderQueryDescription, (ref ShaderComponent shaderComponent, ref TagComponent tagComponent) =>
        {
            if (tagComponent.Name == evtCopy.ShaderName)
            {
                evtCopy.Sprite.ChangeShader(shaderComponent.Value);
            }
        });
    }
    [Query]
    [All<ShaderComponent>]
    public void UpdateDirtyUniforms(ref ShaderComponent shaderComponent)
    {
        shaderComponent.Value.UpdateDirtyUniforms();
    }
}
public struct AssignShaderEvent
{
    public Sprite Sprite;
    public string ShaderName;
}
public struct TagComponent
{
    public string Name;

    public TagComponent(string tag)
    {
        Name = tag;
    }
}
public struct ViewProjectionEvent
{
    public Matrix4x4 ViewProjection;
}