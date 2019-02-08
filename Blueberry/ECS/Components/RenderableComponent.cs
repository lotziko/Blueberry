
namespace Blueberry
{
    public abstract class RenderableComponent : Component, IRenderable
    {
        public int RenderLayer { get { return renderLayer; } set { renderLayer = value; } }

        protected int renderLayer;

        public abstract void Render(Graphics graphics, Camera camera);
    }
}
