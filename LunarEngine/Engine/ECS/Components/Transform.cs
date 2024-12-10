using System.Numerics;
using LunarEngine.Engine.ECS.Components;
namespace LunarEngine.Components;
public struct Position : IComponent
{
    internal Vector3 _value;
    public Vector3 Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (_value == value) return;
            _value = value;
            IsDirty = true;
        }
    }
    public bool IsDirty;
}
public struct Rotation : IComponent
{
    internal Quaternion _value;
    public Quaternion Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (_value == value) return;
            _value = value;
            IsDirty = true;
        }
    }
    public bool IsDirty;
}
public struct Scale : IComponent
{
    internal Vector3 _userValue;
    internal Vector3 _baseValue;
    public Vector3 UserValue
    {
        get
        {
            return _userValue;
        }
        set
        {
            if (_userValue == value) return;
            _userValue = value;
            IsDirty = true;
            ActualValue = _userValue * _baseValue;
        }
    }
    public Vector3 BaseValue
    {
        get
        {
            return _baseValue;
        }
        set
        {
            if (_baseValue == value) return;
            _baseValue = value;
            IsDirty = true;
            ActualValue = _userValue * _baseValue;
        }
    }
    public Vector3 ActualValue;
    public bool IsDirty;
}
public struct Transform : IComponent
{
    public Matrix4x4 Value;
}
public struct Parent : IComponent
{
    public Transform ParentEntity;
}
public struct DirtyTransform : IComponent
{
}
