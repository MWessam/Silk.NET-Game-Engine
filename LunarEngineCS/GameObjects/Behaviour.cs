namespace LunarEngineCS.GameObjects;

public interface IComponent
{
    GameObject GameObject { get; }
    Transform Transform { get; }
    void AssignGameObject(GameObject gameObject);
    void Awake();
    void Start();
    void OnEnable();
    void OnDisable();
    void Update(float delta);
    void FixedUpdate(float fixedDelta);
    void OnDestroy();
}

public abstract class Component : IComponent
{
    public GameObject GameObject { get; private set; }
    public Transform Transform => GameObject.Transform;
    protected Component()
    {
        
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
    public virtual void Update(float delta)
    {
        
    }
    public virtual void FixedUpdate(float fixedDelta)
    {
        
    }
    public virtual void OnDestroy()
    {
        
    }
}