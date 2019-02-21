using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public class Menu : PopupMenu
    {
        private MenuBar menuBar;

        public TextButton openButton;
        public IDrawable buttonDefaultOver, buttonDefaultUp;

        private string title;

        public Menu(string title, Skin skin, string stylename = "default") : this(title, skin.Get<MenuStyle>(stylename))
        {
        }

        public Menu(string title, MenuStyle style) : base(style)
        {
            this.title = title;

            openButton = new TextButton(title, new TextButtonStyle(style.openButtonStyle));
            buttonDefaultUp = openButton.GetStyle().up;
            buttonDefaultOver = openButton.GetStyle().over;

            openButton.AddListener(new Listener(this));
        }

        #region Listener

        private class Listener : InputListener<Menu>
        {
            public Listener(Menu par) : base(par)
            {
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (par.menuBar.GetCurrentMenu() == par) {
                    par.menuBar.CloseMenu();
                    return true;
                }

                par.SwitchMenu();

                ev.Stop();
				return true;
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                if (par.menuBar.GetCurrentMenu() != null && par.menuBar.GetCurrentMenu() != par) par.SwitchMenu();
            }
        }

        #endregion

        public string GetTitle()
        {
            return title;
        }

        private void SwitchMenu()
        {
            menuBar.CloseMenu();
            ShowMenu();
        }

        private void ShowMenu()
        {
            var pos = new Vec2(0, 0);
            openButton.LocalToStageCoordinates(ref pos.X, ref pos.Y);
            SetPosition(pos.X, pos.Y + openButton.GetHeight());
            openButton.GetStage().AddElement(this);
            menuBar.SetCurrentMenu(this);
        }

        public override bool Remove()
        {
            var result = base.Remove();
            menuBar.SetCurrentMenu(null);
            return result;
        }

        /** Called by MenuBar when this menu is added to it */
        internal void SetMenuBar(MenuBar menuBar)
        {
            if (this.menuBar != null && menuBar != null) throw new Exception("Menu was already added to MenuBar");
            this.menuBar = menuBar;
        }

        internal TextButton GetOpenButton()
        {
            return openButton;
        }

        internal void SelectButton()
        {
            openButton.GetStyle().up = openButton.GetStyle().down;
            openButton.GetStyle().over = openButton.GetStyle().down;
        }

        internal void DeselectButton()
        {
            openButton.GetStyle().up = buttonDefaultUp;
            openButton.GetStyle().over = buttonDefaultOver;
        }
    }

    public class MenuStyle : PopupMenuStyle
    {
        public TextButtonStyle openButtonStyle;

        public MenuStyle()
        {
        }

        public MenuStyle(MenuStyle style) : base(style)
        {
            this.openButtonStyle = style.openButtonStyle;
        }

        public MenuStyle(IDrawable background, IDrawable border, TextButtonStyle openButtonStyle) : base(background, border)
        {
            this.openButtonStyle = openButtonStyle;
        }
    }
}