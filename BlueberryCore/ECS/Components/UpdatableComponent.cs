using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public abstract class UpdatableComponent : Component, IUpdatable
    {
        public int UpdateOrder { get { return updateOrder; } set { updateOrder = value; } }

        protected int updateOrder;

        public abstract void Update();
    }
}
