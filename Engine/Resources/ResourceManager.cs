using Silk.NET.OpenGL;

namespace Engine.Resources;

public class ResourceManager : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, Rendering.Texture> _textures = new();

    public ResourceManager(GL gl)
    {
        _gl = gl;
    }

    /// <summary>
    /// Loads a texture from disk, or returns the cached version if already loaded.
    /// </summary>
    public Rendering.Texture LoadTexture(string path)
    {
        // Normalize the path so "assets/player.png" and "assets\\player.png" hit the same cache entry
        string key = Path.GetFullPath(path);

        if (_textures.TryGetValue(key, out var existing))
            return existing;

        var texture = new Rendering.Texture(_gl, key);
        _textures[key] = texture;
        Console.WriteLine($"Loaded texture: {Path.GetFileName(path)} ({texture.Width}x{texture.Height})");
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
            texture.Dispose();
        _textures.Clear();
    }
}
