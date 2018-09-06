using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class MenuItem : Button
    {
        //private static Vector2 tmpVector = new Vector2();

        //MenuItem is modified version of TextButton

        private MenuItemStyle style;

        private Image image;
        private bool generateDisabledImage = true;
        private Label label;
        private Color shortcutLabelColor;
        private Label shortcutLabel;
        private Image subMenuImage;
        private Cell subMenuIconCell;

        private PopupMenu subMenu;

        /** Menu that this item belongs to */
        internal PopupMenu containerMenu;

        public MenuItem(string text, IDrawable image, Skin skin, string stylename = "default") : this(text, new Image(image), skin.Get<MenuItemStyle>(stylename)) { }

        public MenuItem(string text, IDrawable image, MenuItemStyle style) : this(text, new Image(image), style) { }

        public MenuItem(string text, Image image, MenuItemStyle style) : base(style)
        {
            Init(text, image, style);
        }

        private void Init(string text, Image image, MenuItemStyle style)
        {
            this.style = style;
            this.image = image;
            //setSkin(VisUI.getSkin());
            //Sizes sizes = VisUI.getSizes();

            Defaults().Space(3);

            if (image != null) image.SetScaling(Scaling.Fit);
            Add(image);//.size(sizes.menuItemIconSize);

            label = new Label(text, new LabelStyle(style.font, style.fontColor));
            label.SetAlign(UI.Align.left);
            Add(label).Expand().Fill();

            Add(shortcutLabel = new Label("", new LabelStyle(style.font, style.fontColor))).PadLeft(10).Right();
            shortcutLabelColor = shortcutLabel.GetStyle().fontColor;

            subMenuIconCell = Add(subMenuImage = new Image(style.subMenu)).PadLeft(3).PadRight(3).Size(style.subMenu.MinWidth, style.subMenu.MinHeight);
            subMenuIconCell.SetElement(null);

            AddListener(new Change(this));
            AddListener(new Input(this));
        }

        #region Listeners

        private class Change : ChangeListener
        {
            private readonly MenuItem item;

            public Change(MenuItem item)
            {
                this.item = item;
            }

            public override void Changed(ChangeEvent ev, Element element)
            {
                if (item.subMenu != null)
                {//makes submenu item not clickable
                    ev.Stop();
                }
            }
        }

        private class Input : InputListener
        {
            private readonly MenuItem item;

            public Input(MenuItem item)
            {
                this.item = item;
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                if (item.subMenu != null)
                { //removes selection of child submenu if mouse moved to parent submenu
                    item.subMenu.SetActiveItem(null, false);
                    item.subMenu.SetActiveSubMenu(null);
                }

                if (item.subMenu == null || item.IsDisabled())
                { //hides last visible submenu (if any)
                    item.HideSubMenu();
                }
                else
                {
                    item.ShowSubMenu();
                }
            }
        }

        #endregion

        public void SetSubMenu(PopupMenu subMenu)
        {
            this.subMenu = subMenu;

            if (subMenu == null)
            {
                subMenuIconCell.SetElement(null);
            }
            else
            {
                subMenuIconCell.SetElement(subMenuImage);
            }
        }

        public PopupMenu GetSubMenu()
        {
            return subMenu;
        }

        internal void PackContainerMenu()
        {
            if (containerMenu != null) containerMenu.Pack();
        }

        public override void SetParent(Group parent)
        {
            base.SetParent(parent);
            if (parent is PopupMenu)
                containerMenu = (PopupMenu)parent;
            else
                containerMenu = null;
        }

        internal void HideSubMenu()
        {
            if (containerMenu != null)
            {
                containerMenu.SetActiveSubMenu(null);
            }
        }

        internal void ShowSubMenu()
        {
            var stage = GetStage();
            var pos = LocalToStageCoordinates(Vector2.Zero);

            float availableSpaceLeft = pos.X;
            float availableSpaceRight = stage.GetWidth() - (pos.X + GetWidth());
            bool canFitOnTheRight = pos.X + GetWidth() + subMenu.GetWidth() <= stage.GetWidth();
            float subMenuX;
            if (canFitOnTheRight || availableSpaceRight > availableSpaceLeft)
            {
                subMenuX = pos.X + GetWidth() - 1;
            }
            else
            {
                subMenuX = pos.X - subMenu.GetWidth() + 1;
            }

            subMenu.SetPosition(subMenuX, pos.Y - subMenu.GetHeight() + GetHeight());

            if (subMenu.GetY() > 0)
            {
                subMenu.SetY(subMenu.GetY() + subMenu.GetHeight() - GetHeight());
            }

            stage.AddElement(subMenu);
            containerMenu.SetActiveSubMenu(subMenu);
        }

        internal void FireChangeEvent()
        {
            var changeEvent = Pool<ChangeListener.ChangeEvent>.Obtain();
            Fire(changeEvent);
            Pool<ChangeListener.ChangeEvent>.Free(changeEvent);
        }

        public override ButtonStyle GetStyle()
        {
            return style;
        }

        public override void SetStyle(ButtonStyle style)
        {
            if (!(style is MenuItemStyle)) throw new ArgumentException("style must be a MenuItemStyle.");
            base.SetStyle(style);
            this.style = (MenuItemStyle)style;
            if (label != null)
            {
                TextButtonStyle textButtonStyle = (TextButtonStyle)style;
                LabelStyle labelStyle = label.GetStyle();
                labelStyle.font = textButtonStyle.font;
                labelStyle.fontColor = textButtonStyle.fontColor;
                label.SetStyle(labelStyle);
            }
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Color fontColor;
            if (IsDisabled() && style.disabledFontColor != Color.TransparentBlack)
                fontColor = style.disabledFontColor;
            else if (IsPressed() && style.downFontColor != Color.TransparentBlack)
                fontColor = style.downFontColor;
            else if (IsChecked() && style.checkedFontColor != Color.TransparentBlack)
                fontColor = (IsOver() && style.checkedOverFontColor != Color.TransparentBlack) ? style.checkedOverFontColor : style.checkedFontColor;
            else if (IsOver() && style.overFontColor != Color.TransparentBlack)
                fontColor = style.overFontColor;
            else
                fontColor = style.fontColor;
            if (fontColor != Color.TransparentBlack) label.GetStyle().fontColor = fontColor;

            if (IsDisabled())
                shortcutLabel.GetStyle().fontColor = style.disabledFontColor;
            else
                shortcutLabel.GetStyle().fontColor = shortcutLabelColor;

            if (image != null && generateDisabledImage)
            {
                if (IsDisabled())
                    image.SetColor(Color.Gray);
                else
                    image.SetColor(Color.White);
            }


            base.Draw(graphics, parentAlpha);
        }

        public override bool IsOver()
        {
            if (containerMenu == null || containerMenu.GetActiveItem() == null)
            {
                return base.IsOver();
            }
            else
            {
                return containerMenu.GetActiveItem() == this;
            }
        }

        public bool IsGenerateDisabledImage()
        {
            return generateDisabledImage;
        }

        /**
	     * Changes generateDisabledImage property, when true that function is enabled. When it is enabled and this MenuItem
	     * is disabled then image color will be changed to gray meaning that it is disabled, by default it is enabled.
	     */
        public void SetGenerateDisabledImage(bool generateDisabledImage)
        {
            this.generateDisabledImage = generateDisabledImage;
        }

        /**
	     * Set shortcuts text displayed in this menu item.
	     * This DOES NOT set actual hot key for this menu item, it only makes shortcut text visible in item.
	     * @param keycode from {@link Keys}.
	     */
        public MenuItem SetShortcut(Keys key)
        {
            return SetShortcut(key.GetChar() + "");
        }

        public string GetShortcut()
        {
            return shortcutLabel.GetText();
        }

        /**
	     * Set shortcuts text displayed in this menu item. This DOES NOT set actual hot key for this menu item,
	     * it only makes shortcut text visible in item.
	     * @param text text that will be displayed
	     * @return this object for the purpose of chaining methods
	     */
        public MenuItem SetShortcut(string text)
        {
            shortcutLabel.SetText(text);
            PackContainerMenu();
            return this;
        }

        //TODO
        /**
	     * Creates platform dependant shortcut text. Converts int keycodes to String text. Eg. Keys.CONTROL_LEFT,
	     * Keys.SHIFT_LEFT, Keys.F5 will be converted to Ctrl+Shift+F5 on Windows and Linux, and to ⌘⇧F5 on Mac.
	     * <p>
	     * CONTROL_LEFT and CONTROL_RIGHT are mapped to Ctrl. The same goes for Alt (ALT_LEFT, ALT_RIGHT) and Shift (SHIFT_LEFT, SHIFT_RIGHT).
	     * <p>
	     * This DOES NOT set actual hot key for this menu item, it only makes shortcut text visible in item.
	     * @param keycodes keycodes from {@link Keys} that are used to create shortcut text
	     * @return this object for the purpose of chaining methods
	     */
        public MenuItem SetShortcut(params Keys[] keys)
        {
            var str = "";
            for(int i = 0; i < keys.Length; i++)
            {
                if (i > 0)
                    str += " + ";
                str += keys[i].GetChar();
            }
            shortcutLabel.SetText(str);
            PackContainerMenu();
            return this;
        }

        public override void SetStage(Stage stage)
        {
            base.SetStage(stage);
            label.Invalidate(); //fixes issue with disappearing menu item after holding right mouse button and dragging down while opening menu
        }

        public Image GetImage()
        {
            return image;
        }

        public Cell GetImageCell()
        {
            return GetCell(image);
        }

        public Label GetLabel()
        {
            return label;
        }

        public Cell GetLabelCell()
        {
            return GetCell(label);
        }

        public string GetText()
        {
            return label.GetText();
        }

        public void SetText(string text)
        {
            label.SetText(text);
        }

        public Cell GetSubMenuIconCell()
        {
            return subMenuIconCell;
        }

        public Cell GetShortcutCell()
        {
            return GetCell(shortcutLabel);
        }

    }

    public class MenuItemStyle : TextButtonStyle
    {

        public IDrawable subMenu;

        public MenuItemStyle()
        {
        }

        public MenuItemStyle(IDrawable subMenu)
        {
            this.subMenu = subMenu;
        }

        public MenuItemStyle(MenuItemStyle style) : base(style)
        {
            this.subMenu = style.subMenu;
        }
    }
}
