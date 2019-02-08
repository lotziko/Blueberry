using System.Collections.Generic;

namespace Blueberry
{
    public interface IUpdatable
    {
        int UpdateOrder { get; set; }

        void Update(float delta);
    }

    public class UpdateOrderComparer : IComparer<IUpdatable>
    {
        public int Compare(IUpdatable x, IUpdatable y)
        {
            return x.UpdateOrder.CompareTo(y.UpdateOrder);
        }
    }

    public class UpdatableList : List<IUpdatable>
    {
        public void Update(float delta)
        {
            foreach(IUpdatable u in this)
            {
                u.Update(delta);
            }
        }
    }
}
