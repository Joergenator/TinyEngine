using Silk.NET.Input;

namespace Engine.Input;

public class InputManager
{
    private IKeyboard? _keyboard;
    private IMouse? _mouse;

    // We track keys in two sets:
    // _keysDown = all keys currently held down (updated instantly by GLFW callbacks)
    // _keysPressed = keys that started being held THIS frame (cleared each frame)
    // _keysReleased = keys that were let go THIS frame (cleared each frame)
    private readonly HashSet<Key> _keysDown = new();
    private readonly HashSet<Key> _keysPressed = new();
    private readonly HashSet<Key> _keysReleased = new();

    public void Initialize(IInputContext input)
    {
        // Silk.NET can have multiple keyboards (e.g. virtual ones). We grab the first.
        _keyboard = input.Keyboards[0];
        _mouse = input.Mice[0];

        // These callbacks fire the instant a key is pressed/released (not once per frame).
        // We record the events and process them in Update().
        _keyboard.KeyDown += OnKeyDown;
        _keyboard.KeyUp += OnKeyUp;
    }

    /// <summary>
    /// Call once per frame BEFORE game logic reads input.
    /// Clears the pressed/released states from last frame.
    /// </summary>
    public void Update()
    {
        _keysPressed.Clear();
        _keysReleased.Clear();
    }

    // --- Keyboard ---

    /// <summary>True the single frame a key is first pressed.</summary>
    public bool IsKeyPressed(Key key) => _keysPressed.Contains(key);

    /// <summary>True every frame the key is held down.</summary>
    public bool IsKeyDown(Key key) => _keysDown.Contains(key);

    /// <summary>True the single frame a key is released.</summary>
    public bool IsKeyReleased(Key key) => _keysReleased.Contains(key);

    // --- Mouse ---

    /// <summary>Mouse position in window coordinates.</summary>
    public System.Numerics.Vector2 MousePosition =>
        _mouse?.Position ?? System.Numerics.Vector2.Zero;

    /// <summary>True while a mouse button is held.</summary>
    public bool IsMouseButtonDown(MouseButton button) =>
        _mouse?.IsButtonPressed(button) ?? false;

    // --- Callbacks ---

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        // Only count as "pressed" if it wasn't already down (avoids key repeat)
        if (_keysDown.Add(key))
        {
            _keysPressed.Add(key);
        }
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
    {
        _keysDown.Remove(key);
        _keysReleased.Add(key);
    }
}
