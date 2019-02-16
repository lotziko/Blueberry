using System;

namespace Blueberry.UI
{
    public class PopupMenu : Table
    {
        private static Vec2 tmpVector = new Vec2();

        private PopupMenuStyle style;
        private IPopupMenuListener listener;

        private InputListener stageListener;
        private InputListener sharedMenuItemInputListener;

        private ChangeListener sharedMenuItemChangeListener;

        private InputListener defaultInputListener;
        /** The parent sub-menu, that this popup menu belongs to or null if this sub menu is root */
        private PopupMenu parentSubMenu;

        /** The current sub-menu, set by MenuItem */
        private PopupMenu activeSubMenu;
        private MenuItem activeItem;

        public PopupMenu(Skin skin, string stylename = "default") : this(skin.Get<PopupMenuStyle>(stylename)) { }

        public PopupMenu(PopupMenuStyle style)
        {
            this.style = style;
            SetTouchable(Touchable.Enabled);
            Pad(0);
            SetBackground(style.background);
            CreateListeners();
        }

        /**
	     * Removes every instance of {@link PopupMenu} form {@link Stage} actors.
	     * <p>
	     * Generally called from {@link ApplicationListener#resize(int, int)} to remove menus on resize event.
	     */
        public static void RemoveEveryMenu(Stage stage)
        {
            foreach (var element in stage.GetElements())
            {
                if (element is PopupMenu menu)
                {
                    menu.RemoveHierarchy();
                }
            }
        }

        private void CreateListeners()
        {
            stageListener = new StageListener(this);
            sharedMenuItemInputListener = new SharedMenuItemInputListener(this);
            sharedMenuItemChangeListener = new SharedMenuItemChangeListener(this);
        }

        #region Listeners

        private class StageListener : InputListener
        {
            private readonly PopupMenu menu;

            public StageListener(PopupMenu menu)
            {
                this.menu = menu;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (menu.GetRootMenu().SubMenuStructureContains(x, y) == false)
                {
                    menu.Remove();
                }
                return true;
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                var elements = menu.GetElements();

                if (elements.Count == 0 || menu.activeSubMenu != null) return false;

                if (keycode == (int)Key.Down)
                {
                    menu.SelectNextItem();
                }

                if (menu.activeItem == null) return false;

                if (keycode == (int)Key.Up)
                {
                    menu.SelectPreviousItem();
                }

                if (keycode == (int)Key.Left && menu.activeItem.containerMenu.parentSubMenu != null)
                {
                    menu.activeItem.containerMenu.parentSubMenu.SetActiveSubMenu(null);
                }

                if (keycode == (int)Key.Right && menu.activeItem.GetSubMenu() != null)
                {
                    menu.activeItem.ShowSubMenu();
                    menu.activeSubMenu.SelectNextItem();
                }

                if (keycode == (int)Key.Enter)
                {
                    menu.activeItem.FireChangeEvent();
                }

                return false;
            }
        }

        private class SharedMenuItemInputListener : InputListener
        {
            private readonly PopupMenu menu;

            public SharedMenuItemInputListener(PopupMenu menu)
            {
                this.menu = menu;
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                if (pointer == -1 && ev.GetListenerElement() is MenuItem) {
                    var item = (MenuItem)ev.GetListenerElement();
                    if (item.IsDisabled() == false)
                    {
                        menu.SetActiveItem(item, false);
                    }
                }
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (pointer == -1 && ev.GetListenerElement() is MenuItem) {
                    if (menu.activeSubMenu != null) return;

                    var item = (MenuItem)ev.GetListenerElement();
                    if (item == menu.activeItem)
                    {
                        menu.SetActiveItem(null, false);
                    }
                }
            }
        }

        private class SharedMenuItemChangeListener : ChangeListener
        {
            private readonly PopupMenu menu;

            public SharedMenuItemChangeListener(PopupMenu menu)
            {
                this.menu = menu;
            }

            public override void Changed(ChangeEvent ev, Element element)
            {
                if (ev.IsStopped() == false) menu.RemoveHierarchy();
            }
        }

        #endregion

        private PopupMenu GetRootMenu()
        {
            if (parentSubMenu != null) return parentSubMenu.GetRootMenu();
            return this;
        }

        private bool SubMenuStructureContains(float x, float y)
        {
            if (Contains(x, y)) return true;
            if (activeSubMenu != null) return activeSubMenu.SubMenuStructureContains(x, y);
            return false;
        }

        private void RemoveHierarchy()
        {
            if (activeItem != null && activeItem.containerMenu.parentSubMenu != null)
            {
                activeItem.containerMenu.parentSubMenu.RemoveHierarchy();
            }
            Remove();
        }

        private void SelectNextItem()
        {
            var elements = GetElements();
            if (elements.Count == 0) return;
            int startIndex = activeItem == null ? 0 : elements.IndexOf(activeItem) + 1;
            for (int i = startIndex; ; i++)
            {
                if (i >= elements.Count) i = 0;
                var element = elements[i];
                if (element is MenuItem && ((MenuItem)element).IsDisabled() == false)
                {
                    SetActiveItem((MenuItem)element, true);
                    break;
                }
            }
        }

        private void SelectPreviousItem()
        {
            var elements = GetElements();
            if (elements.Count == 0) return;
            int startIndex = elements.IndexOf(activeItem) - 1;
            for (int i = startIndex; ; i--)
            {
                if (i == -1) i = elements.Count - 1;
                var element = elements[i];
                if (element is MenuItem && ((MenuItem)element).IsDisabled() == false)
                {
                    SetActiveItem((MenuItem)element, true);
                    break;
                }
            }
        }

        public override Cell Add(Element element)
        {
            if (element is MenuItem) {
                throw new ArgumentException("MenuItems can be only added to PopupMenu by using addItem(MenuItem) method");
            }
            return base.Add(element);
        }

        public void AddItem(MenuItem item)
        {
            base.Add(item).FillX().ExpandX().Row();
            Pack();
            item.AddListener(sharedMenuItemChangeListener);
            item.AddListener(sharedMenuItemInputListener);
        }

        public void AddSeparator(SeparatorStyle style)
        {
            Add(new Separator(style)).PadTop(2).PadBottom(2).Fill().Expand().Row();
        }

        public void AddSeparator(Skin skin, string stylename = "default")
        {
            AddSeparator(skin.Get<SeparatorStyle>(stylename));
        }

        /**
	     * Returns input listener that can be added to scene2d actor. When right mouse button is pressed on that actor,
	     * menu will be displayed
	     */
        public InputListener GetDefaultInputListener()
        {
            return GetDefaultInputListener(0);
        }

        public InputListener GetDefaultInputListener(int mouseButton)
        {
            if (defaultInputListener == null)
            {
                defaultInputListener = new DefaultInputListener(this, mouseButton);
            }
            return defaultInputListener;
        }

        #region Listener

        private class DefaultInputListener : InputListener
        {
            private readonly int mouseButton;
            private readonly PopupMenu menu;
            public DefaultInputListener(PopupMenu menu, int mouseButton)
            {
                this.menu = menu;
                this.mouseButton = (int)mouseButton;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                return true;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (ev.GetButton() == mouseButton)
                    menu.ShowMenu(ev.GetStage(), ev.GetStageX(), ev.GetStageY());
            }
        }

        #endregion


        public override void Draw(Graphics graphics, float parentAlpha)
        {
            base.Draw(graphics, parentAlpha);
            if (style.border != null) style.border.Draw(graphics, GetX(), GetY(), GetWidth(), GetHeight(), color);
        }

        /**
         * Shows menu as given stage coordinates
         * @param stage stage instance that this menu is being added to
         * @param x stage x position
         * @param y stage y position
         */
        public void ShowMenu(Stage stage, float x, float y)
        {
            SetPosition(x, y/* - getHeight()*/);
            if (stage.Height - GetY() > stage.Height) SetY(GetY() + GetHeight());
            //ActorUtils.KeepWithinStage(stage, this);
            stage.AddElement(this);
        }

        /**
	     * Shows menu below (or above if not enough space) given actor.
	     * @param stage stage instance that this menu is being added to
	     * @param actor used to get calculate menu position in stage, menu will be displayed above or below it
	     */
        public void ShowMenu(Stage stage, Element element)
        {
            var borderSize = style == null ? 1 : style.borderSize;
            var pos = new Vec2(0, 0);
            element.LocalToStageCoordinates(ref pos.X, ref pos.Y);
            float menuY;
            if (pos.Y + GetHeight() > stage.Height)
            {
                menuY = pos.Y - borderSize - GetHeight();
            }
            else
            {
                menuY = pos.Y + borderSize + element.GetHeight();
            }
            ShowMenu(stage, pos.X, menuY);
        }

        public bool Contains(float x, float y)
        {
            return GetX() < x && GetX() + GetWidth() > x && GetY() < y && GetY() + GetHeight() > y;
        }

        /** Called by framework, when PopupMenu is added to MenuItem as submenu */
        internal void SetActiveSubMenu(PopupMenu newSubMenu)
        {
            if (activeSubMenu == newSubMenu) return;
            if (activeSubMenu != null) activeSubMenu.Remove();
            activeSubMenu = newSubMenu;
            if (newSubMenu != null)
            {
                newSubMenu.SetParentMenu(this);
            }
        }

        public override void SetStage(Stage stage)
        {
            base.SetStage(stage);
            if (stage != null) stage.AddListener(stageListener);
        }

        public override bool Remove()
        {
            if (GetStage() != null) GetStage().RemoveListener(stageListener);
            if (activeSubMenu != null) activeSubMenu.Remove();
            SetActiveItem(null, false);
            parentSubMenu = null;
            activeSubMenu = null;
            return base.Remove();
        }

        internal void SetActiveItem(MenuItem newItem, bool keyboardChange)
        {
            activeItem = newItem;
            if (listener != null) listener.ActiveItemChanged(newItem, keyboardChange);
        }

        public MenuItem GetActiveItem()
        {
            return activeItem;
        }

        internal void SetParentMenu(PopupMenu parentSubMenu)
        {
            this.parentSubMenu = parentSubMenu;
        }

        public IPopupMenuListener GetListener()
        {
            return listener;
        }

        public void SetListener(IPopupMenuListener listener)
        {
            this.listener = listener;
        }

        /**
	     * Listener used to get events from {@link PopupMenu}.
	     * @since 1.0.2
	     */
        public interface IPopupMenuListener
        {
            /**
             * Called when active menu item (the highlighted one) has changed. This can't be used to listen when
             * {@link MenuItem} was pressed, add {@link ChangeListener} to {@link MenuItem} directly to achieve this.
             * @param newActiveItem new item that is now active. May be null.
             * @param changedByKeyboard whether the change occurred by keyboard (arrows keys) or by mouse.
             */
            void ActiveItemChanged(MenuItem newActiveItem, bool changedByKeyboard);
        }

    }

    public class PopupMenuStyle
    {
        public IDrawable background;
        public IDrawable border;
        public int borderSize = 1;

        public PopupMenuStyle()
        {
        }

        public PopupMenuStyle(IDrawable background, IDrawable border)
        {
            this.background = background;
            this.border = border;
        }

        public PopupMenuStyle(PopupMenuStyle style)
        {
            this.background = style.background;
            this.border = style.border;
        }
    }
}