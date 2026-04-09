using System.Numerics;

namespace Engine.Core;

public class Camera2D
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1.0f;

    private int _screenWidth;
    private int _screenHeight;

    public Camera2D(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void UpdateScreenSize(int width, int height)
    {
        _screenWidth = width;
        _screenHeight = height;
    }

    /// <summary>
    /// Builds a view-projection matrix:
    /// 1. Translate the world so the camera position is at the center of the screen
    /// 2. Scale by zoom level
    /// 3. Apply orthographic projection
    /// </summary>
    public Matrix4x4 GetViewProjectionMatrix()
    {
        // Center of the screen in pixels
        float centerX = _screenWidth / 2f;
        float centerY = _screenHeight / 2f;

        // View matrix: move the world opposite to the camera, then zoom around screen center
        var view = Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0f)
                 * Matrix4x4.CreateScale(Zoom, Zoom, 1f)
                 * Matrix4x4.CreateTranslation(centerX, centerY, 0f);

        // Projection: pixel coordinates with (0,0) top-left
        var projection = Matrix4x4.CreateOrthographicOffCenter(
            0, _screenWidth, _screenHeight, 0, -1f, 1f);

        return view * projection;
    }
}
