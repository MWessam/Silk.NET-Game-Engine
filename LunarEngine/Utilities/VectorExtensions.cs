using System.Numerics;

namespace LunarEngine.Utilities;

public static class VectorExtensions
{
    public static Vector3 RadianToDegree(this Vector3 value)
    {
        return value * 180.0f / MathF.PI;
    }

    public static Vector3 DegreeToRadian(this Vector3 value)
    {
        return value * MathF.PI / 180.0f;
    }

    public static Vector3 ToEulerAngles(this Quaternion rotation)
    {
        float yaw = MathF.Atan2(2.0f * (rotation.Y * rotation.W + rotation.X * rotation.Z), 1.0f - 2.0f * (rotation.X * rotation.X + rotation.Y * rotation.Y));
        float pitch = MathF.Asin(2.0f * (rotation.X * rotation.W - rotation.Y * rotation.Z));
        float roll = MathF.Atan2(2.0f * (rotation.X * rotation.Y + rotation.Z * rotation.W), 1.0f - 2.0f * (rotation.X * rotation.X + rotation.Z * rotation.Z));

        // If any nan or inf, set that value to 0
        if (float.IsNaN(yaw) || float.IsInfinity(yaw))
        {
            yaw = 0;
        }

        if (float.IsNaN(pitch) || float.IsInfinity(pitch))
        {
            pitch = 0;
        }

        if (float.IsNaN(roll) || float.IsInfinity(roll))
        {
            roll = 0;
        }

        return new Vector3(pitch, yaw, roll);
    }

    public static Quaternion ToQuaternion(this Vector3 eulerAngles)
    {
        return Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);
    }

    public static Vector2 AsVector2(this Vector3 vector3)
    {
        return new Vector2(vector3.X, vector3.Y);
    }
    public static Vector3 AsVector3(this Vector2 vector2, float z = 0.0f)
    {
        return new Vector3(vector2.X, vector2.Y, z);
    }
}