using BlueberryCore;
using BlueberryCore.TextureAtlases;

namespace BlueberryEditor
{
    public class TestComponent : RenderableComponent
    {
        private TextureRegion test;

        public override void Initialize()
        {
            base.Initialize();
            test = Core.content.Load<TextureAtlas>("UI").FindRegion("bill");
        }

        public override void Render(Graphics graphics, Camera camera)
        {
            test.DrawTiled(graphics, 0, 0, 1000, 1000);
        }
    }
}
