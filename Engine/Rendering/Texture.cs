using Silk.NET.OpenGL;
using StbImageSharp;

namespace Engine.Rendering;

public class Texture : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;

    public int Width { get; }
    public int Height { get; }

    public Texture(GL gl, string path)
    {
        _gl = gl;

        // Load the image from disk
        StbImage.stbi_set_flip_vertically_on_load(0);
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        Width = image.Width;
        Height = image.Height;

        // Create an OpenGL texture and upload the pixel data
        _handle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _handle);

        unsafe
        {
            fixed (byte* data = image.Data)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                    (uint)Width, (uint)Height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }
        }

        // Filtering: how the texture looks when scaled up/down
        // Nearest = pixel-art style (sharp), Linear = smooth
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);

        // Wrapping: what happens at the edges
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(int slot = 0)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + slot);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(_handle);
    }
}
