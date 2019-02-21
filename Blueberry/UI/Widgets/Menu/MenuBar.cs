using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public class MenuBar
    {
        private Table mainTable;
        private Table menuItems;

        private Menu currentMenu;
        private List<Menu> menus = new List<Menu>();

        private IMenuBarListener menuListener;

        public MenuBar(Skin skin, string stylename = "default") : this(skin.Get<MenuBarStyle>(stylename))
        {
        }

        public MenuBar(MenuBarStyle style)
        {
            menuItems = new Table();

            mainTable = new MainTable(this);

            mainTable.Left();
            mainTable.Add(menuItems);
            mainTable.SetBackground(style.background);
        }

        private class MainTable : Table
        {
            private MenuBar par;

            public MainTable(MenuBar par) : base()
            {
                this.par = par;
            }

            protected override void SizeChanged()
            {
                base.SizeChanged();
                par.CloseMenu();
            }
        }

        public void AddMenu(Menu menu)
        {
            menus.Add(menu);
            menu.SetMenuBar(this);
            menuItems.Add(menu.GetOpenButton());
        }

        public bool RemoveMenu(Menu menu)
        {
            var removed = menus.Remove(menu);

            if (removed)
            {
                menu.SetMenuBar(null);
                menuItems.RemoveElement(menu.GetOpenButton());
            }

            return removed;
        }

        public void InsertMenu(int index, Menu menu)
        {
            menus.Insert(index, menu);
            menu.SetMenuBar(this);
            Rebuild();
        }

        private void Rebuild()
        {
            menuItems.Clear();
            foreach (var menu in menus)
                menuItems.Add(menu.GetOpenButton());
        }

        /** Closes currently opened menu (if any). Used by framework and typically there is no need to call this manually */
        public void CloseMenu()
        {
            if (currentMenu != null)
            {
                currentMenu.DeselectButton();
                currentMenu.Remove();
                currentMenu = null;
            }
        }

        internal Menu GetCurrentMenu()
        {
            return currentMenu;
        }

        internal void SetCurrentMenu(Menu newMenu)
        {
            if (currentMenu == newMenu) return;
            if (currentMenu != null)
            {
                currentMenu.DeselectButton();
                if (menuListener != null) menuListener.MenuClosed(currentMenu);
            }
            if (newMenu != null)
            {
                newMenu.SelectButton();
                if (menuListener != null) menuListener.MenuOpened(newMenu);
            }
            currentMenu = newMenu;
        }

        public void SetMenuListener(IMenuBarListener menuListener)
        {
            this.menuListener = menuListener;
        }

        /** Returns table containing all menus that should be added to Stage, typically with expandX and fillX properties. */
        public Table GetTable()
        {
            return mainTable;
        }
    }

    public interface IMenuBarListener
    {
        void MenuOpened(Menu menu);

        void MenuClosed(Menu menu);
    }

    public class MenuBarStyle
    {
        public IDrawable background;

        public MenuBarStyle()
        {
        }

        public MenuBarStyle(MenuBarStyle style)
        {
            this.background = style.background;
        }

        public MenuBarStyle(IDrawable background)
        {
            this.background = background;
        }
    }
}
