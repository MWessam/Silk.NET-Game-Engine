using System.Numerics;
using LunarEngine.GameEngine;
using Silk.NET.Input;

namespace LunarEngine.GameObjects;

public class CustomBehaviour : Component
{
    private float _speed = 10.0f;
    private float CalculateSpeed => _speed * Time.DeltaTime;
    public CustomBehaviour(GameObject gameObject) : base(gameObject)
    {
    }
    public override void Awake()
    {
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.W, OnWPressed); 
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.A, OnAPressed); 
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.S, OnSPressed); 
        InputEngine.InputEngine.Instance.AddKeyHeldListener(Key.D, OnDPressed); 
    }
    private void OnWPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(0.0f, CalculateSpeed , 0.0f);
    }
    private void OnAPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(CalculateSpeed, 0.0f, 0.0f);
    }
    private void OnSPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(0.0f, -CalculateSpeed, 0.0f);
    }
    private void OnDPressed(Key obj)
    {
        Transform.LocalPosition += new Vector3(-CalculateSpeed, 0.0f, 0.0f);
    }
}