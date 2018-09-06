using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI.Actions
{
    public class SequenceAction : ParallelAction
    {
        private int index;

        public SequenceAction()
        {
        }

        public SequenceAction(Action action1)
        {
            AddAction(action1);
        }

        public SequenceAction(Action action1, Action action2)
        {
            AddAction(action1);
            AddAction(action2);
        }

        public SequenceAction(Action action1, Action action2, Action action3)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
        }

        public SequenceAction(Action action1, Action action2, Action action3, Action action4)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
        }

        public SequenceAction(Action action1, Action action2, Action action3, Action action4, Action action5)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
            AddAction(action5);
        }

        public override bool Update(float delta)
        {
            if (index >= actions.Count) return true;
            //Pool pool = getPool();
            //setPool(null); // Ensure this action can't be returned to the pool while executings.
            try
            {
                if (actions[index].Update(delta))
                {
                    if (element == null) return true; // This action was removed.
                    index++;
                    if (index >= actions.Count) return true;
                }
                return false;
            }
            finally
            {
                //setPool(pool);
            }
        }

        public override void Restart()
        {
            base.Restart();
            index = 0;
        }
    }
}
