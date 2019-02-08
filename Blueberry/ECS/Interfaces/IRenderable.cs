using System.Collections.Generic;

namespace Blueberry
{
    public interface IRenderable
    {
        int RenderLayer { get; set; }

        void Render(Graphics graphics, Camera camera);
    }

    public class RenderOrderComparer : Comparer<IRenderable>
    {
        public override int Compare(IRenderable x, IRenderable y)
        {
            return y.RenderLayer.CompareTo(x.RenderLayer);
        }
    }

    public class RenderableList : List<IRenderable>
    {
        private static RenderOrderComparer comparer = new RenderOrderComparer();

        public new void Add(IRenderable r)
        {
            base.Add(r);
            Sort(comparer);
        }

        public void Render(Graphics graphics, Camera camera)
        {
            foreach(IRenderable r in this)
            {
                r.Render(graphics, camera);
            }
        }
    }
}
