using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.ECS;
using LunarEngine.ECS.Components;
using LunarEngine.Renderer;

namespace LunarEngine.ECS.Systems;

public partial class GizmosSystem : ScriptableSystem
{
    public override SystemStage Stage => SystemStage.Update;
    public override int Order => 200;

    private IRenderer _renderer;
    public GizmosSystem(World world, IRenderer renderer) : base(world)
    {
        _renderer = renderer;
    }
    public override void Update(in double data)
    {
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
