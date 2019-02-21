using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.UI
{
    /// <summary>
    /// Please use wrappers instead of string because it causes bug with Equal()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListBox<T> : Element, ICullable where T : class
    {
        ListBoxStyle style;
        protected internal readonly List<T> items = new List<T>();
        protected internal readonly ListSelection<T> selection;
        private Rect cullingArea;
        private float prefWidth, prefHeight;
        float itemHeight;
        private int alignment = AlignInternal.left;
        int touchDown = -1, overIndex = -1;
        private InputListener<ListBox<T>> keyListener;
        bool typeToSelect;

        public ListBox(Skin skin, string stylename = "default") : this(skin.Get<ListBoxStyle>(stylename))
        {

        }

        public ListBox(ListBoxStyle style)
        {
            selection = new ListSelection<T>(items);
            selection.SetElement(this);
            selection.SetRequired(true);

            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);

            AddListener(keyListener = new KeyListener(this));
            AddListener(new Listen(this));
        }

        #region Listeners

        private class KeyListener : InputListener<ListBox<T>>
        {
            long typeTimeout;
            string prefix;

            public KeyListener(ListBox<T> par) : base(par)
            {
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                if (par.items.Count == 0) return false;
                int index;
                switch ((Key)keycode)
                {
                    case Key.A:
                        if (Input.IsCtrlDown() && par.selection.GetMultiple())
                        {
                            par.selection.Clear();
                            par.selection.AddAll(par.items);
                            return true;
                        }
                        break;
                    case Key.Home:
                        par.SetSelectedIndex(0);
                        return true;
                    case Key.End:
                        par.SetSelectedIndex(par.items.Count - 1);
                        return true;
                    case Key.Down:
                        index = par.items.IndexOf(par.GetSelected()) + 1;
                        if (index >= par.items.Count) index = 0;
                        par.SetSelectedIndex(index);
                        return true;
                    case Key.Up:
                        index = par.items.IndexOf(par.GetSelected()) - 1;
                        if (index < 0) index = par.items.Count - 1;
                        par.SetSelectedIndex(index);
                        return true;
                    case Key.Escape:
                        par.GetStage().SetKeyboardFocus(null);
                        return true;
                }
                return false;
            }

            public override bool KeyTyped(InputEvent ev, int keycode, char character)
            {
                if (!par.typeToSelect) return false;
                long time = TimeUtils.CurrentTimeMillis();
                if (time > typeTimeout) prefix = "";
                typeTimeout = time + 300;
                prefix += char.ToLower(character);
                for (int i = 0, n = par.items.Count; i < n; i++)
                {
                    if (par.ToString(par.items[i]).ToLower().StartsWith(prefix))
                    {
                        par.SetSelectedIndex(i);
                        break;
                    }
                }
                return false;
            }
        }

        private class Listen : InputListener<ListBox<T>>
        {
            public Listen(ListBox<T> par) : base(par)
            {
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != 0 || button != 0) return true;
                if (par.selection.IsDisabled()) return true;
                par.GetStage().SetKeyboardFocus(par);
                if (par.items.Count == 0) return true;
                int index = par.GetItemIndexAt(y);
                if (index == -1) return true;
                par.selection.Choose(par.items[index]);
                par.touchDown = index;
                return true;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != 0 || button != 0) return;
                par.touchDown = -1;
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                par.overIndex = par.GetItemIndexAt(y);
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                par.overIndex = par.GetItemIndexAt(y);
                return false;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (pointer == 0) par.touchDown = -1;
                if (pointer == -1) par.overIndex = -1;
            }
        }

        #endregion

        public void SetStyle(ListBoxStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            InvalidateHierarchy();
        }

        /** Returns the list's style. Modifying the returned style may not have an effect until {@link #setStyle(ListStyle)} is
	    * called. */
        public ListBoxStyle GetStyle()
        {
            return style;
        }

        #region ILayout

        public override void Layout()
        {
            IFont font = style.font;
            IDrawable selectedDrawable = style.selection;

            itemHeight = font.LineHeight;//font.capHeight - font.descent * 2;
            itemHeight += selectedDrawable.TopHeight + selectedDrawable.BottomHeight;

            prefWidth = 0;

            for (int i = 0; i < items.Count; i++)
            {
                //layout.setText(font, toString(items.get(i)));
                prefWidth = Math.Max(font.MeasureString(ToString(items[i])).X, prefWidth);
            }

            prefWidth += selectedDrawable.LeftWidth + selectedDrawable.RightWidth;
            prefHeight = items.Count * itemHeight;

            IDrawable background = style.background;
            if (background != null)
            {
                prefWidth += background.LeftWidth + background.RightWidth;
                prefHeight += background.TopHeight + background.BottomHeight;
            }
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

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();//TODO: rewrite

            IFont font = style.font;
            IDrawable selectedDrawable = style.selection;
            Col fontColorSelected = style.fontColorSelected;
            Col fontColorUnselected = style.fontColorUnselected;

            Col color = new Col(this.color, this.color.A * parentAlpha);
            //batch.setColor(color.r, color.g, color.b, color.a * parentAlpha);

            float x = GetX(), y = GetY(), width = GetWidth(), height = GetHeight();
            float itemY = 0;

            IDrawable background = style.background;
            if (background != null)
            {
                background.Draw(graphics, x, y, width, height, color);
                float leftWidth = background.LeftWidth;
                x += leftWidth;
                itemY += background.TopHeight;
                width -= leftWidth + background.RightWidth;
            }

            float textOffsetX = selectedDrawable.LeftWidth, textWidth = width - textOffsetX - selectedDrawable.RightWidth;
            float textOffsetY = selectedDrawable.TopHeight - font.Descent;

            var unselectedFontColor = new Col(style.fontColorUnselected, style.fontColorUnselected.A * parentAlpha);
            var selectedFontColor = new Col(style.fontColorSelected, style.fontColorSelected.A * parentAlpha);
            //var hoveredFontColor = new Color(style.fontColorHovered, (int)(style.fontColorHovered.A * parentAlpha));
            Col fontColor = unselectedFontColor;
            //font.SetColor(fontColorUnselected.r, fontColorUnselected.g, fontColorUnselected.b, fontColorUnselected.a * parentAlpha);
            for (int i = 0; i < items.Count; i++)
            {
                if (cullingArea == default || (itemY <= cullingArea.Y + cullingArea.Height && itemY >= cullingArea.Y - itemHeight))
                {
                    T item = items[i];
                    bool selected = selection.Contains(item);
                    if (selected)
                    {
                        IDrawable drawable = selectedDrawable;
                        if (touchDown == i && style.down != null) drawable = style.down;
                        drawable.Draw(graphics, x, y + itemY, width, itemHeight, color);
                        fontColor = selectedFontColor;//font.setColor(fontColorSelected.r, fontColorSelected.g, fontColorSelected.b, fontColorSelected.a * parentAlpha);
                    }
                    DrawItem(graphics, font, item, x + textOffsetX, y + itemY/* - textOffsetY*/, textWidth, fontColor);//DrawItem(graphics, font, i, item, x + textOffsetX, y + itemY - textOffsetY, textWidth);
                    if (selected)
                    {
                        fontColor = unselectedFontColor;
                        //font.setColor(fontColorUnselected.r, fontColorUnselected.g, fontColorUnselected.b,
                        //  fontColorUnselected.a * parentAlpha);
                    }
                }
                else if (itemY > cullingArea.Y)
                {
                    break;
                }
                itemY += itemHeight;
            }
        }

        protected void DrawItem(Graphics graphics, IFont font, T item, float x, float y, float width, Col color)
        {
            var str = ToString(item);
            /*return*/
            font.Draw(graphics, str, x, y, (int)width, color);//(graphics, str, x, y, 0, str.Length, width, alignment, false, "...");
            //graphics.DrawRectangleBorder(x, y, width, font.lineHeight, Table.debugCellColor);
        }

        /*#region Listener

        private class Listener : InputListener<ListBox<T>>
        {
            public Listener(ListBox<T> par) : base(par)
            {
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                if (keycode == (int)Key.A && Input.IsCtrlDown() && par.selection.GetMultiple())
                {
                    par.selection.Clear();
                    par.selection.AddAll(par.items);
                    return true;
                }
                return false;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != 0 || button != 0) return false;
                if (par.selection.IsDisabled()) return false;
                par.GetStage().SetKeyboardFocus(par);
                if (par.items.Count == 0) return false;
                float height = par.GetHeight();
                IDrawable background = par.style.background;
                if (background != null)
                {
                    height -= background.TopHeight + background.BottomHeight;
                    y -= background.BottomHeight;
                }
                int index = (int)(y / par.itemHeight);
                if (index > par.items.Count - 1) return false;
                index = Math.Max(0, index);
                par.selection.Choose(par.items[index]);
                par.touchDown = index;
                Render.Request();
                return true;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != 0 || button != 0) return;
                par.touchDown = -1;
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                Render.Request();
                return true;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (pointer != 0) return;
                par.touchDown = -1;
            }
        }

        #endregion*/
        
        public ListSelection<T> GetSelection()
        {
            return selection;
        }

        /** Returns the first selected item, or null. */
        public T GetSelected()
        {
            return selection.First();
        }

        /** Sets the selection to only the passed item, if it is a possible choice.
	    * @param item May be null. */
        public void SetSelected(T item)
        {
            if (items.Contains(item))
                selection.Set(item);
            else if (selection.GetRequired() && items.Count > 0)
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
            if (index < -1 || index >= items.Count)
                throw new ArgumentOutOfRangeException("index must be >= -1 and < " + items.Count + ": " + index);
            if (index == -1)
            {
                selection.Clear();
            }
            else
            {
                selection.Set(items[index]);
            }
        }

        /** @return null if not over an item. */
        public T GetItemAt(float y)
        {
            int index = GetItemIndexAt(y);
            if (index == -1) return null;
            return items[index];
        }

        /** @return -1 if not over an item. */
        public int GetItemIndexAt(float y)
        {
            float height = GetHeight();
            IDrawable background = style.background;
            if (background != null)
            {
                height -= background.TopHeight + background.BottomHeight;
                y -= background.TopHeight;
            }
            var fIndex = y / itemHeight;
            int index;
            /*if (fIndex < 0)
                index = -1;
            else*/
                index = (int)Math.Floor(fIndex);
            if (index < 0 || index >= items.Count) return -1;
            return index;
        }

        public void SetItems(params T[] newItems)
        {
            if (newItems == null) throw new ArgumentNullException("newItems cannot be null.");
            float oldPrefWidth = PreferredWidth, oldPrefHeight = PreferredHeight;

            items.Clear();
            items.AddRange(newItems);
            selection.Validate();

            Invalidate();
            if (oldPrefWidth != PreferredWidth || oldPrefHeight != PreferredHeight) InvalidateHierarchy();
        }

        /** Sets the items visible in the list, clearing the selection if it is no longer valid. If a selection is
	     * {@link ArraySelection#getRequired()}, the first item is selected. This can safely be called with a
	     * (modified) array returned from {@link #getItems()}. */
        public void SetItems(List<T> newItems)
        {
            if (newItems == null) throw new ArgumentNullException("newItems cannot be null.");
            float oldPrefWidth = PreferredWidth, oldPrefHeight = PreferredHeight;

            if (newItems != items)
            {
                items.Clear();
                items.AddRange(newItems);
            }
            selection.Validate();

            Invalidate();
            if (oldPrefWidth != PreferredWidth || oldPrefHeight != PreferredHeight) InvalidateHierarchy();
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

        public float GetItemHeight()
        {
            return itemHeight;
        }

        protected virtual string ToString(T obj)
        {
            return obj.ToString();
        }

        public void SetCullingArea(Rect cullingArea)
        {
            this.cullingArea = cullingArea;
        }

        /** Sets the horizontal alignment of the list items.
	    * @param alignment See {@link Align}. */
        public void SetAlignment(int alignment)
        {
            this.alignment = alignment;
        }
    }

    public class ListBoxStyle
    {
        public IFont font;
        public Col fontColorSelected = Col.White;
        public Col fontColorUnselected = Col.White;
        public IDrawable selection;
        /** Optional. */
        public IDrawable down, background;

        public ListBoxStyle()
        {
        }

        public ListBoxStyle(IFont font, Col fontColorSelected, Col fontColorUnselected, IDrawable selection)
        {
            this.font = font;
            this.fontColorSelected = fontColorSelected;
            this.fontColorUnselected = fontColorUnselected;
            this.selection = selection;
        }

        public ListBoxStyle(ListBoxStyle style)
        {
            this.font = style.font;
            this.fontColorSelected = style.fontColorSelected;
            this.fontColorUnselected = style.fontColorUnselected;
            this.selection = style.selection;
            this.down = style.down;
        }
    }
}
