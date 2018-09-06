using System;

namespace BlueberryCore
{
    public class Entity
    {
        public bool enabled;

        public Scene scene;

        public readonly Transform transform = new Transform();

        public readonly String name;

        public readonly ComponentList components;

        public void Update()
        {
            components.Update();
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
