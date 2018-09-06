using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
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
}
