using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;

namespace LunarEngine.Engine.ECS.Systems;

public partial class GizmosSystem : ScriptableSystem
{
    public GizmosSystem(World world) : base(world)
    {
    }
    public override void Update(in double data)
    {
        CommandBuffer = new CommandBuffer();
        RenderOutlineQuery(World);
        CommandBuffer.Playback(World);
    }
    [Query]
    [All<SpriteRenderer, Scale, Position>]
    public void RenderOutline(ref SpriteRenderer spriteRenderer, ref Scale scale, ref Position position)
    {
        Renderer.Instance.SubmitRenderCommand(new QuadDrawCommand(position.Value, scale.ActualValue, Vector4.One));
    }
}