using System.Numerics;
using Silk.NET.OpenGL;

namespace Engine.Rendering;

public class Shader : IDisposable
{
    private readonly GL _gl;
    private readonly uint _program;

    public Shader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        // Compile the two shader stages
        uint vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
        uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

        // Link them into a program (the full pipeline)
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);
        _gl.LinkProgram(_program);

        // Check for link errors
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Shader link error: {_gl.GetProgramInfoLog(_program)}");

        // Individual shaders are baked into the program now, safe to delete
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        _gl.UseProgram(_program);
    }

    public void SetUniform(string name, Matrix4x4 value)
    {
        int location = _gl.GetUniformLocation(_program, name);
        if (location == -1)
            throw new Exception($"Uniform '{name}' not found in shader.");
        unsafe
        {
            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }
    }

    public void SetUniform(string name, Vector4 value)
    {
        int location = _gl.GetUniformLocation(_program, name);
        if (location == -1)
            throw new Exception($"Uniform '{name}' not found in shader.");
        _gl.Uniform4(location, value);
    }

    public void SetUniform(string name, int value)
    {
        int location = _gl.GetUniformLocation(_program, name);
        if (location == -1)
            throw new Exception($"Uniform '{name}' not found in shader.");
        _gl.Uniform1(location, value);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
            throw new Exception($"{type} compile error: {_gl.GetShaderInfoLog(shader)}");

        return shader;
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_program);
    }
}
