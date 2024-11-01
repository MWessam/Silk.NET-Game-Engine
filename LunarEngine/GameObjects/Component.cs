using LunarEngine.Assets;
using LunarEngine.Scenes;
using Serilog;

namespace LunarEngine.GameObjects;



public abstract class Component : IComponent
{
    public GameObject GameObject { get; private set; }
    public Transform Transform => GameObject.Transform;
    public Scene ParentScene => GameObject.ParentScene;
    public Component(GameObject gameObject)
    {
        GameObject = gameObject;
    }
    public void AssignGameObject(GameObject gameObject)
    {
        GameObject = gameObject;
    }

    public virtual void Awake()
    {
        
    }
    public virtual void Start()
    {
        
    }
    public virtual void OnEnable()
    {
        
    }
    public virtual void OnDisable()
    {
        
    }
    public virtual void Update(double delta)
    {
        
    }
    public virtual void Tick(float delta)
    {
        
    }
    public virtual void OnDestroy()
    {
        
    }

    public T AddComponent<T>() where T : IComponent => GameObject.AddComponent<T>();
    public T AddComponent<T>(T component) where T : IComponent => GameObject.AddComponent<T>(component);
    public T GetComponent<T>() where T : IComponent => GameObject.GetComponent<T>();
    public GameObject Instantiate(GameObject gameObject) => GameObject.Instantiate(gameObject);

    public virtual void Clone(IComponent component)
    {
    }
}