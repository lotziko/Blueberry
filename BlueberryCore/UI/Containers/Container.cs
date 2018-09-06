using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class Container<T> : Group where T : Element
    {
        private T element;
        private Value minWidth = Value.minWidth, minHeight = Value.minHeight;
        private Value prefWidth = Value.prefWidth, prefHeight = Value.prefHeight;
        private Value maxWidth = Value.zero, maxHeight = Value.zero;
        private Value padTop = Value.zero, padLeft = Value.zero, padBottom = Value.zero, padRight = Value.zero;
        private float fillX, fillY;
        private int align;
        private IDrawable background;
        private bool clip;
        private bool round = true;

        /** Creates a container with no element. */
        public Container()
        {
            SetTouchable(Touchable.ChildrenOnly);
            SetTransform(false);
        }

        public Container(T element) : this()
        {
            SetElement(element);
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();
            if (IsTransform())
            {
                ApplyTransform(graphics, ComputeTransform());
                DrawBackground(graphics, parentAlpha, 0, 0);
                if (clip)
                {
                    graphics.Flush();
                    float padLeft = this.padLeft.Get(this), padBottom = this.padBottom.Get(this);
                    if (ClipBegin(graphics, padLeft, padBottom, GetWidth() - padLeft - padRight.Get(this), GetHeight() - padBottom - padTop.Get(this)))
                    {
                        DrawElements(graphics, parentAlpha);
                        graphics.Flush();
                        ClipEnd(graphics);
                    }
                }
                else
                    DrawElements(graphics, parentAlpha);
                ResetTransform(graphics);
            }
            else
            {
                DrawBackground(graphics, parentAlpha, GetX(), GetY());
                base.Draw(graphics, parentAlpha);
            }
        }

        /** Called to draw the background, before clipping is applied (if enabled). Default implementation draws the background
	 * drawable. */
        protected void DrawBackground(Graphics graphics, float parentAlpha, float x, float y)
        {
            if (background == null) return;
            var color = GetColor();
            background.Draw(graphics, x, y, GetWidth(), GetHeight(), new Color(color, (int)(color.A * parentAlpha)));
        }

        /** Sets the background drawable and adjusts the container's padding to match the background.
	 * @see #setBackground(Drawable, bool) */
        public void SetBackground(IDrawable background)
        {
            SetBackground(background, true);
        }

        /** Sets the background drawable and, if adjustPadding is true, sets the container's padding to
	 * {@link Drawable#getBottomHeight()} , {@link Drawable#getTopHeight()}, {@link Drawable#getLeftWidth()}, and
	 * {@link Drawable#getRightWidth()}.
	 * @param background If null, the background will be cleared and padding removed. */
        public void SetBackground(IDrawable background, bool adjustPadding)
        {
            if (this.background == background) return;
            this.background = background;
            if (adjustPadding)
            {
                if (background == null)
                    Pad(Value.zero);
                else
                    Pad(background.TopHeight, background.LeftWidth, background.BottomHeight, background.RightWidth);
                Invalidate();
            }
        }

        /** @see #setBackground(Drawable) */
        public Container<T> Background(IDrawable background)
        {
            SetBackground(background);
            return this;
        }

        public IDrawable GetBackground()
        {
            return background;
        }

        #region ILayout

        public override void Layout()
        {
            if (element == null) return;

            float padLeft = this.padLeft.Get(this), padBottom = this.padBottom.Get(this);
            float containerWidth = GetWidth() - padLeft - padRight.Get(this);
            float containerHeight = GetHeight() - padBottom - padTop.Get(this);
            float minWidth = this.minWidth.Get(element), minHeight = this.minHeight.Get(element);
            float prefWidth = this.prefWidth.Get(element), prefHeight = this.prefHeight.Get(element);
            float maxWidth = this.maxWidth.Get(element), maxHeight = this.maxHeight.Get(element);

            float width;
            if (fillX > 0)
                width = containerWidth * fillX;
            else
                width = Math.Min(prefWidth, containerWidth);
            if (width < minWidth) width = minWidth;
            if (maxWidth > 0 && width > maxWidth) width = maxWidth;

            float height;
            if (fillY > 0)
                height = containerHeight * fillY;
            else
                height = Math.Min(prefHeight, containerHeight);
            if (height < minHeight) height = minHeight;
            if (maxHeight > 0 && height > maxHeight) height = maxHeight;

            float x = padLeft;
            if ((align & AlignInternal.right) != 0)
                x += containerWidth - width;
            else if ((align & AlignInternal.left) == 0) // center
                x += (containerWidth - width) / 2;

            float y = padBottom;
            if ((align & AlignInternal.bottom) != 0)
                y += containerHeight - height;
            else if ((align & AlignInternal.top) == 0) // center
                y += (containerHeight - height) / 2;

            if (round)
            {
                x = MathF.Round(x);
                y = MathF.Round(y);
                width = MathF.Round(width);
                height = MathF.Round(height);
            }

            element.SetBounds(x, y, width, height);
            if (element is ILayout) ((ILayout)element).Validate();
        }

        #endregion

        /** @param element May be null. */
        public void SetElement(T element)
        {
            if (element == this) throw new ArgumentException("element cannot be the Container.");
            if (element == this.element) return;
            if (this.element != null) base.RemoveElement(this.element);
            this.element = element;
            if (element != null) base.AddElement(element);
        }

        /** @return May be null. */
        public T GetElement()
        {
            return element;
        }

        /** @deprecated Container may have only a single child.
	    * @see #setActor(Actor) */
        public override E AddElement<E>(E element)
        {
            throw new NotSupportedException("Use Container#setActor.");
        }

        public override bool RemoveElement(Element element)
        {
            if (element == null) throw new ArgumentNullException("actor cannot be null.");
            if (element != this.element) return false;
            SetElement(null);
            return true;
        }

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value. */
        public Container<T> Size(Value size)
        {
            minWidth = size ?? throw new ArgumentNullException("size cannot be null.");
            minHeight = size;
            prefWidth = size;
            prefHeight = size;
            maxWidth = size;
            maxHeight = size;
            return this;
        }

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values. */
        public Container<T> Size(Value width, Value height)
        {
            minWidth = width ?? throw new ArgumentNullException("width cannot be null.");
            minHeight = height ?? throw new ArgumentNullException("height cannot be null.");
            prefWidth = width;
            prefHeight = height;
            maxWidth = width;
            maxHeight = height;
            return this;
        }

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value. */
        public Container<T> Size(float size)
        {
            Size(new Value.Fixed(size));
            return this;
        }

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values. */
        public Container<T> Size(float width, float height)
        {
            Size(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }

        /** Sets the minWidth, prefWidth, and maxWidth to the specified value. */
        public Container<T> Width(Value width)
        {
            minWidth = width ?? throw new ArgumentNullException("width cannot be null.");
            prefWidth = width;
            maxWidth = width;
            return this;
        }

        /** Sets the minWidth, prefWidth, and maxWidth to the specified value. */
        public Container<T> Width(float width)
        {
            Width(new Value.Fixed(width));
            return this;
        }

        /** Sets the minHeight, prefHeight, and maxHeight to the specified value. */
        public Container<T> Height(Value height)
        {
            minHeight = height ?? throw new ArgumentNullException("height cannot be null.");
            prefHeight = height;
            maxHeight = height;
            return this;
        }

        /** Sets the minHeight, prefHeight, and maxHeight to the specified value. */
        public Container<T> Height(float height)
        {
            Height(new Value.Fixed(height));
            return this;
        }

        /** Sets the minWidth and minHeight to the specified value. */
        public Container<T> MinSize(Value size)
        {
            minWidth = size ?? throw new ArgumentNullException("size cannot be null.");
            minHeight = size;
            return this;
        }

        /** Sets the minWidth and minHeight to the specified values. */
        public Container<T> MinSize(Value width, Value height)
        {
            minWidth = width ?? throw new ArgumentNullException("width cannot be null.");
            minHeight = height ?? throw new ArgumentNullException("height cannot be null.");
            return this;
        }

        public Container<T> SetMinWidth(Value minWidth)
        {
            this.minWidth = minWidth ?? throw new ArgumentNullException("minWidth cannot be null.");
            return this;
        }

        public Container<T> SetMinHeight(Value minHeight)
        {
            this.minHeight = minHeight ?? throw new ArgumentNullException("minHeight cannot be null.");
            return this;
        }

        /** Sets the minWidth and minHeight to the specified value. */
        public Container<T> MinSize(float size)
        {
            MinSize(new Value.Fixed(size));
            return this;
        }

        /** Sets the minWidth and minHeight to the specified values. */
        public Container<T> MinSize(float width, float height)
        {
            MinSize(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }

        public Container<T> SetMinWidth(float minWidth)
        {
            this.minWidth = new Value.Fixed(minWidth);
            return this;
        }

        public Container<T> SetMinHeight(float minHeight)
        {
            this.minHeight = new Value.Fixed(minHeight);
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified value. */
        public Container<T> PrefSize(Value size)
        {
            prefWidth = size ?? throw new ArgumentNullException("size cannot be null.");
            prefHeight = size;
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified values. */
        public Container<T> PrefSize(Value width, Value height)
        {
            prefWidth = width ?? throw new ArgumentNullException("width cannot be null.");
            prefHeight = height ?? throw new ArgumentNullException("height cannot be null.");
            return this;
        }

        public Container<T> PrefWidth(Value prefWidth)
        {
            this.prefWidth = prefWidth ?? throw new ArgumentNullException("prefWidth cannot be null.");
            return this;
        }

        public Container<T> PrefHeight(Value prefHeight)
        {
            this.prefHeight = prefHeight ?? throw new ArgumentNullException("prefHeight cannot be null.");
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified value. */
        public Container<T> PrefSize(float width, float height)
        {
            PrefSize(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified values. */
        public Container<T> PrefSize(float size)
        {
            PrefSize(new Value.Fixed(size));
            return this;
        }

        public Container<T> PrefWidth(float prefWidth)
        {
            this.prefWidth = new Value.Fixed(prefWidth);
            return this;
        }

        public Container<T> PrefHeight(float prefHeight)
        {
            this.prefHeight = new Value.Fixed(prefHeight);
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified value. */
        public Container<T> MaxSize(Value size)
        {
            maxWidth = size ?? throw new ArgumentNullException("size cannot be null.");
            maxHeight = size;
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified values. */
        public Container<T> MaxSize(Value width, Value height)
        {
            maxWidth = width ?? throw new ArgumentNullException("width cannot be null.");
            maxHeight = height ?? throw new ArgumentNullException("height cannot be null.");
            return this;
        }

        public Container<T> SetMaxWidth(Value maxWidth)
        {
            this.maxWidth = maxWidth ?? throw new ArgumentNullException("maxWidth cannot be null.");
            return this;
        }

        public Container<T> SetMaxHeight(Value maxHeight)
        {
            this.maxHeight = maxHeight ?? throw new ArgumentNullException("maxHeight cannot be null.");
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified value. */
        public Container<T> MaxSize(float size)
        {
            MaxSize(new Value.Fixed(size));
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified values. */
        public Container<T> MmaxSize(float width, float height)
        {
            MaxSize(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }

        public Container<T> SetMaxWidth(float maxWidth)
        {
            this.maxWidth = new Value.Fixed(maxWidth);
            return this;
        }

        public Container<T> SetMaxHeight(float maxHeight)
        {
            this.maxHeight = new Value.Fixed(maxHeight);
            return this;
        }

        /** Sets the padTop, padLeft, padBottom, and padRight to the specified value. */
        public Container<T> Pad(Value pad)
        {
            padTop = pad ?? throw new ArgumentNullException("pad cannot be null.");
            padLeft = pad;
            padBottom = pad;
            padRight = pad;
            return this;
        }

        public Container<T> Pad(Value top, Value left, Value bottom, Value right)
        {
            padTop = top ?? throw new ArgumentNullException("top cannot be null.");
            padLeft = left ?? throw new ArgumentNullException("left cannot be null.");
            padBottom = bottom ?? throw new ArgumentNullException("bottom cannot be null.");
            padRight = right ?? throw new ArgumentNullException("right cannot be null.");
            return this;
        }

        public Container<T> PadTop(Value padTop)
        {
            this.padTop = padTop ?? throw new ArgumentNullException("padTop cannot be null.");
            return this;
        }

        public Container<T> PadLeft(Value padLeft)
        {
            this.padLeft = padLeft ?? throw new ArgumentNullException("padLeft cannot be null.");
            return this;
        }

        public Container<T> PadBottom(Value padBottom)
        {
            this.padBottom = padBottom ?? throw new ArgumentNullException("padBottom cannot be null.");
            return this;
        }

        public Container<T> PadRight(Value padRight)
        {
            this.padRight = padRight ?? throw new ArgumentNullException("padRight cannot be null.");
            return this;
        }

        /** Sets the padTop, padLeft, padBottom, and padRight to the specified value. */
        public Container<T> Pad(float pad)
        {
            Value value = new Value.Fixed(pad);
            padTop = value;
            padLeft = value;
            padBottom = value;
            padRight = value;
            return this;
        }

        public Container<T> Pad(float top, float left, float bottom, float right)
        {
            padTop = new Value.Fixed(top);
            padLeft = new Value.Fixed(left);
            padBottom = new Value.Fixed(bottom);
            padRight = new Value.Fixed(right);
            return this;
        }

        public Container<T> PadTop(float padTop)
        {
            this.padTop = new Value.Fixed(padTop);
            return this;
        }

        public Container<T> PadLeft(float padLeft)
        {
            this.padLeft = new Value.Fixed(padLeft);
            return this;
        }

        public Container<T> PadBottom(float padBottom)
        {
            this.padBottom = new Value.Fixed(padBottom);
            return this;
        }

        public Container<T> PadRight(float padRight)
        {
            this.padRight = new Value.Fixed(padRight);
            return this;
        }

        /** Sets fillX and fillY to 1. */
        public Container<T> Fill()
        {
            fillX = 1f;
            fillY = 1f;
            return this;
        }

        /** Sets fillX to 1. */
        public Container<T> FillX()
        {
            fillX = 1f;
            return this;
        }

        /** Sets fillY to 1. */
        public Container<T> FillY()
        {
            fillY = 1f;
            return this;
        }

        public Container<T> Fill(float x, float y)
        {
            fillX = x;
            fillY = y;
            return this;
        }

        /** Sets fillX and fillY to 1 if true, 0 if false. */
        public Container<T> Fill(bool x, bool y)
        {
            fillX = x ? 1f : 0;
            fillY = y ? 1f : 0;
            return this;
        }

        /** Sets fillX and fillY to 1 if true, 0 if false. */
        public Container<T> Fill(bool fill)
        {
            fillX = fill ? 1f : 0;
            fillY = fill ? 1f : 0;
            return this;
        }

        /** Sets the alignment of the actor within the container. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom},
         * {@link Align#left}, {@link Align#right}, or any combination of those. */
        public Container<T> Align(int align)
        {
            this.align = align;
            return this;
        }

        /** Sets the alignment of the actor within the container to {@link Align#center}. This clears any other alignment. */
        public Container<T> Center()
        {
            align = AlignInternal.center;
            return this;
        }

        /** Sets {@link Align#top} and clears {@link Align#bottom} for the alignment of the actor within the container. */
        public Container<T> Top()
        {
            align |= AlignInternal.top;
            align &= ~AlignInternal.bottom;
            return this;
        }

        /** Sets {@link Align#left} and clears {@link Align#right} for the alignment of the actor within the container. */
        public Container<T> Left()
        {
            align |= AlignInternal.left;
            align &= ~AlignInternal.right;
            return this;
        }

        /** Sets {@link Align#bottom} and clears {@link Align#top} for the alignment of the actor within the container. */
        public Container<T> Bottom()
        {
            align |= AlignInternal.bottom;
            align &= ~AlignInternal.top;
            return this;
        }

        /** Sets {@link Align#right} and clears {@link Align#left} for the alignment of the actor within the container. */
        public Container<T> Right()
        {
            align |= AlignInternal.right;
            align &= ~AlignInternal.left;
            return this;
        }

        #region ILayout

        public override float MinWidth
        {
            get
            {
                return minWidth.Get(element) + padLeft.Get(this) + padRight.Get(this);
            }
        }

        public override float MinHeight
        {
            get
            {
                return minHeight.Get(element) + padTop.Get(this) + padBottom.Get(this);
            }
        }

        public override float PreferredWidth
        {
            get
            {
                float v = prefWidth.Get(element);
                if (background != null) v = Math.Max(v, background.MinWidth);
                return Math.Max(MinWidth, v + padLeft.Get(this) + padRight.Get(this));
            }
        }

        public override float PreferredHeight
        {
            get
            {
                float v = prefHeight.Get(element);
                if (background != null) v = Math.Max(v, background.MinHeight);
                return Math.Max(MinHeight, v + padTop.Get(this) + padBottom.Get(this));
            }
        }

        public override float MaxWidth
        {
            get
            {
                float v = maxWidth.Get(element);
                if (v > 0) v += padLeft.Get(this) + padRight.Get(this);
                return v;
            }
        }

        public override float MaxHeight
        {
            get
            {
                float v = maxHeight.Get(element);
                if (v > 0) v += padTop.Get(this) + padBottom.Get(this);
                return v;
            }
        }

        #endregion

        public Value GetMinWidthValue()
        {
            return minWidth;
        }

        public Value GetMinHeightValue()
        {
            return minHeight;
        }

        public Value GetPrefWidthValue()
        {
            return prefWidth;
        }

        public Value GetPrefHeightValue()
        {
            return prefHeight;
        }

        public Value GetMaxWidthValue()
        {
            return maxWidth;
        }

        public Value GetMaxHeightValue()
        {
            return maxHeight;
        }








        /** @return May be null if this value is not set. */
        public Value GetPadTopValue()
        {
            return padTop;
        }

        public float GetPadTop()
        {
            return padTop.Get(this);
        }

        /** @return May be null if this value is not set. */
        public Value GetPadLeftValue()
        {
            return padLeft;
        }

        public float GetPadLeft()
        {
            return padLeft.Get(this);
        }

        /** @return May be null if this value is not set. */
        public Value GetPadBottomValue()
        {
            return padBottom;
        }

        public float GetPadBottom()
        {
            return padBottom.Get(this);
        }

        /** @return May be null if this value is not set. */
        public Value GetPadRightValue()
        {
            return padRight;
        }

        public float GetPadRight()
        {
            return padRight.Get(this);
        }

        /** Returns {@link #getPadLeft()} plus {@link #getPadRight()}. */
        public float GetPadX()
        {
            return padLeft.Get(this) + padRight.Get(this);
        }

        /** Returns {@link #getPadTop()} plus {@link #getPadBottom()}. */
        public float GetPadY()
        {
            return padTop.Get(this) + padBottom.Get(this);
        }

        public float GetFillX()
        {
            return fillX;
        }

        public float GetFillY()
        {
            return fillY;
        }

        public int GetAlign()
        {
            return align;
        }

        /** If true (the default), positions and sizes are rounded to integers. */
        public void SetRound(bool round)
        {
            this.round = round;
        }

        /** Causes the contents to be clipped if they exceed the container bounds. Enabling clipping will set
         * {@link #setTransform(boolean)} to true. */
        public void SetClip(bool enabled)
        {
            clip = enabled;
            SetTransform(enabled);
            Invalidate();
        }

        public bool GetClip()
        {
            return clip;
        }

        public override Element Hit(Vector2 point, bool touchable = true)
        {
            if (clip)
            {
                if (touchable && GetTouchable() == Touchable.Disabled) return null;
                if (x < 0 || x >= GetWidth() || y < 0 || y >= GetHeight()) return null;
            }
            return base.Hit(point, touchable);
        }

        public override void DrawDebug(Graphics graphics)
        {
            Validate();
            if (IsTransform())
            {
                ApplyTransform(graphics, ComputeTransform());
                if (clip)
                {
                    graphics.Flush();
                    float padLeft = this.padLeft.Get(this), padBottom = this.padBottom.Get(this);
                    bool draw = background == null ? ClipBegin(graphics, 0, 0, GetWidth(), GetHeight())
                        : ClipBegin(graphics, padLeft, padBottom, GetWidth() - padLeft - padRight.Get(this),
                            GetHeight() - padBottom - padTop.Get(this));
                    if (draw)
                    {
                        DrawDebugElements(graphics, 1f);
                        ClipEnd(graphics);
                    }
                }
                else
                    DrawDebugElements(graphics, 1f);
                ResetTransform(graphics);
            }
            else
                base.DrawDebug(graphics);
        }
    }
}
