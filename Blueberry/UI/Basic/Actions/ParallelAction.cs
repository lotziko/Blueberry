using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public class ParallelAction : Action
    {
        protected List<Action> actions = new List<Action>(4);
        private bool complete;

        public ParallelAction()
        {
        }

        public ParallelAction(Action action1)
        {
            AddAction(action1);
        }

        public ParallelAction(Action action1, Action action2)
        {
            AddAction(action1);
            AddAction(action2);
        }

        public ParallelAction(Action action1, Action action2, Action action3)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
        }

        public ParallelAction(Action action1, Action action2, Action action3, Action action4)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
        }

        public ParallelAction(Action action1, Action action2, Action action3, Action action4, Action action5)
        {
            AddAction(action1);
            AddAction(action2);
            AddAction(action3);
            AddAction(action4);
            AddAction(action5);
        }

        public override bool Update(float delta)
        {
            if (complete) return true;
            complete = true;
            //Pool pool = getPool();
            //setPool(null); // Ensure this action can't be returned to the pool while executing.
            try
            {
                for (int i = 0, n = actions.Count; i < n && element != null; i++)
                {
                    Action currentAction = actions[i];
                    if (currentAction.GetElement() != null && !currentAction.Update(delta)) complete = false;
                    if (element == null) return true; // This action was removed.
                }
                return complete;
            }
            finally
            {
                //SetPool(pool);
            }
        }

        public override void Restart()
        {
            complete = false;
            for (int i = 0, n = actions.Count; i < n; i++)
                actions[i].Restart();
        }

        public override void Reset()
        {
            base.Reset();
            lock(actions)
                actions.Clear();
        }

        public void AddAction(Action action)
        {
            lock (actions)
                actions.Add(action);
            if (element != null) action.SetElement(element);
        }

        public override void SetElement(Element element)
        {
            lock (actions)
                for (int i = 0, n = actions.Count; i < n; i++)
                    actions[i].SetElement(element);
            base.SetElement(element);
        }

        public List<Action> GetActions()
        {
            return actions;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder(64);
            buffer.Append(base.ToString());
            buffer.Append('(');
            for (int i = 0, n = actions.Count; i < n; i++)
            {
                if (i > 0) buffer.Append(", ");
                buffer.Append(actions[i]);
            }
            buffer.Append(')');
            return buffer.ToString();
        }
    }
}
