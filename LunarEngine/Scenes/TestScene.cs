using Arch.Core;
using LunarEngine.Assets;
using LunarEngine.GameObjects;

namespace LunarEngine.Scenes;

public class TestScene : Scene
{
    public TestScene()
    {
        using var world = World.Create();
        var cameraController = GameObject.CreateGameObject($"Camera Controller",
        this
        );
        var birb = GameObject.CreateGameObject($"birb",
            this,
            typeof(SpriteRenderer)
            // typeof(CustomBehaviour),
            // typeof(Rigidbody2D)
        );
        var spawnerObject = GameObject.CreateGameObject("spawner",
        this,
        typeof(ObjectSpawner));
        var waterTexture = AssetManager.TextureLibrary.CreateTexture("water", @"..\..\..\Resources\water.png");
        var waterShader = AssetManager.ShaderLibrary.CreateShader("water", @"..\..\..\Resources\shader.vert", @"..\..\..\Resources\shader.frag");
        var spriteRenderer = birb.GetComponent<SpriteRenderer>();
        spriteRenderer.Sprite.ChangeTexture(waterTexture.Texture);
        spriteRenderer.Sprite.ChangeShader(waterShader.Shader);
        spawnerObject.GetComponent<ObjectSpawner>().InjectObjectToSpawn(birb);
        IsActive = true;
    }

    private void ForEach(Entity entity, ref Position pos, ref Velocity velocity)
    {
        pos.x += velocity.dx;
    }
}