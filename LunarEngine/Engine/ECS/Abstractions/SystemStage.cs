namespace LunarEngine.ECS;

public enum SystemStage
{
    Awake,
    Start,
    PreUpdate,
    FixedUpdate,
    Update,
    LateUpdate,
    RenderPrepare,
    RenderSubmit
}
