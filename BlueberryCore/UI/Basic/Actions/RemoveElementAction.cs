using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI.Actions
{
    public class RemoveElementAction : Action
    {
        private bool removed;

        public override bool Update(float delta)
        {
            if (!removed)
            {
                removed = true;
                target.Remove();
            }
            return true;
        }

        public override void Restart()
        {
            removed = false;
        }
    }
}
