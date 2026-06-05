using System.Numerics;
using LunarEngine.ECS.Systems;
using LunarEngine.Renderer;
using LunarEngine.Utilities;
using Serilog;
using Silk.NET.Maths;

namespace LunarEngine.ECS.Components;
public struct CameraComponent : IComponent
{
    public Camera Camera;
    public bool IsPrimary = true;

    public CameraComponent()
    {
        Camera = null;
    }
}
public class Camera : IComponent
{
    public Matrix4x4 View;
    public Matrix4x4 Projection;
    public Matrix4x4 ViewProjection;
    public float Width = 5;
    public float Height = 5;
    public float Fov = 30;
    public float AspectRatio = 1.778f;
    public float Near = 0.1f;
    public float Far = 1000.0f;

    public Camera()
    {
        
    }
    public Camera(float width, float height, float near, float far)
    {
        Width = width;
        Height = height;
        Near = near;
        Far = far;
    }

    // TODO: Remove from here.
    // public Vector3 ScreenPointToWorldPoint(Vector2 screenPoint)
    // {
    //     // Convert screen point to normalized device coordinates (NDC)
    //     float x = (2.0f * screenPoint.X) / GraphicsEngine.WindowResolution.X - 1.0f;
    //     float y = 1.0f - (2.0f * screenPoint.Y) / GraphicsEngine.WindowResolution.Y; // Flip y-coordinate
    //
    //     // Create clip coordinates
    //     Vector4 clipCoords = new Vector4(x, y, -1.0f, 1.0f);
    //
    //     // Convert to view space
    //     if (!(Matrix4x4.Invert(Projection, out var invertedProjection) && Matrix4x4.Invert(View, out var invertedView)))
    //     {
    //         Log.Error($"Projection or view matrix were NOT a square matrix. Couldn't invert.");
    //         return Vector3.Zero;
    //     }
    //     Vector4 viewCoords = Vector4.Transform(clipCoords, invertedProjection);
    //
    //     // Convert to world space
    //     Vector4 worldCoords = Vector4.Transform(viewCoords, invertedView);
    //
    //     // Return the world position, ignoring the w component
    //     return new Vector3(worldCoords.X, worldCoords.Y, worldCoords.Z);
    // }
}

public class EditorCamera : Camera
{
    public Vector2 PanSpeed
    {
        get
        {
            float x = MathF.Min(_viewport.X / 1000.0f, 2.4f);
            float xFactor = 0.0366f * (x * x) - 0.1778f * x + 0.3021f;
            
            float y = MathF.Min(_viewport.Y / 1000.0f, 2.4f);
            float yFactor = 0.0366f * (y * y) - 0.1778f * y + 0.3021f;
            
            return new Vector2(xFactor, yFactor);
        }
    }

    public float RotationSpeed
    {
        get
        {
            return 0.05f;
        }
    }
    public float ZoomSpeed
    {
        get
        {
            float distance = _distance;
            distance = MathF.Max(distance, 0.0f);
            float speed = distance * distance;
            speed = MathF.Min(speed, 100.0f);
            return speed;
        }
    }
    private Transform _transform;
    private Position _position;
    private Rotation _rotation;
    private Vector2D<int> _viewport;
    private float _distance;
    private Vector3 _focalPoint;
    private float _pitch;
    private float _yaw;

    public EditorCamera()
    {
        _transform = new Transform();
        _position.Value = new Vector3(0.0f, 0.0f, -10.0f);
        _rotation.Value = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        _distance = MathF.Abs(_position.Value.Z);
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(Fov.DegreeToRadian(), AspectRatio, Near, Far);
        UpdateTransformMatrix();
    }

    public void UpdateViewProjection()
    {
        var forward = _transform.GetForward();
        var up = _transform.GetUp();
        View = Matrix4x4.CreateLookAt(_position.Value, _position.Value + forward, up);
        ViewProjection = View * Projection;
    }

    public void UpdateViewportCamera(Vector2D<int> viewport)
    {
        var aspectRatio = (float)viewport.X / viewport.Y;
        Width = Height * aspectRatio;
        _viewport = viewport;
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(Fov.DegreeToRadian(), AspectRatio, Near, Far);
        UpdateViewProjection();
    }

    public void MousePan(Vector2 delta, float dt)
    {
        var panSpeed = PanSpeed;
        var right = new Vector3(_transform.Value.M11, _transform.Value.M12, _transform.Value.M13);
        var up = new Vector3(_transform.Value.M21, _transform.Value.M22, _transform.Value.M23);
        _focalPoint += right * delta.X * panSpeed.X * dt + -up * delta.Y * panSpeed.Y * dt;
        CalculatePosition();
    }

    public void MouseZoom(float delta, float dt)
    {
        var distance = _distance - delta * ZoomSpeed * dt;
        if (distance < 1.0f)
        {
            return;
        }

        _distance = distance;
        CalculatePosition();
    }

    public void MouseRotate(Vector2 delta, float dt)
    {
        float yawSign = _transform.GetUp().Y < 0 ? -1.0f : 1.0f;

        _yaw += yawSign * delta.X * RotationSpeed * dt;
        _pitch += delta.Y * RotationSpeed * dt;
        CalculateOrientation();
    }

    public void KeyboardMove(Vector2 input, float dt)
    {
        var right = _transform.GetRight() * input.X;
        var forward = _transform.GetForward() * input.Y;
        _position.Value += (right + forward) * dt;
    }

    public void Update()
    {
        // UpdateViewProjection();
    }

    private void UpdateTransformMatrix()
    {
        TransformSystem.CalculateTransform(ref _transform, _rotation.Value, _position.Value, Vector3.One, true);
        UpdateViewProjection();
    }

    private void CalculatePosition()
    {
        var forward = _transform.GetForward();
        _position.Value = _focalPoint - forward * _distance;
        UpdateTransformMatrix();
    }

    private void CalculateOrientation()
    {
        _rotation.Value = Quaternion.CreateFromYawPitchRoll(-_yaw, -_pitch, 0.0f);
        UpdateTransformMatrix();
    }

    public void LookAt(Vector3 entityPosValue, Transform entityTransform, Quaternion entityRotValue = default)
    {
        _focalPoint = entityPosValue;
        Vector3 directionToEntity = Vector3.Normalize(entityPosValue - _position.Value);
        
        Quaternion targetRotation = LookRotation(directionToEntity, Vector3.UnitY);
        var eulerAngles = targetRotation.ToEulerAngles();
        _pitch = eulerAngles.X;
        _yaw = eulerAngles.Y;
        float distance = 10.0f;
        if (distance > 0.0f)
        {
            // Assume that your Transform type exposes a GetForward() method.
            Vector3 entityForward = entityTransform.GetForward(); // Typically a normalized vector.
            _position.Value = entityPosValue - Vector3.UnitZ * distance;
        }
        CalculateOrientation();
    }

    private Quaternion LookRotation(Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        // Recalculate right and corrected up vectors.
        Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Cross(forward, right);

        // Create a rotation matrix from these basis vectors.
        Matrix4x4 m = new Matrix4x4(
            right.X,    right.Y,    right.Z,    0,
            up.X,       up.Y,       up.Z,       0,
            forward.X,  forward.Y,  forward.Z,  0,
            0,          0,          0,          1
        );
        // Convert the rotation matrix to a quaternion.
        return Quaternion.CreateFromRotationMatrix(m);
    }
}