using System;

namespace Blueberry
{
    public class Entity
    {
        public bool enabled;

        public Scene scene;

        public readonly Transform transform = new Transform();

        public readonly string name;

        public readonly ComponentList components;

        public void Update(float delta)
        {
            components.Update(delta);
        }

        public T AddComponent<T>(T component) where T : Component
        {
            components.Add(component);
            component.entity = this;
            component.Initialize();
            return component;
        }

        public void Destroy()
        {
            scene.RemoveEntity(this);
        }

        public Entity(String name)
        {
            this.name = name;
            components = new ComponentList(this);
        }
    }
}
