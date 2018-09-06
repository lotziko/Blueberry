using BlueberryCore.UI.Actions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueberryCore.UI
{
    public class SelectBox<T> : Element, IDisablable where T : class
    {

        SelectBoxStyle style;
        protected internal readonly List<T> items = new List<T>();
        protected internal readonly ListSelection<T> selection;
        SelectBoxList<T> selectBoxList;
        private float prefWidth, prefHeight;
        private readonly ClickListener clickListener;
        bool disabled;
        private int alignment = AlignInternal.left;

        public SelectBox(Skin skin, string stylename = "default") : this(skin.Get<SelectBoxStyle>(stylename))
        {

        }

        public SelectBox(SelectBoxStyle style)
        {
            selection = new ListSelection<T>(items);

            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);

            selection.SetElement(this);
            selection.SetRequired(true);

            selectBoxList = new SelectBoxList<T>(this);
            
            AddListener(clickListener = new Listener(this));
        }


        #region Listener

        private class Listener : ClickListener
        {
            private readonly SelectBox<T> box;

            public Listener(SelectBox<T> box)
            {
                this.box = box;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                FocusManager.ResetFocus(box.GetStage());
                if (pointer == 0 && button != 0) return false;
                if (box.disabled) return false;
                if (box.selectBoxList.HasParent())
                    box.HideList();
                else
                    box.ShowList();
                return true;
            }
        }

        #endregion


        /** Set the max number of items to display when the select box is opened. Set to 0 (the default) to display as many as fit in
	    * the stage height. */
        public void SetMaxListCount(int maxListCount)
        {
            selectBoxList.maxListCount = maxListCount;
        }

        /** @return Max number of items to display when the box is opened, or <= 0 to display them all. */
        public int GetMaxListCount()
        {
            return selectBoxList.maxListCount;
        }

        public override void SetStage(Stage stage)
        {
            if (stage == null) selectBoxList.Hide();
            base.SetStage(stage);
        }

        public void SetStyle(SelectBoxStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            if (selectBoxList != null)
            {
                selectBoxList.SetStyle(style.scrollStyle);
                selectBoxList.list.SetStyle(style.listStyle);
            }
            InvalidateHierarchy();
        }

        /** Returns the select box's style. Modifying the returned style may not have an effect until {@link #setStyle(SelectBoxStyle)}
	     * is called. */
        public SelectBoxStyle GetStyle()
        {
            return style;
        }

        /** Set the backing Array that makes up the choices available in the SelectBox */
        public void SetItems(params T[] newItems)
        {
            if (newItems == null) throw new ArgumentNullException("newItems cannot be null.");
            float oldPrefWidth = PreferredWidth;

            items.Clear();
            items.AddRange(newItems);
            selection.Validate();
            selectBoxList.list.SetItems(items);

            Invalidate();
            if (oldPrefWidth != PreferredWidth) InvalidateHierarchy();
        }

        /** Sets the items visible in the select box. */
        public void SetItems(List<T> newItems)
        {
            if (newItems == null) throw new ArgumentNullException("newItems cannot be null.");
            float oldPrefWidth = PreferredWidth;

            if (newItems != items)
            {
                items.Clear();
                items.AddRange(newItems);
            }
            selection.Validate();
            selectBoxList.list.SetItems(items);

            Invalidate();
            if (oldPrefWidth != PreferredWidth) InvalidateHierarchy();
        }

        public void ClearItems()
        {
            if (items.Count == 0) return;
            items.Clear();
            selection.Clear();
            InvalidateHierarchy();
        }

        /** Returns the internal items array. If modified, {@link #setItems(Array)} must be called to reflect the changes. */
        public List<T> GetItems()
        {
            return items;
        }

        #region ILayout

        public override void Layout()
        {
            IDrawable bg = style.background;
            BitmapFont font = style.font;

            if (bg != null)
            {
                prefHeight = Math.Max(bg.TopHeight + bg.BottomHeight , Math.Max(font.capHeight - font.descent * 2, bg.MinHeight));
            }
            else
                prefHeight = font.capHeight - font.descent * 2;

            float maxItemWidth = 0;
            for (int i = 0; i < items.Count; i++)
            {
                //layout.setText(font, toString(items.get(i)));
                maxItemWidth = Math.Max(font.ComputeWidth(ToString(items[i])), maxItemWidth);
            }

            prefWidth = maxItemWidth;
            if (bg != null) prefWidth += bg.LeftWidth + bg.RightWidth;

            var listStyle = style.listStyle;
            ScrollPaneStyle scrollStyle = style.scrollStyle;
            float listWidth = maxItemWidth + listStyle.selection.LeftWidth + listStyle.selection.RightWidth;
        }

        public override float PreferredWidth
        {
            get
            {
                Validate();
                return prefWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                Validate();
                return prefHeight;
            }
        }

        #endregion

        #region IDisablable

        public void SetDisabled(bool disabled)
        {
            if (disabled && !this.disabled) HideList();
            this.disabled = disabled;
        }

        public bool IsDisabled()
        {
            return disabled;
        }

        #endregion

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();
            
            IDrawable background;
            if (disabled && style.backgroundDisabled != null)
                background = style.backgroundDisabled;
            else if (selectBoxList.HasParent() && style.backgroundOpen != null)
                background = style.backgroundOpen;
            else if (clickListener.IsOver() && style.backgroundOver != null)
                background = style.backgroundOver;
            else if (style.background != null)
                background = style.background;
            else
                background = null;
            BitmapFont font = style.font;
            Color fontColor = (disabled && style.disabledFontColor != Color.TransparentBlack) ? style.disabledFontColor : style.fontColor;

            //Color color = GetColor();
            float x = GetX(), y = GetY();
            float width = GetWidth(), height = GetHeight();

            var color = new Color(this.color, (int)(this.color.A * parentAlpha));
            if (background != null) background.Draw(graphics, x, y, width, height, color);

            T selected = selection.First();
            if (selected != null)
            {
                if (background != null)
                {
                    width -= background.LeftWidth + background.RightWidth;
                    height -= background.BottomHeight + background.TopHeight;
                    x += background.LeftWidth;
                    y += (int)(height / 2 - font.MeasureString(ToString(selected)).Y / 2);//height / 2 + background.BottomHeight + font.capHeight / 2);
                }
                else
                {
                    y += (int)(height / 2 + font.capHeight / 2);
                }
                DrawItem(graphics, font, selected, x, y, width, new Color(fontColor, (int)(fontColor.A * parentAlpha)));
            }
        }

        protected /*GlyphLayout*/void DrawItem(Graphics graphics, BitmapFont font, T item, float x, float y, float width, Color color)
        {
            var str = ToString(item);
            font.Draw(graphics, str, x, y, (int)width, color);
        }

        /** Sets the alignment of the selected item in the select box.See {@link #getList()} and {@link List#setAlignment(int)} to set
	     * the alignment in the list shown when the select box is open.

         * @param alignment See { @link Align }. */
        public void SetAlignment(int alignment)
        {
            this.alignment = alignment;
        }

        /** Get the set of selected items, useful when multiple items are selected
         * @return a Selection object containing the selected elements */
        public ListSelection<T> GetSelection()
        {
            return selection;
        }

        /** Returns the first selected item, or null. For multiple selections use {@link SelectBox#getSelection()}. */
        public T GetSelected()
        {
            return selection.First();
        }

        /** Sets the selection to only the passed item, if it is a possible choice, else selects the first item. */
        public void SetSelected(T item)
        {
            if (items.Contains(item))
                selection.Set(item);
            else if (items.Count > 0)
                selection.Set(items.First());
            else
                selection.Clear();
        }

        /** @return The index of the first selected item. The top item has an index of 0. Nothing selected has an index of -1. */
        public int GetSelectedIndex()
        {
            var selected = selection.Items();
            return selected.Count == 0 ? -1 : items.IndexOf(selected.First());
        }

        /** Sets the selection to only the selected index. */
        public void SetSelectedIndex(int index)
        {
            selection.Set(items[index]);
        }

        protected internal string ToString(T item)
        {
            return item.ToString();
        }

        public void ShowList()
        {
            if (items.Count == 0) return;
            selectBoxList.Show(GetStage());
        }

        public void HideList()
        {
            selectBoxList.Hide();
        }

        /** Returns the list shown when the select box is open. */
        public ListBox<T> GetList()
        {
            return selectBoxList.list;
        }

        /** Disables scrolling of the list shown when the select box is open. */
        public void SetScrollingDisabled(bool y)
        {
            selectBoxList.SetScrollingDisabled(true, y);
            InvalidateHierarchy();
        }

        /** Returns the scroll pane containing the list that is shown when the select box is open. */
        public ScrollPane GetScrollPane()
        {
            return selectBoxList;
        }

        protected internal void OnShow(Element selectBoxList, bool below)
        {
            selectBoxList.color.A = 0;
            selectBoxList.AddAction(SAction.FadeIn(0.3f, Interpolation.fade));
        }

        protected internal void OnHide(Element selectBoxList)
        {
            selectBoxList.color.A = 255;
            selectBoxList.AddAction(SAction.Sequence(SAction.FadeOut(0.4f, Interpolation.fade), SAction.RemoveElement()));
        }

    }

    class SelectBoxList<T> : ScrollPane where T : class
    {
        private readonly SelectBox<T> selectBox;
        internal int maxListCount;
        private Vector2 screenPosition = new Vector2();
        internal readonly ListBox<T> list;
        private readonly InputListener hideListener;
        private Element previousScrollFocus;

        static Vector2 temp = new Vector2();

        public SelectBoxList(SelectBox<T> selectBox) : base(null, selectBox.GetStyle().scrollStyle)
        {
            this.selectBox = selectBox;

            SetOverscroll(false, false);
            SetFadeScrollBars(false);
            SetScrollingDisabled(true, false);

            list = new SBLList<T>(selectBox.GetStyle().listStyle, this);
            list.SetTouchable(Touchable.Disabled);
            SetElement(list);

            list.AddListener(new SBLClickListener(this));

            AddListener(new SBLInputListener(this));
            AddListener(hideListener = new SBLInputHideListener(this));
        }

        #region Listeners and overriden classes

        private class SBLList<Type> : ListBox<T> where Type : T
        {
            private readonly SelectBoxList<T> box;

            public SBLList(ListBoxStyle style, SelectBoxList<T> box) : base(style)
            {
                this.box = box;
            }

            protected override string ToString(T obj)
            {
                return box.selectBox.ToString(obj);
            }
        }

        private class SBLClickListener : ClickListener
        {
            private readonly SelectBoxList<T> box;

            public SBLClickListener(SelectBoxList<T> box)
            {
                this.box = box;
            }

            public override void Clicked(InputEvent ev, float x, float y)
            {
                box.selectBox.selection.Choose(box.list.GetSelected());
                box.Hide();
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                box.list.SetSelectedIndex(Math.Min(box.selectBox.items.Count - 1, (int)(y / box.list.GetItemHeight())));
                return true;
            }
        }

        private class SBLInputListener : InputListener
        {
            private readonly SelectBoxList<T> box;

            public SBLInputListener(SelectBoxList<T> box)
            {
                this.box = box;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (toElement == null || !box.IsAscendantOf(toElement)) box.list.selection.Set(box.selectBox.GetSelected());
            }
        }

        private class SBLInputHideListener : InputListener
        {
            private readonly SelectBoxList<T> box;

            public SBLInputHideListener(SelectBoxList<T> box)
            {
                this.box = box;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                var target = ev.GetTarget();
                if (box.IsAscendantOf(target)) return false;
                box.list.selection.Set(box.selectBox.GetSelected());
                box.Hide();
                return false;
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                if (keycode == (int)Keys.Escape) box.Hide();
                return false;
            }
        }


        #endregion

        public void Show(Stage stage)
        {
            if (list.IsTouchable()) return;

            stage.RemoveCaptureListener(hideListener);
            stage.AddCaptureListener(hideListener);
            stage.AddElement(this);

            screenPosition.Set(0, 0);
            screenPosition = selectBox.LocalToStageCoordinates(screenPosition);

            float itemHeight = list.GetItemHeight();
            float height = itemHeight * (maxListCount <= 0 ? selectBox.items.Count : Math.Min(maxListCount, selectBox.items.Count));
            IDrawable scrollPaneBackground = GetStyle().background;
            if (scrollPaneBackground != null) height += scrollPaneBackground.TopHeight + scrollPaneBackground.BottomHeight;
            IDrawable listBackground = list.GetStyle().background;
            if (listBackground != null) height += listBackground.TopHeight + listBackground.BottomHeight;

            float heightBelow = screenPosition.Y;
            float heightAbove = stage.GetCamera().Height - screenPosition.Y - selectBox.GetHeight();
            bool below = true;
            if (height > heightBelow)
            {
                if (heightAbove > heightBelow)
                {
                    below = false;
                    height = Math.Min(height, heightAbove);
                }
                else
                    height = heightBelow;
            }

            if (below)
                SetY(screenPosition.Y + selectBox.GetHeight());
            else
                SetY(screenPosition.Y - height);
            SetX(screenPosition.X);
            SetHeight(height);
            Validate();
            float width = Math.Max(PreferredWidth, selectBox.GetWidth());
            if (PreferredHeight > height && !disableY) width += GetScrollBarWidth();
            SetWidth(width);

            Validate();
            ScrollTo(0, list.GetHeight() - selectBox.GetSelectedIndex() * itemHeight - itemHeight / 2, 0, 0, true, true);
            UpdateVisualScroll();

            previousScrollFocus = null;
            var element = stage.GetScrollFocus();
            if (element != null && !element.IsDescendantOf(this)) previousScrollFocus = element;
            stage.SetScrollFocus(this);

            list.selection.Set(selectBox.GetSelected());
            list.SetTouchable(Touchable.Enabled);
            ClearActions();
            selectBox.OnShow(this, below);
        }

        public void Hide()
        {
            if (!list.IsTouchable() || !HasParent()) return;
            list.SetTouchable(Touchable.Disabled);

            Stage stage = GetStage();
            if (stage != null)
            {
                stage.RemoveCaptureListener(hideListener);
                if (previousScrollFocus != null && previousScrollFocus.GetStage() == null) previousScrollFocus = null;
                var element = stage.GetScrollFocus();
                if (element == null || IsAscendantOf(element)) stage.SetScrollFocus(previousScrollFocus);
            }

            ClearActions();
            selectBox.OnHide(this);
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            temp.Set(0, 0);
            temp = selectBox.LocalToStageCoordinates(temp);
            if (!temp.Equals(screenPosition)) Hide();
            base.Draw(graphics, parentAlpha);
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            ToFront();
        }
    }

    public class SelectBoxStyle
    {
        public BitmapFont font;
        public Color fontColor = Color.White;
        /** Optional. */
        public Color disabledFontColor;
        /** Optional. */
        public IDrawable background;
        public ScrollPaneStyle scrollStyle;
        public ListBoxStyle listStyle;
        /** Optional. */
        public IDrawable backgroundOver, backgroundOpen, backgroundDisabled;

        public SelectBoxStyle()
        {
        }

        public SelectBoxStyle(BitmapFont font, Color fontColor, IDrawable background, ScrollPaneStyle scrollStyle,
            ListBoxStyle listStyle)
        {
            this.font = font;
            this.fontColor = fontColor;
            this.background = background;
            this.scrollStyle = scrollStyle;
            this.listStyle = listStyle;
        }

        public SelectBoxStyle(SelectBoxStyle style)
        {
            this.font = style.font;
            this.fontColor = style.fontColor;
            if (style.disabledFontColor != Color.TransparentBlack) this.disabledFontColor = new Color(style.disabledFontColor, style.disabledFontColor.A);
            this.background = style.background;
            this.backgroundOver = style.backgroundOver;
            this.backgroundOpen = style.backgroundOpen;
            this.backgroundDisabled = style.backgroundDisabled;
            this.scrollStyle = new ScrollPaneStyle(style.scrollStyle);
            this.listStyle = new ListBoxStyle(style.listStyle);
        }
    }
}
