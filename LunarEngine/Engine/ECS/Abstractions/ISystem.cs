using LunarEngine.Core;

namespace LunarEngine.ECS;

public interface ISystem
{
    int Order { get; }
    SystemStage Stage { get; }
    void Initialize(IWorld world, ServiceContainer services);
    void Update(double deltaTime);
}

public interface IRenderSystem : ISystem
{
    void Render(double deltaTime);
}
