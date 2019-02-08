using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry
{
    public class EntityList
    {
        public readonly List<Entity> entities = new List<Entity>();

        List<Entity> toAdd = new List<Entity>();

        List<Entity> toRemove = new List<Entity>();

        public void Add(Entity entity)
        {
            toAdd.Add(entity);
        }

        public void Remove(Entity entity)
        {
            toRemove.Add(entity);
        }

        public void UpdateLists()
        {
            if (toRemove.Count > 0)
            {
                foreach (Entity e in toRemove)
                {
                    entities.Remove(e);
                }
                toRemove.Clear();
            }
            if (toAdd.Count > 0)
            {
                foreach (Entity e in toAdd)
                {
                    entities.Add(e);
                }
                toAdd.Clear();
            }
        }

        public void Update(float delta)
        {
            UpdateLists();

            foreach (Entity e in entities)
            {
                e.Update(delta);
            }
        }
    }
}
