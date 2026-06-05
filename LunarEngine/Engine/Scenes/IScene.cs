using LunarEngine.ECS;
using LunarEngine.Engine.Graphics;

namespace LunarEngine.Scenes;

public interface IScene
{
    string Name { get; }
    IWorld World { get; }
    SystemScheduler Scheduler { get; }
    bool IsActive { get; set; }

    void Awake();
    void Start();
    void Update(double deltaTime);
    void FixedUpdate(double fixedDeltaTime);
    void Render(IRenderer renderer);
    void Dispose();
}
