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
    private Renderer _renderer;
    public GizmosSystem(World world, Renderer renderer) : base(world)
    {
        _renderer = renderer;
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
        _renderer.SubmitRenderCommand(new QuadDrawCommand(position.Value, scale.ActualValue, Vector4.One));
    }
}