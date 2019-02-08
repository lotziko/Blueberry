using Blueberry;
using Blueberry.DataTools;
using Blueberry.UI;
using BlueberryOpenTK;
using BlueberryOpenTK.PipelineTools;
using System;

namespace OpenTKTest
{
    public class TestScene : Scene
    {
        Stage stage;
        DataBinding<float> binding = new DataBinding<float>();

        public override void Initialize()
        {
            var test = CreateEntity("test");
            //test.AddComponent(new TestComponent());
            
            var canvas = test.AddComponent(new UiCanvas());
            stage = canvas.stage;

            var t = new Table();
            t.FillParent = true;
            canvas.stage.AddElement(t);

            var atlas = Content.Load<TextureAtlas>(@"UI.bba");
            var font = new BitmapFont(@"C:\Program Files\GameMaker Studio 2\Fonts\OpenSans-Regular.ttf", 17);

            var labelStyle = new LabelStyle()
            {
                font = font,
                fontColor = Col.White
            };

            var tooltipStyle = new TooltipStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("default-pane"))
            };

            var sliderStyle = new SliderStyle()
            {
                background = new TextureRegionDrawable(atlas.FindRegion("slider")),
                knob = new TextureRegionDrawable(atlas.FindRegion("slider-knob")),
                knobOver = new TextureRegionDrawable(atlas.FindRegion("slider-knob-over")),
                knobDown = new TextureRegionDrawable(atlas.FindRegion("slider-knob-down")),
                disabledKnob = new TextureRegionDrawable(atlas.FindRegion("slider-knob-disabled")),
            };

            for(int i = 0; i < 10; i++)
            {
                var l = new Label("1", labelStyle);
                l.DataBinding = binding;
                t.Add(l).Pad(5);

                var s = new Slider(0, 10, 1, false, sliderStyle);
                s.DataBinding = binding;
                t.Add(s).Pad(5).Row();
            }

            var type = typeof(SliderStyle);
            var obj = System.Reflection.Assembly.GetAssembly(type).CreateInstance(type.FullName);

            //new Tooltip.Builder("test", labelStyle).Style(tooltipStyle).Target(s).Build();


            Input.InputProcessor = canvas.stage;

            //canvas.stage.DebugAll();
            /*
            

            

            var windowStyle = new WindowStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("window")),
                titleFont = font,
                titleFontColor = Col.White
            };

            var vscrollpaneStyle = new VerticalScrollPaneStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("border")),
                vScroll = new NinePatchDrawable(atlas.FindNinePatch("scroll")),
                vScrollKnob = new NinePatchDrawable(atlas.FindNinePatch("scroll-knob-vertical")),
                hScroll = new NinePatchDrawable(atlas.FindNinePatch("scroll-horizontal")),
                hScrollKnob = new NinePatchDrawable(atlas.FindNinePatch("scroll-knob-horizontal"))
            };

            var scrollpaneStyle = new ScrollPaneStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("border")),
                vScroll = new NinePatchDrawable(atlas.FindNinePatch("scroll")),
                vScrollKnob = new NinePatchDrawable(atlas.FindNinePatch("scroll-knob-vertical")),
                vScrollKnobOver = new NinePatchDrawable(atlas.FindNinePatch("border")),
                vScrollKnobDown = new NinePatchDrawable(atlas.FindNinePatch("border-error")),
                hScroll = new NinePatchDrawable(atlas.FindNinePatch("scroll-horizontal")),
                hScrollKnob = new NinePatchDrawable(atlas.FindNinePatch("scroll-knob-horizontal"))
            };

            var listBoxStyle = new ListBoxStyle()
            {
                background = new TextureRegionDrawable(atlas.FindRegion("select-box-list-bg")),
                font = font,
                selection = new NinePatchDrawable(atlas.FindNinePatch("padded-list-selection")),
                fontColorSelected = Col.White,
                fontColorUnselected = Col.White
            };

            var pane1 = new ListBox<string>(listBoxStyle);
            pane1.GetSelection().SetMultiple(true);
            for (int i = 0; i < 50; i++)
            {
                pane1.GetItems().Add("Workspace" + i);
            }
            var scroll1 = new VerticalScrollPane(pane1, vscrollpaneStyle);

            var window1 = new Window("test", windowStyle);
            window1.Add(scroll1).Expand().Fill();
            window1.SetMovable(true);
            window1.SetResizable(true);
            stage.AddElement(window1);

            var pane = new ListBox<string>(listBoxStyle);
            pane.GetSelection().SetMultiple(true);
            for (int i = 0; i < 50; i++)
            {
                pane.GetItems().Add("Workspace" + i);
            }
            var scroll = new ScrollPane(pane, scrollpaneStyle);
            scroll.SetFadeScrollBars(false);
             

            Input.InputProcessor = canvas.stage;
            */
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
