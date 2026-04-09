# TinyEngine Roadmap

## Goal
Build a 2D game engine in C# from scratch to learn engine architecture deeply, make games with it, and use as a portfolio project.

## Stack
- C# / .NET 10
- Silk.NET (window, input, OpenGL bindings)
- NuGet for dependencies

## Project Structure
```
TinyEngine/
├── Engine/
│   ├── Core/        # Game loop, window, app entry
│   ├── Input/
│   ├── Rendering/
│   ├── ECS/
│   ├── Scene/
│   └── Resources/
├── Game/            # Test game using the engine
└── TinyEngine.slnx
```

## Phase 1 — Tiny Engine
- [x] 1. Project setup (solution, NuGet, git)
- [ ] 2. Game loop (fixed timestep Update + Render, delta time)
- [ ] 3. Window (Silk.NET, handle close/resize)
- [ ] 4. Input system (keyboard/mouse, pressed/held/released)
- [ ] 5. Renderer (colored rectangles and textured sprites)
- [ ] 6. Transform (position, rotation, scale)

**Milestone:** Textured sprite moving with keyboard input at 60fps

## Phase 2 — Engine Architecture
- [ ] 7. Entity + Component system (Unity-style GameObject/Component)
- [ ] 8. Scene system (Load/Update/Render lifecycle)
- [ ] 9. Resource manager (texture caching)
- [ ] 10. Camera (2D, position + zoom, world-to-screen transform)

**Milestone:** Multi-entity scene with a following camera

## Phase 3 — Full 2D
- [ ] 11. AABB collision detection
- [ ] 12. Audio (SDL_mixer or miniaudio)
- [ ] 13. Tilemap support
- [ ] 14. Basic UI / text rendering
- [ ] 15. Scene serialization (JSON)
