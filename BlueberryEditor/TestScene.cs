using BlueberryCore;
using BlueberryCore.TextureAtlases;
using BlueberryCore.UI;
using BlueberryEditor.UIElements;
using Microsoft.Xna.Framework;
using static BlueberryCore.UI.Tree;

namespace BlueberryEditor
{
    class TestScene : Scene
    {
        public override void Initialize()
        {
            var test = CreateEntity("test");
            var canvas = test.AddComponent(new UiCanvas());

            var table = new Table();
            table.FillParent = true;
            canvas.stage.SetDebug(true);
            Input.SetInputProcessor(canvas.stage);
            
            var skin = Skin.CreateDefaultSkin();
            Label label;
            ZoomableArea area;
            
            table.Add(area = new ZoomableArea()).Size(300, 300);
            table.Add(label = new Label("", skin));
            area.OnZoomChanged += (percent) =>
            {
                label.SetText(percent + "%");
            };

            canvas.stage.AddElement(table);
        }
    }
}
