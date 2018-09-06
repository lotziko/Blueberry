using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public abstract class UpdatableRenderableComponent : Component, IUpdatable, IRenderable
    {
        public int RenderLayer { get { return renderLayer; } set { renderLayer = value; } }
        public int UpdateOrder { get { return updateOrder; } set { updateOrder = value; } }

        protected int renderLayer;
        protected int updateOrder;

        public abstract void Render(Graphics graphics, Camera camera);
        public abstract void Update();
    }
}
