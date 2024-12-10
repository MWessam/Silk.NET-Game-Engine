using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;

namespace LunarEngine.GameEngine;

public partial class ShaderSystem : ScriptableSystem
{
    public ShaderSystem(World world) : base(world)
    {
    }
    public override void Awake()
    {
    }


}
public struct AssignShaderEvent
{
    public Sprite Sprite;
    public string ShaderName;
}
public struct TagComponent : IComponent
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