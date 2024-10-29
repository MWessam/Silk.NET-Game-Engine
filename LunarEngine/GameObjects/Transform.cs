using System.Collections;
using System.Numerics;
using LunarEngine.Utilities;

namespace LunarEngine.GameObjects;

public class Transform : IEnumerable<Transform>
{
    #region Statics
    public static Vector3 DefaultZero => Vector3.Zero;
    public static Vector3 DefaultOne => Vector3.One;
    public static Vector3 DefaultRight => Vector3.UnitX;
    public static Vector3 DefaultLeft => -Vector3.UnitX;
    public static Vector3 DefaultUp => Vector3.UnitY;
    public static Vector3 DefaultDown => -Vector3.UnitY;
    public static Vector3 DefaultForward => -Vector3.UnitZ;
    public static Vector3 DefaultBackward => Vector3.UnitZ;
    #endregion
    public Transform Parent;
    public List<Transform> Children = new();
    private Quaternion _localRotation = Quaternion.Identity;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _localEulerAngles = DefaultZero;
    private Vector3 _eulerAngles = DefaultZero;
    private Vector3 _localPosition = DefaultZero;
    private Vector3 _position = DefaultZero;
    private Vector3 _localScale = DefaultOne;
    private Vector3 _scale = DefaultOne;
    private Matrix4x4 _model;

    // Local Properties
    public Vector3 LocalPosition
    {
        get => _localPosition;
        set
        {
            _localPosition = value;
            MarkDirty();
        }
    }
    public Vector3 LocalScale
    {
        get => _localScale;
        set
        {
            _localScale = value;
            MarkDirty();
        }
    }
    public Quaternion LocalRotation
    {
        get => _localRotation;
        set
        {
            _localRotation = value;
            _localEulerAngles = value.ToEulerAngles().RadianToDegree();
            MarkDirty();
        }
    }
    public Vector3 LocalEulerAngles
    {
        get => _localEulerAngles;
        set
        {
            _localEulerAngles = value;
            _localRotation = value.DegreeToRadian().ToQuaternion();
            MarkDirty();
        }
    }
    
    // World Properties
    public Vector3 Position
    {
        get
        {
            if (Parent != null)
            {
                return Parent.Position + Vector3.Transform(LocalPosition, Parent.Rotation) * Parent.Scale;
            }
            return LocalPosition;
        }
    }
    public Vector3 Scale
    {
        get
        {
            // Calculate world scale based on parent's scale
            if (Parent != null)
            {
                return Vector3.Multiply(LocalScale, Parent.Scale);
            }
            return LocalScale;
        }
    }
    public Quaternion Rotation
    {
        get
        {
            if (Parent != null)
            {
                return Parent.Rotation * LocalRotation; // Order matters: parent rotation followed by local rotation
            }
            return LocalRotation;
        }
    }
    public Vector3 EulerAngles => Rotation.ToEulerAngles();
    public Vector3 Right => new Vector3(Model.M11, Model.M12, Model.M13);
    public Vector3 Up => new Vector3(Model.M21, Model.M22, Model.M23);
    public Vector3 Forward => new Vector3(Model.M31, Model.M32, Model.M33);
    
    public Matrix4x4 Model
    {
        get
        {
            if (IsDirty)
            {
                _model = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) *
                    Matrix4x4.CreateTranslation(Position);
                IsDirty = false;
            }

            return _model;
        }
    }
    public bool IsDirty { get; set; }


    public void Translate(Vector3 translation, Space relativeTo = Space.Local)
    {
        if (relativeTo == Space.World)
        {
            LocalPosition += translation;
        }
        else
        {
            LocalPosition += TransformDirection(translation);
        }
    }
    public void Rotate(Vector3 eulerAngles)
    {
        LocalEulerAngles += eulerAngles;
    }
    public void Rotate(Quaternion rotation)
    {
        LocalRotation *= rotation;
    }
    public void Scaled(Vector3 scale)
    {
        LocalScale *= scale;
    }
    public void Matrix(Matrix4x4 matrix)
    {
        Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
        LocalScale = scale;
        LocalRotation = rotation;
        LocalPosition = translation;
    }
    public Vector3 TransformDirection(Vector3 translation)
    {
        return Vector3.Transform(translation, Rotation);
    }
    
    public void AddChild(Transform child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        
        child.Parent.RemoveChild(child);
        child.Parent = this;
        Children.Add(child);
        MarkDirty();
    }
    
    public void RemoveChild(Transform child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        
        if (Children.Remove(child))
        {
            child.Parent = null;
            MarkDirty();
        }
    }
    public void MarkDirty()
    {
        IsDirty = true;
        foreach (var child in Children)
        {
            child.MarkDirty(); // Propagate the dirty state down the hierarchy
        }
    }
    public IEnumerator<Transform> GetEnumerator()
    {
        return Children.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
public enum Space
{
    Local,
    World
}



