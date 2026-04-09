# TinyEngine — Step-by-Step Walkthrough

## Step 1: Project Setup

### Solution structure
- **Solution (`TinyEngine.slnx`)** — the top-level file that groups projects together. `dotnet build` at the root knows what to build because of this.
- **Engine project** — a **class library** (no `Main`, can't run on its own). This is the reusable engine code.
- **Game project** — a **console app** (has `Main`/`Program.cs`). This is the executable that uses the engine. It has a `<ProjectReference>` to Engine so it can access engine classes.

### NuGet packages (added to Engine)
- **Silk.NET.Windowing** — creates a window using GLFW under the hood. Provides the game loop (Load/Update/Render events).
- **Silk.NET.Input** — keyboard and mouse access.
- **Silk.NET.OpenGL** — C# bindings to OpenGL so we can draw things.

Game doesn't need these packages directly — it gets them through its reference to Engine.

---

## Step 2 & 3: Game Loop and Window

### The flow
1. **`Program.cs`** creates a `GameEngine` instance (which configures the window and hooks up events)
2. **`engine.Run()`** calls `_window.Run()` which:
   - Opens the window
   - Fires `OnLoad()` once
   - Enters an infinite loop: calls `OnUpdate()` then `OnRender()` every frame, passing delta time
   - When you close the window, fires `OnClosing()` and the loop exits

`Run()` is **blocking** — nothing after it executes until the window closes. The entire game lives inside that loop.

### GameEngine lifecycle
```
OnLoad()          ← once
  ↓
OnUpdate(dt)  ←──┐
OnRender(dt)     │ repeats every frame
  ↓──────────────┘
OnClosing()       ← when window closes
```

### Delta time
Delta time (`dt`) is the seconds since the last frame (e.g. ~0.016 at 60fps). You multiply movement by delta time so things move at the same speed regardless of framerate (e.g., `position += speed * deltaTime`).

### OpenGL initialization
In `OnLoad`, we grab the OpenGL context from the window:
```csharp
_gl = _window.CreateOpenGL();
```
This gives us the `GL` object — our handle to talk to the GPU. Every draw call, shader setup, and texture load goes through this object.

### ClearColor and Clear
**`ClearColor`** sets a color value in OpenGL's state. It doesn't draw anything — it just says "when I ask you to clear, use this color." Think of it as dipping a paint roller in a color.

**`Clear`** actually fills the screen with that color. It wipes everything from the previous frame and replaces it with the clear color. This is the paint roller going across the canvas.

You need both. Without `ClearColor`, OpenGL doesn't know what color to use. Without `Clear`, the screen never gets wiped — you'd see old frames stacking on top of each other (which looks like a smeared mess).

They are called every frame in `OnRender` because rendering works like a flipbook:
1. Clear the page (blank slate)
2. Draw everything on top (sprites, shapes)
3. Silk.NET flips the page to the screen (this happens automatically after `OnRender`)
4. Repeat next frame

### OnResize
`_gl.Viewport(size)` tells OpenGL the drawable area changed so it doesn't stretch or crop when you resize the window.

---

## Step 4: Input System

### Architecture
Three `HashSet<Key>` track different states:

**`_keysDown`** — all keys currently held. Keys are **added** in `OnKeyDown` and **removed** in `OnKeyUp`. This persists across frames — it represents the physical state of the keyboard right now.

**`_keysPressed`** — keys that went down **this frame**. Added in `OnKeyDown` (only if the key wasn't already in `_keysDown` — this prevents key-repeat from triggering it multiple times). Cleared every frame by `Update()`.

**`_keysReleased`** — keys that went up **this frame**. Added in `OnKeyUp`. Cleared every frame by `Update()`.

### Key states
| Method | When it's true |
|---|---|
| `IsKeyPressed` | Only the first frame you push the key |
| `IsKeyDown` | Every frame while held |
| `IsKeyReleased` | Only the frame you let go |

### Key repeat prevention
The `OnKeyDown` callback has a subtle detail:
```csharp
if (_keysDown.Add(key))       // Add returns false if already present
{
    _keysPressed.Add(key);    // only fires on the FIRST frame
}
```
`HashSet.Add()` returns `false` if the key was already in the set. So holding a key down doesn't keep re-triggering "pressed" — only the initial push counts.

### Frame ordering
GLFW fires key callbacks before `OnUpdate` runs (during event polling). The order matters:
```
GLFW polls OS events → OnKeyDown/OnKeyUp callbacks fire (populate sets)
        ↓
OnUpdate(deltaTime)
    1. OnUpdateCallback runs (game reads input)
    2. Input.Update() clears pressed/released for next frame
```

If `Input.Update()` (the clear) happened before the game reads input, the game would never see any pressed/released events — they'd be cleared before being read. That's why the game reads first, then we clear.

---

## Step 5: Renderer (Colored Rectangles)

### The rendering pipeline — how pixels get on screen

There are three pieces: **shaders**, **the quad**, and **the draw call**.

### 1. Shaders (GPU programs)

Shaders are tiny programs written in GLSL that run on the GPU. We have two:

**Vertex shader** — runs once per vertex (corner point). Its job is to position each vertex on screen:
```glsl
gl_Position = uProjection * uModel * vec4(aPosition, 0.0, 1.0);
```
This multiplies each vertex position by two matrices:
- `uModel` — moves and scales the quad to where we want it (e.g. position 350,250, size 100x100)
- `uProjection` — converts pixel coordinates to OpenGL's -1..1 range

**Fragment shader** — runs once per pixel inside the shape. Its job is to decide the color:
```glsl
FragColor = uColor;
```
Right now it just outputs a solid color. Later when we add textures, this will sample from an image.

The `Shader` class compiles these from source, links them into a program, and provides `SetUniform()` to send values (color, matrices) from C# to the GPU.

### 2. The quad (rectangle geometry)

OpenGL only draws triangles. A rectangle is two triangles:
```
0───1        Triangle 1: 0, 1, 2  (top-left, top-right, bottom-right)
│ ╲ │        Triangle 2: 0, 2, 3  (top-left, bottom-right, bottom-left)
3───2
```

We define a **1x1 unit quad** (0,0 to 1,1) with 6 vertices:
```csharp
float[] vertices =
{
    // X    Y     TexU  TexV
    0f, 0f,  0f, 0f,   // top-left
    1f, 0f,  1f, 0f,   // top-right
    1f, 1f,  1f, 1f,   // bottom-right
    0f, 0f,  0f, 0f,   // top-left (again)
    1f, 1f,  1f, 1f,   // bottom-right (again)
    0f, 1f,  0f, 1f,   // bottom-left
};
```

Each vertex has 4 floats: position (X, Y) and texture coordinates (U, V) for later.

This data gets uploaded to the GPU once in the constructor via:
- **VBO** (Vertex Buffer Object) — the raw vertex data sitting in GPU memory
- **VAO** (Vertex Array Object) — tells OpenGL how to interpret the VBO ("first 2 floats are position, next 2 are texture coords")

```csharp
// "position is at offset 0, it's 2 floats"
_gl.VertexAttribPointer(0, 2, Float, false, stride, 0);

// "tex coords are at offset 8 bytes (2 floats), it's 2 floats"
_gl.VertexAttribPointer(1, 2, Float, false, stride, (void*)(2 * sizeof(float)));
```

### 3. The draw call (DrawRect)

When you call `DrawRect(position, size, color)`:

```csharp
// Build a model matrix that scales the 1x1 quad to the desired size
// and moves it to the desired position
var model = Matrix4x4.CreateScale(size.X, size.Y, 1f)
          * Matrix4x4.CreateTranslation(position.X, position.Y, 0f);
```

So a 1x1 quad scaled by (100, 100) and translated to (350, 250) becomes a 100x100 rectangle at that position.

Then we send everything to the GPU and draw:
```csharp
_shader.Use();                              // activate our shader program
_shader.SetUniform("uProjection", ...);     // send the projection matrix
_shader.SetUniform("uModel", model);        // send the model matrix
_shader.SetUniform("uColor", color);        // send the color
_gl.BindVertexArray(_vao);                  // point to our quad geometry
_gl.DrawArrays(Triangles, 0, 6);           // draw 6 vertices = 2 triangles = 1 rect
```

### 4. The projection matrix

Without this, OpenGL works in a -1..1 coordinate space (center is 0,0). That's awkward for 2D games. The orthographic projection:

```csharp
Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1f, 1f);
```

Maps pixel coordinates so that (0,0) is top-left and (800,600) is bottom-right — just like you'd expect in a 2D game. This is why `UpdateProjection` is called on resize.

### 5. The full frame

Every frame in `OnRender`:
1. `Clear` — wipe the screen to blue
2. `DrawRect(white)` — GPU runs vertex shader on 6 vertices, rasterizes the triangles, runs fragment shader on each pixel, outputs white
3. `DrawRect(red)` — same thing, red on top
4. Silk.NET swaps the buffer — the finished image appears on screen

### Unsafe code

OpenGL needs raw pointers to pass data (vertices, matrices) to the GPU. C# requires `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the project file to use pointers. This is standard for any C# graphics programming.

---

## Step 6: Transform (Position, Rotation, Scale)

### What it is

Transform holds four properties that describe where and how an object exists in the world:

- **Position** — where in the world (in pixels)
- **Scale** — the size (since our quad is 1x1, Scale of (100,100) = 100x100 pixel rectangle)
- **Rotation** — angle in degrees
- **Origin** — the pivot point for rotation, in pixel space relative to the object

The key method is `GetModelMatrix()` which builds a matrix from these properties. The GPU multiplies every vertex of the 1x1 quad by this matrix to get its final screen position.

### The matrix chain

```
Scale → Translate(-Origin) → Rotate → Translate(+Origin) → Translate(Position)
```

Traced for vertex (0,0) with Scale=(100,100), Origin=(50,50), Rotation=45°, Position=(350,250):

1. **Scale(100,100)** — (0,0) → (0,0) — quad is now 100x100 pixels
2. **Translate(-50,-50)** — (0,0) → (-50,-50) — shift so the center is at (0,0)
3. **Rotate 45°** — (-50,-50) → (0, -70.7) — spin around (0,0)
4. **Translate(+50,+50)** — (0,-70.7) → (50, -20.7) — shift back
5. **Translate(350,250)** — (50,-20.7) → (400, 229.3) — move to world position

### Why Origin matters

Without Origin (or Origin = 0,0), rotation happens around the top-left corner of the sprite, which looks unnatural. Setting Origin to the center of the sprite (half the scale) makes it spin in place.

### Matrix order matters

The original (broken) order was `Translate(-Origin) → Scale → ...`. This offset the 1x1 quad by -50 pixels before scaling. A 1x1 quad shifted by -50 then scaled by 100x puts the vertex at -5000 pixels — way off screen. Scale must happen first so the origin offset operates in pixel space.

### Integration with SpriteRenderer

`DrawRect` now accepts a Transform and calls `transform.GetModelMatrix()` to get the model matrix, instead of manually building Scale * Translate. The old `DrawRect(position, size, color)` overload still works — it creates a Transform internally.

---

## Step 7 & 8: Entity-Component System and Scene

### The core idea

Separate **what something is** (Entity) from **what it does** (Components).

### Entity

An Entity is a container. It doesn't know how to move, render, or do anything on its own. It has:

- **Name** — for identification ("Player", "Obstacle")
- **Transform** — position, rotation, scale (every entity has one)
- **List of Components** — behaviors attached to it

```csharp
var player = new Entity("Player");
player.Transform.Position = new Vector2(350, 250);
player.AddComponent<SpriteComponent>();
```

`AddComponent<T>()` creates a new component, sets its `Entity` reference back to this entity, and stores it. `GetComponent<T>()` searches the list and returns the first match — this is how components find each other later (e.g., a physics component finding the sprite component on the same entity).

### Component

A Component is a behavior you bolt onto an Entity. It's an abstract base class with three virtual methods:

```csharp
public virtual void Initialize() { }              // once, when added to scene
public virtual void Update(double dt) { }          // every frame
public virtual void Render(SpriteRenderer renderer) { }  // every frame
```

All three are optional — override only what you need. A `SpriteComponent` only overrides `Render`. A future `PlayerController` would only override `Update`.

Every component has an `Entity` reference, so it can access the Transform:
```csharp
public override void Render(SpriteRenderer renderer)
{
    renderer.DrawRect(Entity.Transform, Color);  // "draw ME where MY entity is"
}
```

The component doesn't own position data — it reads it from its entity's Transform. Multiple components on the same entity all share the same Transform.

### Scene

A Scene is a list of entities. It has three jobs:

1. **AddEntity / CreateEntity** — adds an entity and calls `Initialize()` on all its components
2. **Update(dt)** — loops through every entity → every component's `Update`
3. **Render(renderer)** — loops through every entity → every component's `Render`

```
Scene.Update(dt)
  → Entity "Player".Update(dt)
      → SpriteComponent.Update(dt)
  → Entity "Obstacle".Update(dt)
      → SpriteComponent.Update(dt)

Scene.Render(renderer)
  → Entity "Player".Render(renderer)
      → SpriteComponent.Render(renderer)   // draws white rect
  → Entity "Obstacle".Render(renderer)
      → SpriteComponent.Render(renderer)   // draws red rect
```

### How GameEngine uses it

The engine calls the scene before the callbacks each frame:

```
OnUpdate:
  1. ActiveScene.Update(dt)        ← all components update
  2. OnUpdateCallback(dt)          ← game-specific logic
  3. Input.Update()                ← clear input

OnRender:
  1. Clear screen
  2. ActiveScene.Render(renderer)  ← all components draw
  3. OnRenderCallback(dt)          ← extra drawing if needed
```

### Why this pattern matters

Adding a new game object requires no engine changes:
```csharp
var enemy = scene.CreateEntity("Enemy");
enemy.Transform.Position = new Vector2(500, 300);
enemy.Transform.Scale = new Vector2(40, 40);
var sprite = enemy.AddComponent<SpriteComponent>();
sprite.Color = new Vector4(0, 1, 0, 1);
```

The scene picks it up automatically. This scales to hundreds of entities without Program.cs becoming a mess.
