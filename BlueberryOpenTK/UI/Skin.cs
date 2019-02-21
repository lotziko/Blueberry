
using System.Collections.Generic;

namespace Blueberry.UI
{
    public partial class Skin
    {
        private static List<(string name, IDrawable drawable)> ProcessAtlas(TextureAtlas atlas)
        {
            var result = new List<(string name, IDrawable drawable)>();
            foreach(var r in atlas.regions)
            {
                if (r.splits != null)
                    result.Add((r.name, new NinePatchDrawable(atlas.FindNinePatch(r.name))));
                else
                    result.Add((r.name, new TextureRegionDrawable(atlas.FindRegion(r.name))));
            }
            return result;
        }

        public static Skin CreateDefaultSkin(IFont defFont, TextureAtlas atlas)
        {
            var skin = new Skin();

            var buttonStyle = new ButtonStyle()
            {
                over = new NinePatchDrawable(atlas.FindNinePatch("button-over")),
                down = new NinePatchDrawable(atlas.FindNinePatch("button-down")),
                up = new NinePatchDrawable(atlas.FindNinePatch("button")),
                focusedBorder = new NinePatchDrawable(atlas.FindNinePatch("border"))
            };
            skin.Add("default", buttonStyle);

            var textButtonStyle = new TextButtonStyle()
            {
                over = new NinePatchDrawable(atlas.FindNinePatch("button-over")),
                down = new NinePatchDrawable(atlas.FindNinePatch("button-down")),
                up = new NinePatchDrawable(atlas.FindNinePatch("button")),
                focusedBorder = new NinePatchDrawable(atlas.FindNinePatch("border")),
                font = defFont,
                fontColor = Col.White,
                disabledFontColor = Col.Gray
            };
            skin.Add("default", textButtonStyle);

            //var imageButtonStyle = new ImageButtonStyle()
            //{
            //    over = new NinePatchDrawable(atlas.FindNinePatch("button-over")),
            //    down = new NinePatchDrawable(atlas.FindNinePatch("button-down")),
            //    up = new NinePatchDrawable(atlas.FindNinePatch("button")),
            //    focusedBorder = new NinePatchDrawable(atlas.FindNinePatch("border")),
            //};
            //skin.Add("default", imageButtonStyle);

            //var closeWindowButtonStyle = new ImageButtonStyle()
            //{
            //    over = new NinePatchDrawable(atlas.FindNinePatch("button-over")),
            //    down = new NinePatchDrawable(atlas.FindNinePatch("button-red")),
            //    up = new NinePatchDrawable(atlas.FindNinePatch("button-window-bg")),
            //    imageUp = new TextureRegionDrawable(atlas.FindRegion("icon-close")),
            //    focusedBorder = new NinePatchDrawable(atlas.FindNinePatch("border")),
            //};
            //skin.Add("close-window", closeWindowButtonStyle);

            var labelStyle = new LabelStyle()
            {
                font = defFont,
                fontColor = Col.White
            };
            skin.Add("default", labelStyle);

            var textFieldStyle = new TextFieldStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("textfield")),
                backgroundOver = new NinePatchDrawable(atlas.FindNinePatch("textfield-over")),
                focusedBorder = new NinePatchDrawable(atlas.FindNinePatch("border")),
                cursor = new NinePatchDrawable(atlas.FindNinePatch("cursor")),
                //focusedBackground = new PrimitiveDrawable(new Col(51, 51, 51, 255)),
                //focusedFontColor = Col.White,
                selection = new TextureRegionDrawable(atlas.FindRegion("selection")),

                font = defFont,
                fontColor = Col.White,
                focusedFontColor = Col.White,
                disabledFontColor = Col.Gray,
                messageFontColor = Col.White
            };
            skin.Add("default", textFieldStyle);

            var progressBarStyle = new ProgressBarStyle()
            {
                background = new TextureRegionDrawable(atlas.FindRegion("progressbar")),
                knob = new TextureRegionDrawable(atlas.FindRegion("progressbar-filled")),
                knobBefore = new TextureRegionDrawable(atlas.FindRegion("progressbar-filled"))
            };
            skin.Add("default", progressBarStyle);

            var sliderStyle = new SliderStyle()
            {
                background = new TextureRegionDrawable(atlas.FindRegion("slider")),
                knob = new TextureRegionDrawable(atlas.FindRegion("slider-knob")),
                knobOver = new TextureRegionDrawable(atlas.FindRegion("slider-knob-over")),
                knobDown = new TextureRegionDrawable(atlas.FindRegion("slider-knob-down")),
                disabledKnob = new TextureRegionDrawable(atlas.FindRegion("slider-knob-disabled")),
            };
            skin.Add("default", sliderStyle);

            var splitPaneStyleVertical = new SplitPaneStyle()
            {
                handle = new TextureRegionDrawable(atlas.FindRegion("splitpane-vertical")),
                handleOver = new TextureRegionDrawable(atlas.FindRegion("splitpane-vertical-over")),
            };
            skin.Add("default-vertical", splitPaneStyleVertical);

            var splitPaneStyleHorizontal = new SplitPaneStyle()
            {
                handle = new TextureRegionDrawable(atlas.FindRegion("splitpane")),
                handleOver = new TextureRegionDrawable(atlas.FindRegion("splitpane-over")),
            };
            skin.Add("default-horizontal", splitPaneStyleHorizontal);

            var checkBoxStyle = new CheckBoxStyle()
            {
                checkBackground = new TextureRegionDrawable(atlas.FindRegion("vis-check")),
                checkBackgroundOver = new TextureRegionDrawable(atlas.FindRegion("vis-check-over")),
                checkBackgroundDown = new TextureRegionDrawable(atlas.FindRegion("vis-check-down")),
                tick = new TextureRegionDrawable(atlas.FindRegion("vis-check-tick")),
                tickDisabled = new TextureRegionDrawable(atlas.FindRegion("vis-check-tick-disabled")),
                focusedBorder = new NinePatchDrawable(atlas.FindNinePatch("border")),
                font = defFont,
                fontColor = Col.White,
                disabledFontColor = Col.Gray,
            };
            skin.Add("default", checkBoxStyle);

            var scrollPaneStyle = new ScrollPaneStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("border")),
                vScroll = new NinePatchDrawable(atlas.FindNinePatch("scroll")),
                vScrollKnob = new NinePatchDrawable(atlas.FindNinePatch("scroll-knob-vertical")),
                hScroll = new NinePatchDrawable(atlas.FindNinePatch("scroll-horizontal")),
                hScrollKnob = new NinePatchDrawable(atlas.FindNinePatch("scroll-knob-horizontal")),
            };
            skin.Add("default", scrollPaneStyle);

            var treeStyle = new TreeStyle()
            {
                minus = new TextureRegionDrawable(atlas.FindRegion("tree-minus")),
                plus = new TextureRegionDrawable(atlas.FindRegion("tree-plus")),
                selection = new NinePatchDrawable(atlas.FindNinePatch("tree-selection")),
                over = new TextureRegionDrawable(atlas.FindRegion("tree-over"))
            };
            skin.Add("default", treeStyle);

            var listBoxStyle = new ListBoxStyle()
            {
                background = new TextureRegionDrawable(atlas.FindRegion("select-box-list-bg")),
                font = defFont,
                selection = new NinePatchDrawable(atlas.FindNinePatch("padded-list-selection")),
                fontColorSelected = Col.White,
                fontColorUnselected = Col.White
            };
            skin.Add("default", listBoxStyle);

            //var selectBoxStyle = new SelectBoxStyle()
            //{
            //    listStyle = listBoxStyle,
            //    scrollStyle = scrollPaneStyle,
            //    background = new NinePatchDrawable(atlas.FindNinePatch("default-select")),
            //    //backgroundOpen = new PrimitiveDrawable(new Col(51, 51, 51, 255)),
            //    //backgroundOver = new PrimitiveDrawable(new Col(62, 62, 66, 255)),
            //    font = defFont,
            //    fontColor = Col.White,
            //    disabledFontColor = Col.Gray
            //};
            //skin.Add("default", selectBoxStyle);

            var windowStyle = new WindowStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("window")),
                titleFont = defFont,
                titleFontColor = Col.White
            };
            skin.Add("default", windowStyle);

            var resizableWindowStyle = new WindowStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("window-resizable")),
                titleFont = defFont,
                titleFontColor = Col.White
            };
            skin.Add("resizable", resizableWindowStyle);

            var dialogWindowStyle = new WindowStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("window-resizable")),
                //stageBackground = new PrimitiveDrawable(new Col(Col.Black, 0.45f)),
                titleFont = defFont,
                titleFontColor = Col.White
            };
            skin.Add("dialog", dialogWindowStyle);

            //var colorPickerStyle = new ColorPickerStyle
            //{
            //    background = new NinePatchDrawable(atlas.FindNinePatch("window")),
            //    titleFont = defFont,
            //    titleFontColor = Col.White,
            //    label = labelStyle,
            //    input = textFieldStyle,
            //    button = textButtonStyle,
            //    cross = new TextureRegionDrawable(atlas.FindRegion("color-picker-cross")),
            //    barSelector = new TextureRegionDrawable(atlas.FindRegion("color-picker-bar-selector")),
            //    barSelectorVertical = new TextureRegionDrawable(atlas.FindRegion("color-picker-bar-selector-vertical"))
            //};
            //skin.Add("default", colorPickerStyle);

            var popupMenuStyle = new PopupMenuStyle()
            {
                background = new NinePatchDrawable(atlas.FindNinePatch("button")),
                border = new NinePatchDrawable(atlas.FindNinePatch("border"))
            };
            skin.Add("default", popupMenuStyle);

            var menuItemStyle = new MenuItemStyle()
            {
                font = defFont,
                fontColor = Col.White,
                down = buttonStyle.down,
                up = buttonStyle.up,
                over = buttonStyle.over,
                focusedBorder = buttonStyle.focusedBorder,
                subMenu = new TextureRegionDrawable(atlas.FindRegion("sub-menu")),
            };
            skin.Add("default", menuItemStyle);

            var separatorStyle = new SeparatorStyle()
            {
                background = new TextureRegionDrawable(atlas.FindRegion("separator")),
                thickness = 2
            };
            skin.Add("default", separatorStyle);

            return skin;
        }
    }
}
