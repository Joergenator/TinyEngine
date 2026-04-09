using System.Numerics;
using Silk.NET.OpenGL;
using Engine.Core;

namespace Engine.Rendering;

public class SpriteRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly uint _vao; // Vertex Array Object — stores the quad's layout
    private readonly uint _vbo; // Vertex Buffer Object — stores the quad's vertex data

    // View-projection matrix — set by the camera each frame
    private Matrix4x4 _viewProjection;

    // Shader source code (GLSL) — these run on the GPU
    private const string VertexSource = @"
        #version 330 core
        layout (location = 0) in vec2 aPosition;
        layout (location = 1) in vec2 aTexCoord;

        out vec2 TexCoord;

        uniform mat4 uProjection;
        uniform mat4 uModel;

        void main()
        {
            TexCoord = aTexCoord;
            gl_Position = uProjection * uModel * vec4(aPosition, 0.0, 1.0);
        }
    ";

    private const string FragmentSource = @"
        #version 330 core
        in vec2 TexCoord;
        out vec4 FragColor;

        uniform vec4 uColor;
        uniform sampler2D uTexture;
        uniform bool uUseTexture;

        void main()
        {
            if (uUseTexture)
                FragColor = texture(uTexture, TexCoord) * uColor;
            else
                FragColor = uColor;
        }
    ";

    public SpriteRenderer(GL gl, int screenWidth, int screenHeight)
    {
        _gl = gl;
        _shader = new Shader(gl, VertexSource, FragmentSource);

        // Default projection until a camera is set
        _viewProjection = Matrix4x4.CreateOrthographicOffCenter(
            0, screenWidth, screenHeight, 0, -1f, 1f);

        // Define a 1x1 unit quad (two triangles). We'll scale it with the model matrix.
        // Each vertex: X, Y, TexU, TexV
        float[] vertices =
        {
            // pos        // tex
            0f, 0f,       0f, 0f,  // top-left
            1f, 0f,       1f, 0f,  // top-right
            1f, 1f,       1f, 1f,  // bottom-right

            0f, 0f,       0f, 0f,  // top-left
            1f, 1f,       1f, 1f,  // bottom-right
            0f, 1f,       0f, 1f,  // bottom-left
        };

        // Upload the quad to the GPU
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        unsafe
        {
            fixed (float* data = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(vertices.Length * sizeof(float)),
                    data, BufferUsageARB.StaticDraw);
            }
        }

        // Tell OpenGL how to read the vertex data:
        // Location 0 = position (2 floats), Location 1 = tex coords (2 floats)
        uint stride = 4 * sizeof(float); // 4 floats per vertex
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);

        _gl.EnableVertexAttribArray(1);
        unsafe
        {
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride,
                (void*)(2 * sizeof(float)));
        }

        _gl.BindVertexArray(0);
    }

    public void SetViewProjection(Matrix4x4 viewProjection)
    {
        _viewProjection = viewProjection;
    }

    public void DrawRect(Vector2 position, Vector2 size, Vector4 color)
    {
        var transform = new Transform
        {
            Position = position,
            Scale = size
        };
        DrawRect(transform, color);
    }

    public void DrawRect(Transform transform, Vector4 color)
    {
        _shader.Use();

        _shader.SetUniform("uProjection", _viewProjection);
        _shader.SetUniform("uModel", transform.GetModelMatrix());
        _shader.SetUniform("uColor", color);
        _shader.SetUniform("uUseTexture", 0); // false

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);
    }

    public void DrawSprite(Transform transform, Texture texture, Vector4 color)
    {
        _shader.Use();

        texture.Bind(0);
        _shader.SetUniform("uTexture", 0); // texture slot 0
        _shader.SetUniform("uUseTexture", 1); // true
        _shader.SetUniform("uProjection", _viewProjection);
        _shader.SetUniform("uModel", transform.GetModelMatrix());
        _shader.SetUniform("uColor", color);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        _shader.Dispose();
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
    }
}
