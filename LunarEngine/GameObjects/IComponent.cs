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