# Lunar Engine #

A Silk.NET Game Engine.

## Engine Architecture/Structure:
The Engine is split into multiple modules.

Game Engine Module: The main module and entry point of our engine. It contains references to all other modules and coordinates them together.

Graphics Engine Module: Graphcis rendering and windowing. Handles all the draw calls and low level graphics apis. Expects a span of renderer objects.

Input: Basic input mapping system. Handles key press,release,held and mouse press,release,held,move,scroll events. Static global that anyone can subscribe/unsubscibe to.

SceneManager: Container of all scenes. Stores any new scene and loads/unloads other scenes. Also support additive scene loading.

Scene: Container of all game objects. Handles their lifecycle and is able to add/remove objects at will.

Game Object: Entity in scene that contains multiple components. Handles each component life cycle. Contains a transform component and a reference to the parent scene.

Component: Customizable component that can pretty much do anything. Has reference to game object and its transform and scene.

AssetLibrary: Contains assets that you can look up with their name. Current assets are shaders and textures.

AssetManager: Contains all asset libraries.

Physics: Handles physics with a fixed tick rate. Contains a reference to all rigid bodies in the scene and updates their data according to physics simulation.
## 

## Get Started
Clone ``` https://github.com/MWessam/Silk.NET-Game-Engine.git ```



Create objects using:
```csharp
GameObject.CreateGameObject(name, scene, params Type[] types)
```
This will automatically create an object and add it to the scene with the components specified.

Modify my test scene or extend your own scene.

Add all objects you need to add in the scene's constructor.

```csharp
public class TestScene : Scene
{
    public TestScene()
    {
        // Create some objects.
        var cameraController = GameObject.CreateGameObject($"Camera Controller",
            this
        );
        var birb = GameObject.CreateGameObject($"birb",
            this,
            typeof(SpriteRenderer),
            // typeof(Rigidbody2D),
            typeof(CameraController)

        );
        var spawnerObject = GameObject.CreateGameObject("spawner",
            this,
            typeof(ObjectSpawner));
        spawnerObject.GetComponent<ObjectSpawner>().InjectObjectToSpawn(birb);

        // Activate Scene
        IsActive = true;
    }
}
```

Create Custom Components:

Extend from component class and override the life cycle methods u need.
MAKE SURE TO ONLY USE THIS CONSTRUCTOR! Don't add your own constructor, it won't work.
```csharp
public class ObjectSpawner : Component
{
    private GameObject ObjectToSpawn;
    public ObjectSpawner(GameObject gameObject) : base(gameObject)
    {
    }

    public void InjectObjectToSpawn(GameObject gameObject)
    {
        ObjectToSpawn = gameObject;
    }

    public override void OnEnable()
    {
        Input.Instance.AddKeyDownListener(Key.K, OnKPressed);
    }

    public void OnKPressed(Key key)
    {
        var obj = Instantiate(ObjectToSpawn);
        obj.Transform.Position = Vector3.Zero;
    }
}
```

