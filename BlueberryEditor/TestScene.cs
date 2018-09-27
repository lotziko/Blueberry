using BlueberryCore;
using BlueberryCore.TextureAtlases;
using BlueberryCore.UI;
using BlueberryEditor.IDE;
using BlueberryEditor.IDE.ImageEditor;
using BlueberryEditor.UIElements;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
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
            canvas.stage.SetDebug(false);
            Input.SetInputProcessor(canvas.stage);
            
            var skin = Skin.CreateDefaultSkin();
            var atlas = Core.content.Load<TextureAtlas>("UI");

            Label label;
            ZoomableArea area;

            table.Add(area = new ZoomableArea(new ImageEditorCanvas(new ImageEditorController(atlas.GetTexture())))).Size(600, 600);
            table.Add(label = new Label("", skin));
            area.OnZoomChanged += (percent) =>
            {
                label.SetText(percent + "%");
            };
            /*
            var style = new FolderTreeStyle()
            {
                windowStyle = skin.Get<WindowStyle>(),
                scrollPaneStyle = skin.Get<ScrollPaneStyle>(),
                labelStyle = skin.Get<LabelStyle>(),
                treeStyle = skin.Get<TreeStyle>(),
                buttonStyle = skin.Get<ImageButtonStyle>(),
                fieldStyle = skin.Get<TextFieldStyle>(),
                arrowLeft = new TextureRegionDrawable("UI", "icon-bb-arrow-left"),
                arrowRight = new TextureRegionDrawable("UI", "icon-bb-arrow-right"),
                parentFolder = new TextureRegionDrawable("UI", "icon-folder-parent"),
                iconMaxWidth = 14,
            };
            style.icons.Add("unknown", new TextureRegionDrawable("UI", "icon-bb-file"));
            style.icons.Add(".png", new TextureRegionDrawable("UI", "icon-bb-image"));
            style.icons.Add("folder", new TextureRegionDrawable("UI", "icon-bb-folder"));

            var tree = new FolderTree("D:/SolidWorks/SOLIDWORKS/cefRuntime/locales/", style);
            table.Add(tree).Size(300, 300).Row();

            table.Add(new Palette(new PaletteStyle(skin.Get<ColorPickerStyle>())));
            */

            canvas.stage.AddElement(table);


            /*var proj = new ProjectResource("D:/testProject/", new ProjectData("Test"));
            var spr = new SpriteResource(proj.Path + "/sprites/", new SpriteData("testSpr"));
            proj.Add(spr);
            new ProjectWriter().Write(proj);
            new SpriteWriter().Write(spr);*/

            /*var proj = ProjectManager.Load("D:/testProject");
            var sprPathes = proj.Get("BBSprite");
            var sprResources = new List<SpriteResource>();
            foreach(var spr in sprPathes)
            {
                sprResources.Add(ResourceManager.Read<SpriteResource>( proj.Path + spr));
            }*/
            
            //ProjectManager.Load("D:/testProject");

        }
    }
}
