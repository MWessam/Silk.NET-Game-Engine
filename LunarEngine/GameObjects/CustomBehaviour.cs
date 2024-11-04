// using System.Numerics;
// using LunarEngine.GameEngine;
// using LunarEngine.InputEngine;
// using LunarEngine.Physics;
// using Serilog;
// using Silk.NET.Input;
//
// namespace LunarEngine.GameObjects;
//
// public class ObjectSpawner : Component
// {
//     private GameObject ObjectToSpawn;
//     private Vector2 _mousePos;
//     public ObjectSpawner(GameObject gameObject) : base(gameObject)
//     {
//     }
//
//     public void InjectObjectToSpawn(GameObject gameObject)
//     {
//         ObjectToSpawn = gameObject;
//     }
//
//     public override void OnEnable()
//     {
//         Input.Instance.AddKeyDownListener(Key.K, OnKPressed);
//         Input.Instance.OnMouseMoved += OnMouseMoved;
//     }
//
//     private void OnMouseMoved(Vector2 obj)
//     {
//         _mousePos = obj;
//     }
//     public void OnKPressed(Key key)
//     {
//         var obj = Instantiate(ObjectToSpawn);
//         Vector3 screenPointToWorldPoint = ParentScene.Camera.ScreenPointToWorldPoint(_mousePos);
//         obj.GetComponent<Rigidbody2D>().OverridePosition(screenPointToWorldPoint with {Z = 0.0f});
//     }
// }
//
// public class CustomBehaviour : Component
// {
//     private float _speed = 100.0f;
//     private float CalculateSpeed => _speed;
//     private float _horizontalInput;
//     private float _verticalInput;
//     private Transform _cachedTransform;
//     private Rigidbody2D _cachedRb;
//     private static Random s_rng = new();
//     public CustomBehaviour(GameObject gameObject) : base(gameObject)
//     {
//     }
//     public override void Awake()
//     {
//         _cachedTransform = Transform;
//         _cachedRb = GetComponent<Rigidbody2D>();
//     }
//     public override void Start()
//     {
//         _cachedRb.GravityScale = 1.0f;
//         _cachedRb.Mass = 1.0f;
//         _cachedTransform.LocalScale *= 0.1f;
//     }
//     public override void OnEnable()
//     {
//         Input.Instance.OnMouseMoved += OnMouseMoved;
//     }
//     public override void OnDisable()
//     {
//         Input.Instance.OnMouseMoved -= OnMouseMoved;
//     }
//     public override void Update(double delta)
//     {
//         _horizontalInput = Input.Instance.KeyboardAxis.X;
//         _verticalInput = Input.Instance.KeyboardAxis.Y;
//     }
//     public override void Tick(float delta)
//     {
//         // _cachedRb.Velocity = new Vector3(_horizontalInput * delta * _speed, _verticalInput * delta * _speed, 0.0f);
//     }
//     private void OnMouseMoved(Vector2 obj)
//     {
//         // var worldPoint = ParentScene.Camera.ScreenPointToWorldPoint(obj);
//         // Log.Debug($"World point: {worldPoint}");
//         // _cachedTransform.Position = new Vector3(worldPoint.X, worldPoint.Y, _cachedTransform.Position.Z);
//     }
// }
// public class CameraController : Component
// {
//     private float _speed = 10.0f;
//     private float CalculateSpeed => _speed * Time.DeltaTime;
//     private Transform _cachedTransform;
//     private static Random s_rng = new();
//     public CameraController(GameObject gameObject) : base(gameObject)
//     {
//     }
//     public override void Awake()
//     {
//         _cachedTransform = Transform;
//     }
//
//     public override void OnEnable()
//     {
//         Input.Instance.AddKeyHeldListener(Key.W, OnWPressed); 
//         Input.Instance.AddKeyHeldListener(Key.A, OnAPressed); 
//         Input.Instance.AddKeyHeldListener(Key.S, OnSPressed); 
//         Input.Instance.AddKeyHeldListener(Key.D, OnDPressed); 
//         Input.Instance.OnMouseMoved += OnMouseMoved;
//     }
//
//     public override void OnDisable()
//     {
//         Input.Instance.RemoveKeyHeldListener(Key.W, OnWPressed); 
//         Input.Instance.RemoveKeyHeldListener(Key.A, OnAPressed); 
//         Input.Instance.RemoveKeyHeldListener(Key.S, OnSPressed); 
//         Input.Instance.RemoveKeyHeldListener(Key.D, OnDPressed); 
//     }
//
//     public override void Update(double delta)
//     {
//         var randomX = 1 - 2 * s_rng.NextSingle();
//         var randomY = s_rng.NextSingle();
//         // _cachedTransform.LocalPosition = new Vector3(randomX * 4, randomY * 4, 0.0f);
//         // _cachedTransform.LocalScale = new Vector3(randomX, randomY, 0.0f);
//         ParentScene.Camera.Transform.Position = Vector3.Lerp(ParentScene.Camera.Transform.Position, _cachedTransform.Position, (float)delta * 10.0f);
//         // Log.Debug(ParentScene.Camera.Transform.Position.ToString());
//     }
//
//     private void OnWPressed(Key obj)
//     {
//         ParentScene.Camera.Transform.Position += new Vector3(0.0f, CalculateSpeed , 0.0f);
//     }
//     private void OnAPressed(Key obj)
//     {
//         ParentScene.Camera.Transform.Position += new Vector3(CalculateSpeed, 0.0f, 0.0f);
//     }
//     private void OnSPressed(Key obj)
//     {
//         ParentScene.Camera.Transform.Position += new Vector3(0.0f, -CalculateSpeed, 0.0f);
//     }
//     private void OnDPressed(Key obj)
//     {
//         ParentScene.Camera.Transform.Position += new Vector3(-CalculateSpeed, 0.0f, 0.0f);
//     }
//     private void OnMouseMoved(Vector2 obj)
//     {
//         
//     }
// }