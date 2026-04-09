using System.Numerics;
using Engine.Rendering;

namespace Engine.ECS;

public class SpriteComponent : Component
{
    public Vector4 Color { get; set; } = new(1, 1, 1, 1);
    public Texture? Texture { get; set; }

    public override void Render(SpriteRenderer renderer)
    {
        if (Texture != null)
            renderer.DrawSprite(Entity.Transform, Texture, Color);
        else
            renderer.DrawRect(Entity.Transform, Color);
    }
}
