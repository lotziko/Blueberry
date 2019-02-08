
namespace Blueberry
{
    public abstract class UpdatableComponent : Component, IUpdatable
    {
        public int UpdateOrder { get { return updateOrder; } set { updateOrder = value; } }

        protected int updateOrder;

        public abstract void Update(float delta);
    }
}
