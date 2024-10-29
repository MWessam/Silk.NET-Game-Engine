using LunarEngine.Assets;
using Serilog;

namespace LunarEngine.GameObjects;



public abstract class Component : IComponent
{
    public GameObject GameObject { get; private set; }
    public Transform Transform => GameObject.Transform;
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
    public virtual void FixedUpdate(double fixedDelta)
    {
        
    }
    public virtual void OnDestroy()
    {
        
    }
}