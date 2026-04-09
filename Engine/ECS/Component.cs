using Engine.Rendering;

namespace Engine.ECS;

public abstract class Component
{
    /// <summary>The entity this component is attached to.</summary>
    public Entity Entity { get; internal set; } = null!;

    /// <summary>Called once when the entity is added to a scene.</summary>
    public virtual void Initialize() { }

    /// <summary>Called every frame for game logic.</summary>
    public virtual void Update(double deltaTime) { }

    /// <summary>Called every frame for drawing.</summary>
    public virtual void Render(SpriteRenderer renderer) { }
}
