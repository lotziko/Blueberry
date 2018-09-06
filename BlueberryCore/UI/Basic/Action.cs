using System;

namespace BlueberryCore.UI
{
    public class Action : IPoolable
    {
        protected Element element;

        protected Element target;
        
        public virtual bool Update(float delta)
        {
            return false;
        }

        public virtual void Restart() { }

        public virtual void SetElement(Element element)
        {
            this.element = element;
            if (target == null) SetTarget(element);
            if (element == null)
            {
                Pool<Action>.Free(this);
            }
        }

        public Element GetElement() => element;

        public virtual void SetTarget(Element target)
        {
            this.target = target;
        }

        public Element GetTarget() => target;

        public virtual void Reset()
        {
            element = null;
            target = null;
            Restart();
        }

        public override string ToString()
        {
            String name = GetType().Name;
            int dotIndex = name.LastIndexOf('.');
            if (dotIndex != -1) name = name.Substring(dotIndex + 1);
            if (name.EndsWith("Action")) name = name.Substring(0, name.Length - 6);
            return name;
        }
    }
}
