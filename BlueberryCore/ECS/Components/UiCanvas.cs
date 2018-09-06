using BlueberryCore.UI;

namespace BlueberryCore
{
    public class UiCanvas : UpdatableRenderableComponent
    {
        public Stage stage;

        public override void Initialize()
        {
            stage = new Stage(entity.scene.camera)
            {
                entity = entity
            };
        }

        public override void Render(Graphics graphics, Camera camera)
        {
            stage.Draw(graphics);
        }

        public override void Update()
        {
            stage.Update((float)Core.time.ElapsedGameTime.TotalSeconds);
        }
    }
}
