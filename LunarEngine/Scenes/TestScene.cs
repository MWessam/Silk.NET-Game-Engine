using LunarEngine.GameObjects;
using LunarEngine.Physics;

namespace LunarEngine.Scenes;

public class TestScene : Scene
{
    public TestScene()
    {
        var gameObject = GameObject.CreateGameObject($"obj",
            this,
            typeof(SpriteRenderer),
            typeof(CustomBehaviour)
        );
        var spawnerObject = GameObject.CreateGameObject("spawner",
            this,
            typeof(ObjectSpawner));
        spawnerObject.GetComponent<ObjectSpawner>().InjectObjectToSpawn(gameObject);
        IsActive = true;
    }
}