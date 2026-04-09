using System.Numerics;
using Engine.Core;
using Engine.ECS;
using Engine.Scene;
using Silk.NET.Input;

var engine = new GameEngine("My First Game", 800, 600);

var scene = new Scene("Main");
engine.ActiveScene = scene;

// Player
var player = scene.CreateEntity("Player");
player.Transform.Position = new Vector2(350, 250);
player.Transform.Scale = new Vector2(100, 100);
player.Transform.Origin = new Vector2(50, 50);
var playerSprite = player.AddComponent<SpriteComponent>();

// Scatter some obstacles around the world to see the camera move
var colors = new[]
{
    new Vector4(1, 0, 0, 1),    // red
    new Vector4(0, 1, 0, 1),    // green
    new Vector4(1, 1, 0, 1),    // yellow
    new Vector4(1, 0, 1, 1),    // magenta
};

for (int i = 0; i < 4; i++)
{
    var obj = scene.CreateEntity($"Obstacle{i}");
    obj.Transform.Position = new Vector2(i * 300, i * 200);
    obj.Transform.Scale = new Vector2(60, 60);
    var sprite = obj.AddComponent<SpriteComponent>();
    sprite.Color = colors[i];
}

engine.OnLoadCallback = () =>
{
    playerSprite.Texture = engine.Resources.LoadTexture("Assets/AlienC.png");
};

float speed = 200f;
float rotateSpeed = 90f;

engine.OnUpdateCallback = (dt) =>
{
    float delta = (float)dt;
    var pos = player.Transform.Position;

    if (engine.Input.IsKeyDown(Key.W)) pos.Y -= speed * delta;
    if (engine.Input.IsKeyDown(Key.S)) pos.Y += speed * delta;
    if (engine.Input.IsKeyDown(Key.A)) pos.X -= speed * delta;
    if (engine.Input.IsKeyDown(Key.D)) pos.X += speed * delta;

    player.Transform.Position = pos;

    if (engine.Input.IsKeyDown(Key.Q)) player.Transform.Rotation -= rotateSpeed * delta;
    if (engine.Input.IsKeyDown(Key.E)) player.Transform.Rotation += rotateSpeed * delta;

    // Zoom with Z/X
    if (engine.Input.IsKeyDown(Key.Z)) engine.Camera.Zoom += 1f * delta;
    if (engine.Input.IsKeyDown(Key.X)) engine.Camera.Zoom -= 1f * delta;

    // Camera follows the center of the player
    engine.Camera.Position = player.Transform.Position + player.Transform.Origin;

    if (engine.Input.IsKeyPressed(Key.Escape))
        engine.Close();
};

engine.Run();
