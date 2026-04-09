using Engine.Core;
using Engine.Rendering;

namespace Engine.ECS;

public class Entity
{
    public string Name { get; set; }
    public Transform Transform { get; set; } = new();

    private readonly List<Component> _components = new();

    public Entity(string name = "Entity")
    {
        Name = name;
    }

    /// <summary>Creates a component of type T and attaches it to this entity.</summary>
    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        component.Entity = this;
        _components.Add(component);
        return component;
    }

    /// <summary>Finds the first component of type T on this entity, or null.</summary>
    public T? GetComponent<T>() where T : Component
    {
        foreach (var component in _components)
        {
            if (component is T match)
                return match;
        }
        return null;
    }

    public void Initialize()
    {
        foreach (var component in _components)
            component.Initialize();
    }

    public void Update(double deltaTime)
    {
        foreach (var component in _components)
            component.Update(deltaTime);
    }

    public void Render(SpriteRenderer renderer)
    {
        foreach (var component in _components)
            component.Render(renderer);
    }
}
