using System.Collections.Generic;

namespace BlueberryCore
{
    public class Component
    {
        public Entity entity;

        public bool Enabled
        {
            get { if (entity != null) return _enabled && entity.enabled; return _enabled; }
            set { _enabled = value; }
        }

        bool _enabled = true;
        
        public virtual void Initialize() { }

    }
}
