using System.Numerics;

namespace Engine.Core;

public class Transform
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f; // degrees
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero; // rotation pivot (in local space)

    /// <summary>
    /// Builds the model matrix: Scale → Rotate around origin → Translate.
    /// </summary>
    public Matrix4x4 GetModelMatrix()
    {
        float radians = Rotation * (MathF.PI / 180f);

        // 1. Scale the 1x1 quad to pixel size
        // 2. Move origin to (0,0) so rotation happens around the pivot
        // 3. Rotate
        // 4. Move origin back
        // 5. Translate to world position
        return Matrix4x4.CreateScale(Scale.X, Scale.Y, 1f)
             * Matrix4x4.CreateTranslation(-Origin.X, -Origin.Y, 0f)
             * Matrix4x4.CreateRotationZ(radians)
             * Matrix4x4.CreateTranslation(Origin.X, Origin.Y, 0f)
             * Matrix4x4.CreateTranslation(Position.X, Position.Y, 0f);
    }
}
