using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class ListBox<T> : Element, ICullable where T : class
    {
        ListBoxStyle style;
        protected internal readonly List<T> items = new List<T>();
        protected internal readonly ListSelection<T> selection;
        private Rectangle cullingArea;
        private float prefWidth, prefHeight;
        float itemHeight;
        private int alignment = AlignInternal.left;
        int touchDown;

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

            AddListener(new Listener(this));
        }

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
            BitmapFont font = style.font;
            IDrawable selectedDrawable = style.selection;

            itemHeight = font.lineHeight;//font.capHeight - font.descent * 2;
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
            Validate();

            BitmapFont font = style.font;
            IDrawable selectedDrawable = style.selection;
            Color fontColorSelected = style.fontColorSelected;
            Color fontColorUnselected = style.fontColorUnselected;

            Color color = new Color(this.color, (int)(this.color.A * parentAlpha));
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
            float textOffsetY = selectedDrawable.TopHeight - font.descent;

            var unselectedFontColor = new Color(style.fontColorUnselected, (int)(style.fontColorUnselected.A * parentAlpha));
            var selectedFontColor = new Color(style.fontColorSelected, (int)(style.fontColorSelected.A * parentAlpha));
            //var hoveredFontColor = new Color(style.fontColorHovered, (int)(style.fontColorHovered.A * parentAlpha));
            Color fontColor = unselectedFontColor;
            //font.SetColor(fontColorUnselected.r, fontColorUnselected.g, fontColorUnselected.b, fontColorUnselected.a * parentAlpha);
            for (int i = 0; i < items.Count; i++)
            {
                if (cullingArea == default || (itemY + itemHeight <= cullingArea.Y + cullingArea.Height && itemY >= cullingArea.Y))
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
                else if (itemY < cullingArea.Y)
                {
                    break;
                }
                itemY += itemHeight;
            }
        }

        protected /*GlyphLayout*/void DrawItem(Graphics graphics, BitmapFont font, T item, float x, float y, float width, Color color)
        {
            var str = ToString(item);
            /*return*/
            font.Draw(graphics, str, x, y, (int)width, color);//(graphics, str, x, y, 0, str.Length, width, alignment, false, "...");
            //graphics.DrawRectangleBorder(x, y, width, font.lineHeight, Table.debugCellColor);
        }

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

        public void SetCullingArea(Rectangle cullingArea)
        {
            this.cullingArea = cullingArea;
        }

        /** Sets the horizontal alignment of the list items.
	    * @param alignment See {@link Align}. */
        public void SetAlignment(int alignment)
        {
            this.alignment = alignment;
        }

        #region Listener

        private class Listener : InputListener
        {
            private readonly ListBox<T> lb;

            public Listener(ListBox<T> lb)
            {
                this.lb = lb;
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                if (keycode == (int)Keys.A && InputUtils.IsCtrlDown() && lb.selection.GetMultiple())
                {
                    lb.selection.Clear();
                    lb.selection.AddAll(lb.items);
                    return true;
                }
                return false;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != 0 || button != 0) return false;
                if (lb.selection.IsDisabled()) return false;
                lb.GetStage().SetKeyboardFocus(lb);
                if (lb.items.Count == 0) return false;
                float height = lb.GetHeight();
                IDrawable background = lb.style.background;
                if (background != null)
                {
                    height -= background.TopHeight + background.BottomHeight;
                    y -= background.BottomHeight;
                }
                int index = (int)(y / lb.itemHeight);
                if (index > lb.items.Count - 1) return false;
                index = Math.Max(0, index);
                lb.selection.Choose(lb.items[index]);
                lb.touchDown = index;
                return true;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != 0 || button != 0) return;
                lb.touchDown = -1;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (pointer != 0) return;
                lb.touchDown = -1;
            }
        }

        #endregion

    }

    public class ListBoxStyle
    {
        public BitmapFont font;
        public Color fontColorSelected = Color.White;
        public Color fontColorUnselected = Color.White;
        public IDrawable selection;
        /** Optional. */
        public IDrawable down, background;

        public ListBoxStyle()
        {
        }

        public ListBoxStyle(BitmapFont font, Color fontColorSelected, Color fontColorUnselected, IDrawable selection)
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
