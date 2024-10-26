using System.Numerics;
using Silk.NET.Input;

namespace LunarEngine.GameObjects;

public interface IComponent
{
    GameObject GameObject { get; }
    Transform Transform { get; }
    void AssignGameObject(GameObject gameObject);
    void Awake();
    void Start();
    void OnEnable();
    void OnDisable();
    void Update(double delta);
    void FixedUpdate(double fixedDelta);
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
public class CustomBehaviour : Component
{
    public override void Awake()
    {
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.W, OnWPressed); 
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.A, OnAPressed); 
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.S, OnSPressed); 
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.D, OnDPressed); 
    }
    private void OnWPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(0.0f, 0.1f, 0.0f);
    }
    private void OnAPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(0.1f, 0.0f, 0.0f);
    }
    private void OnSPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(0.0f, -0.1f, 0.0f);
    }
    private void OnDPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(-0.1f, 0.0f, 0.0f);
    }
}