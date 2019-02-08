using Blueberry;
using Blueberry.DataTools;
using BlueberryOpenTK;

namespace OpenTKTest
{
    public class TestComponent : RenderableComponent
    {
        NinePatch win;
        BitmapFont font;
        TextureAtlas atlas;

        public TestComponent()
        {
            new TexturePacker(new Settings() { maxWidth = 485, maxHeight = 275, pot = false }).Pack(@"C:\Users\lotziko\Dropbox\NewTomorrowGraphics", @"C:\Users\lotziko", "test", true);
            //atlas = Content.Load<TextureAtlas>(@"C:\Users\lotziko\Dropbox\tst\tst.atlas");
            //font = new BitmapFont(@"C:\Windows\Fonts\consola.ttf", 50);
        }

        public override void Render(Graphics graphics, Camera camera)
        {
            graphics.Begin();
            //graphics.DrawTexture(atlas.GetTexture(), 0, 0);
            graphics.End();
        }
    }
}
