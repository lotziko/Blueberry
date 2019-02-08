
using Blueberry.UI;

namespace Blueberry
{
    public class UiCanvas : UpdatableRenderableComponent
    {
        public Stage stage;

        public override void Initialize()
        {
            stage = new Stage(new ScreenViewport())
            {
                //entity = entity
            };
        }

        public override void Render(Graphics graphics, Camera camera)
        {
            stage.Draw(graphics);
        }

        public override void Update(float delta)
        {
            stage.Update(delta);
        }
    }
}
