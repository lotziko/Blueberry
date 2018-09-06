using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public class ComponentList
    {
        static UpdateOrderComparer comparer = new UpdateOrderComparer();

        Entity _entity;

        public readonly List<Component> components = new List<Component>();

        public readonly List<IUpdatable> updatables = new List<IUpdatable>();

        List<Component> toAdd = new List<Component>();

        List<Component> toRemove = new List<Component>();

        bool updateOrderChanged = false;

        public ComponentList(Entity entity)
        {
            _entity = entity;
            updatables = new List<IUpdatable>();
        }
        
        public void Update()
        {
            UpdateLists();

            foreach (IUpdatable u in updatables)
            {
                u.Update();
            }
        }

        public void Clear()
        {
            foreach(Component c in components)
            {
                Remove(c);
            }
        }

        public void Add(Component component)
        {
            toAdd.Add(component);
        }

        public void Remove(Component component)
        {
            toRemove.Add(component);
        }

        public void UpdateLists()
        {
            if (toRemove.Count > 0)
            {
                foreach(Component c in toRemove)
                {
                    components.Remove(c);
                    HandleRemove(c);
                }
                toRemove.Clear();
            }
            if (toAdd.Count > 0)
            {
                foreach (Component c in toAdd)
                {
                    if (c is IUpdatable)
                        updatables.Add(c as IUpdatable);

                    if (c is IRenderable)
                        _entity.scene.renderables.Add(c as IRenderable);
                }
                toAdd.Clear();
                updateOrderChanged = true;
            }
            if (updateOrderChanged)
            {
                updatables.Sort(comparer);
                updateOrderChanged = false;
            }
        }

        private void HandleRemove(Component component)
        {
            if (component is IUpdatable)
            {
                updatables.Remove(component as IUpdatable);
            }
            if (component is IRenderable)
            {
                _entity.scene.renderables.Remove(component as IRenderable);
            }
        }
    }
}
