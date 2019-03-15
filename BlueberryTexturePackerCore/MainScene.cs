using Blueberry;
using Blueberry.DataTools;
using Blueberry.UI;

namespace BlueberryTexturePackerCore
{
    public class MainScene : Scene
    {
        private Stage stage;

        public override void Initialize()
        {
            base.Initialize();

            //new TexturePacker(new Settings()).Pack(@"C:\Users\lotziko\Dropbox\BlueberryUIAssets", "Resources/", "UI", false, new BBAtlasExporter());

            var entity = CreateEntity("main");
            var canvas = entity.AddComponent(new UiCanvas());
            stage = canvas.stage;

            var atlas = Content.Load<TextureAtlas>("Resources/UI.bba");
            var font = new FreeTypeFont("Resources/OpenSans-Regular.ttf", 20);
            var skin = new Skin.Builder("Resources/UI.json").Font("default", font).Atlas(atlas).Build();

            var table = new MainTable(new AtlasModel("Resources/UI.bba"), skin)
            {
                FillParent = true
            };
            canvas.stage.AddElement(table);
            //canvas.stage.SetDebug(true);

            Input.InputProcessor = canvas.stage;
        }

        public override void OnResize(int width, int height)
        {
            base.OnResize(width, height);
            stage?.Viewport.Update(width, height, true);
        }
    }
}
