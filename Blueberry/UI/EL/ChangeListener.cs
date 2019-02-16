using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.UI
{
    public abstract class ChangeListener : IEventListener
    {
        public bool Handle(Event e)
        {
            if (!(e is ChangeEvent)) return false;
                Changed((ChangeEvent)e, e.GetTarget());
		    return false;
        }

        public abstract void Changed(ChangeEvent ev, Element element);

        public class ChangeEvent : Event
        {
        }
    }

    public abstract class ChangeListener<T> : ChangeListener
    {
        protected T par;

        public ChangeListener(T par)
        {
            this.par = par;
        }
    }
}
