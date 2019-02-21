using Blueberry;
using Blueberry.DataTools;
using Blueberry.UI;
using BlueberryOpenTK;

namespace OpenTKTest
{
    public class TestScene : Scene
    {
        Stage stage;
        DataBinding<float> binding = new DataBinding<float>();

        public override void Initialize()
        {
            new TexturePacker(new Settings()).Pack(@"C:\Users\lotziko\Dropbox\BlueberryUIAssets", @"Content/", "UI", false, new BBAtlasExporter());
            var test = CreateEntity("test");
            //test.AddComponent(new TestComponent());
            
            var canvas = test.AddComponent(new UiCanvas());
            stage = canvas.stage;

            var t = new Table();
            t.FillParent = true;
            canvas.stage.AddElement(t);

            var root = "";

            var atlas = Content.Load<TextureAtlas>(root + "Content/UI.bba");
            var preview = Content.Load<TextureAtlas>(root + "Content/UI.bba");//C:\Users\lotziko\Dropbox\tst\1.atlas");
            var font = new FreeTypeFont(root + "Content/OpenSans-Regular.ttf", /*17*/20);
            var skin = new Skin.Builder(root + "Content/UI.json").Font("default", font).Atlas(atlas).Build();
            
            t.Add(new AtlasPackerTable(new AtlasController(preview), skin));

            Input.InputProcessor = canvas.stage;
           
            //canvas.stage.DebugAll();
        }

        public override void OnResize(int width, int height)
        {
            //binding.Value++;
            base.OnResize(width, height);
            stage?.Viewport.Update(width, height, true);
        }
    }
}
