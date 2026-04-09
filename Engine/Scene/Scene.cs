using Engine.ECS;
using Engine.Rendering;

namespace Engine.Scene;

public class Scene
{
    public string Name { get; set; }

    private readonly List<Entity> _entities = new();

    public Scene(string name = "Scene")
    {
        Name = name;
    }

    /// <summary>Add an entity to this scene. Calls Initialize on all its components.</summary>
    public Entity AddEntity(Entity entity)
    {
        _entities.Add(entity);
        entity.Initialize();
        return entity;
    }

    /// <summary>Create a new entity, add it to the scene, and return it.</summary>
    public Entity CreateEntity(string name = "Entity")
    {
        var entity = new Entity(name);
        return AddEntity(entity);
    }

    public void Update(double deltaTime)
    {
        foreach (var entity in _entities)
            entity.Update(deltaTime);
    }

    public void Render(SpriteRenderer renderer)
    {
        foreach (var entity in _entities)
            entity.Render(renderer);
    }
}
