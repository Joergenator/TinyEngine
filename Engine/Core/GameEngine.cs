using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Engine.Input;
using Engine.Rendering;
using Engine.Resources;

namespace Engine.Core;

public class GameEngine
{
    private readonly IWindow _window;
    private GL _gl = null!;
    public InputManager Input { get; } = new();
    public SpriteRenderer Renderer { get; private set; } = null!;
    public ResourceManager Resources { get; private set; } = null!;
    public Camera2D Camera { get; private set; } = null!;
    public Scene.Scene? ActiveScene { get; set; }
    public Action? OnLoadCallback { get; set; }
    public Action<double>? OnUpdateCallback { get; set; }
    public Action<double>? OnRenderCallback { get; set; }

    public GameEngine(string title = "TinyEngine", int width = 800, int height = 600)
    {
        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        options.VSync = true;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;
        _window.Resize += OnResize;
    }

    public void Run()
    {
        _window.Run();
    }

    public void Close()
    {
        _window.Close();
    }

    private void OnLoad()
    {
        // Get the OpenGL context from the window.
        // This is our handle to all GL calls (clear, draw, shaders, etc.)
        _gl = _window.CreateOpenGL();

        // Get the input context and hand it to our InputManager
        var input = _window.CreateInput();
        Input.Initialize(input);

        // Enable alpha blending so transparent textures work
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Create the renderer, resource manager, and camera
        Renderer = new SpriteRenderer(_gl, _window.Size.X, _window.Size.Y);
        Resources = new ResourceManager(_gl);
        Camera = new Camera2D(_window.Size.X, _window.Size.Y);

        Console.WriteLine("Engine loaded!");
        Console.WriteLine($"OpenGL {_gl.GetStringS(StringName.Version)}");

        // Let the game load resources now that OpenGL is ready
        OnLoadCallback?.Invoke();
    }

    private void OnUpdate(double deltaTime)
    {
        // Update the active scene (all entities and their components)
        ActiveScene?.Update(deltaTime);

        // Let the game run its logic (reads this frame's input)
        OnUpdateCallback?.Invoke(deltaTime);

        // Clear pressed/released AFTER the game has read them
        Input.Update();
    }

    private void OnRender(double deltaTime)
    {
        _gl.ClearColor(0.39f, 0.58f, 0.93f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Update the renderer with the camera's view-projection matrix
        Renderer.SetViewProjection(Camera.GetViewProjectionMatrix());

        // Render the active scene (all entities and their components)
        ActiveScene?.Render(Renderer);

        // Let the game draw after clearing
        OnRenderCallback?.Invoke(deltaTime);
    }

    private void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        Camera?.UpdateScreenSize(size.X, size.Y);
    }

    private void OnClosing()
    {
        Resources?.Dispose();
        Renderer?.Dispose();
        _gl.Dispose();
        Console.WriteLine("Engine shutting down.");
    }
}
