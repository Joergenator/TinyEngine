using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Engine.Core;

public class GameEngine
{
    private readonly IWindow _window;

    public GameEngine(string title = "TinyEngine", int width = 800, int height = 600)
    {
        // Configure the window: size, title, VSync, target framerate
        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        options.VSync = true;

        // Silk.NET creates a platform window (GLFW on desktop)
        _window = Window.Create(options);

        // Hook into the window's lifecycle events
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;
    }

    /// <summary>
    /// Starts the game loop. This blocks until the window is closed.
    /// </summary>
    public void Run()
    {
        _window.Run();
    }

    private void OnLoad()
    {
        // Called once when the window first opens.
        // This is where you'll later initialize OpenGL, load resources, etc.
        Console.WriteLine("Engine loaded!");
    }

    private void OnUpdate(double deltaTime)
    {
        // Called every frame for game logic.
        // deltaTime = seconds since last frame (e.g. ~0.016 at 60fps)
        Console.WriteLine($"Update | dt: {deltaTime:F4}s | FPS: {1.0 / deltaTime:F0}");
    }

    private void OnRender(double deltaTime)
    {
        // Called every frame for drawing.
        // Right now it's a black window — we'll add OpenGL clearing next.
    }

    private void OnClosing()
    {
        Console.WriteLine("Engine shutting down.");
    }
}
