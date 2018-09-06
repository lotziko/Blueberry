using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public interface IUpdatable
    {
        int UpdateOrder { get; set; }

        void Update();
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
        public void Update()
        {
            foreach(IUpdatable u in this)
            {
                u.Update();
            }
        }
    }
}
