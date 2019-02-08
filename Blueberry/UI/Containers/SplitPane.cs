using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public class SplitPane : Group
    {
        SplitPaneStyle style;
        private Element firstWidget, secondWidget;
        private bool vertical, clampSplitAmount;
        private float splitAmount = 0.5f, minAmount, maxAmount = 1;
        private IDrawable handle;

        private Rect firstWidgetBounds = new Rect();
        private Rect secondWidgetBounds = new Rect();
        private Rect tempScissors = new Rect();
        private Rect handleBounds = new Rect();
        bool cursorOverHandle, dragging;

        Vec2 lastPoint = new Vec2();
        Vec2 handlePosition = new Vec2();

        public SplitPane(Element firstWidget, Element secondWidget, bool vertical, Skin skin, string stylename = "default") : this(firstWidget, secondWidget, vertical, skin.Get<SplitPaneStyle>(stylename)) { }

        public SplitPane(Element firstWidget, Element secondWidget, bool vertical, SplitPaneStyle style)
        {
            this.vertical = vertical;
            SetStyle(style);
            SetFirstWidget(firstWidget);
            SetSecondWidget(secondWidget);
            SetSize(PreferredWidth, PreferredHeight);
            listeners.Add(new Listener(this));
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            RefreshBackground();
        }

        protected void RefreshBackground()
        {
            IDrawable handle = null;

            if (cursorOverHandle && style.handleOver != null)
                handle = style.handleOver;
            else
                handle = style.handle;

            if (handle != this.handle)
            {
                this.handle = handle;
                Render.Request();
            }
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();

            var color = this.color;
            float alpha = (color.A / 255 * parentAlpha) * 255;

            ApplyTransform(graphics, ComputeTransform());
            if (firstWidget != null && firstWidget.IsVisible())
            {
                graphics.Flush();
                tempScissors = stage.CalculateScissors(firstWidgetBounds, this);
                if (ScissorStack.PushScissors(tempScissors))
                {
                    firstWidget.Draw(graphics, parentAlpha);
                    graphics.Flush();
                    ScissorStack.PopScissors();
                }
            }
            if (secondWidget != null && secondWidget.IsVisible())
            {
                graphics.Flush();
                tempScissors = stage.CalculateScissors(secondWidgetBounds, this);
                if (ScissorStack.PushScissors(tempScissors))
                {
                    secondWidget.Draw(graphics, parentAlpha);
                    graphics.Flush();
                    ScissorStack.PopScissors();
                }
            }
            handle.Draw(graphics, handleBounds.X, handleBounds.Y, handleBounds.Width, handleBounds.Height, new Col(color, alpha));
            ResetTransform(graphics);
        }

        #region Listener

        private class Listener : InputListener
        {
            private SplitPane p;

            public Listener(SplitPane pane)
            {
                p = pane;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (p.handleBounds.Contains(x, y))
                {
                    FocusManager.ResetFocus(p.GetStage());

                    p.lastPoint.Set(x, y);
                    p.handlePosition.Set(x, y);
                    p.dragging = true;
                    return true;
                }
                return false;
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                if (!p.dragging)
                    return;

                IDrawable handle = p.style.handle;
                if (!p.vertical)
                {
                    float delta = x - p.lastPoint.X;
                    float availWidth = p.GetWidth() - handle.MinWidth;
                    float dragX = p.handlePosition.X + delta;
                    p.handlePosition.X = dragX;
                    dragX = Math.Max(0, dragX);
                    dragX = Math.Min(availWidth, dragX);
                    p.splitAmount = dragX / availWidth;
                    p.lastPoint.Set(x, y);
                }
                else
                {
                    float delta = y - p.lastPoint.Y;
                    float availHeight = p.GetHeight() - handle.MinHeight;
                    float dragY = p.handlePosition.Y + delta;
                    p.handlePosition.Y = dragY;
                    dragY = Math.Max(0, dragY);
                    dragY = Math.Min(availHeight, dragY);
                    p.splitAmount = dragY / availHeight;
                    p.lastPoint.Set(x, y);
                }
                p.Invalidate();
                Render.Request();
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                p.dragging = false;
                p.cursorOverHandle = false;
                //Is used to reset cursor and handle state on mouse release if it's over handle
                MouseMoved(ev, x, y);
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (!p.dragging)
                    p.cursorOverHandle = false;
                ResetCursor();
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                p.cursorOverHandle = p.handleBounds.Contains(x, y);
                ResetCursor();
                return false;
            }

            private void ResetCursor()
            {
                /*if (p.cursorOverHandle)
                    Mouse.SetCursor(p.vertical ? MouseCursor.SizeNS : MouseCursor.SizeWE);
                else
                    Mouse.SetCursor(MouseCursor.Arrow);*/
            }
        }

        #endregion

        public void SetStyle(SplitPaneStyle style)
        {
            this.style = style;
            InvalidateHierarchy();
        }

        public SplitPaneStyle GetStyle() => style;

        public void SetClampSplitAmount(bool clamp)
        {
            clampSplitAmount = clamp;
        }

        public bool IsClampSplitAmount()
        {
            return clampSplitAmount;
        }

        public void SetVertical(bool vertical)
        {
            if (this.vertical == vertical)
                return;
            this.vertical = vertical;
            InvalidateHierarchy();
        }

        public bool IsVertical()
        {
            return vertical;
        }

        public void SetSplitAmount(float splitAmount)
        {
            this.splitAmount = splitAmount; // will be clamped during layout
            Invalidate();
        }

        public float GetSplitAmount()
        {
            return splitAmount;
        }

        protected void ClampSplitAmount()
        {
            float effectiveMinAmount = minAmount, effectiveMaxAmount = maxAmount;

            if (vertical)
            {
                float availableHeight = GetHeight() - style.handle.MinHeight;
                if (firstWidget is ILayout)
                    effectiveMinAmount = Math.Max(effectiveMinAmount, Math.Min((firstWidget as ILayout).MinHeight / availableHeight, 1));
                if (secondWidget is ILayout)
                    effectiveMaxAmount = Math.Min(effectiveMaxAmount, 1 - Math.Min((secondWidget as ILayout).MinHeight / availableHeight, 1));
            }
            else
            {
                float availableWidth = GetWidth() - style.handle.MinWidth;
                if (firstWidget is ILayout)
                    effectiveMinAmount = Math.Max(effectiveMinAmount, Math.Min((firstWidget as ILayout).MinWidth / availableWidth, 1));
                if (secondWidget is ILayout)
                    effectiveMaxAmount = Math.Min(effectiveMaxAmount, 1 - Math.Min((secondWidget as ILayout).MinWidth / availableWidth, 1));
            }

            if (effectiveMinAmount > effectiveMaxAmount) // Locked handle. Average the position.
                splitAmount = 0.5f * (effectiveMinAmount + effectiveMaxAmount);
            else
                splitAmount = Math.Max(Math.Min(splitAmount, effectiveMaxAmount), effectiveMinAmount);
        }

        public float GetMinSplitAmount()
        {
            return minAmount;
        }

        public void SetMinSplitAmount(float minAmount)
        {
            if (minAmount < 0 || minAmount > 1) throw new Exception("minAmount has to be >= 0 and <= 1");
            this.minAmount = minAmount;
        }

        public float GetMaxSplitAmount()
        {
            return maxAmount;
        }

        public void SetMaxSplitAmount(float maxAmount)
        {
            if (maxAmount < 0 || maxAmount > 1) throw new Exception("maxAmount has to be >= 0 and <= 1");
            this.maxAmount = maxAmount;
        }

        /** @param widget May be null. */
        public void SetFirstWidget(Element widget)
        {
            if (firstWidget != null) base.RemoveElement(firstWidget);
            firstWidget = widget;
            if (widget != null) base.AddElement(widget);
            Invalidate();
        }

        /** @param widget May be null. */
        public void SetSecondWidget(Element widget)
        {
            if (secondWidget != null) base.RemoveElement(secondWidget);
            secondWidget = widget;
            if (widget != null) base.AddElement(widget);
            Invalidate();
        }

        public override T AddElement<T>(T element)
        {
            throw new Exception("use SplitPane#setWidget");
        }

        public override bool RemoveElement(Element element)
        {
            if (element == null) throw new ArgumentNullException("actor cannot be null.");
            if (element == firstWidget)
            {
                SetFirstWidget(null);
                return true;
            }
            if (element == secondWidget)
            {
                SetSecondWidget(null);
                return true;
            }
            return true;
        }

        public bool IsCursorOverHandle()
        {
            return cursorOverHandle;
        }

        #region ILayout

        public override void Layout()
        {
            if (clampSplitAmount)
                ClampSplitAmount();
            if (!vertical)
                CalculateHorizBoundsAndPositions();
            else
                CalculateVertBoundsAndPositions();

            Element firstWidget = this.firstWidget;
            if (firstWidget != null)
            {
                Rect firstWidgetBounds = this.firstWidgetBounds;
                firstWidget.SetBounds(firstWidgetBounds.X, firstWidgetBounds.Y, firstWidgetBounds.Width, firstWidgetBounds.Height);
                if (firstWidget is ILayout) (firstWidget as ILayout).Validate();
            }
            Element secondWidget = this.secondWidget;
            if (secondWidget != null)
            {
                Rect secondWidgetBounds = this.secondWidgetBounds;
                secondWidget.SetBounds(secondWidgetBounds.X, secondWidgetBounds.Y, secondWidgetBounds.Width, secondWidgetBounds.Height);
                if (secondWidget is ILayout) (secondWidget as ILayout).Validate();
            }
        }

        public override float PreferredWidth
        {
            get
            {
                float first = firstWidget == null ? 0 : (firstWidget is ILayout ? (firstWidget as ILayout).PreferredWidth : firstWidget.GetWidth());
                float second = secondWidget == null ? 0 : (secondWidget is ILayout ? (secondWidget as ILayout).PreferredWidth : secondWidget.GetWidth());
                if (vertical) return Math.Max(first, second);
                return first + style.handle.MinWidth + second;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                float first = firstWidget == null ? 0 : (firstWidget is ILayout ? (firstWidget as ILayout).PreferredHeight : firstWidget.GetHeight());
                float second = secondWidget == null ? 0 : (secondWidget is ILayout ? (secondWidget as ILayout).PreferredHeight : secondWidget.GetHeight());
                if (!vertical) return Math.Max(first, second);
                return first + style.handle.MinHeight + second;
            }
        }

        public override float MinWidth
        {
            get
            {
                float first = firstWidget is ILayout ? (firstWidget as ILayout).MinWidth : 0;
                float second = secondWidget is ILayout ? (secondWidget as ILayout).MinWidth : 0;
                if (vertical) return Math.Max(first, second);
                return first + style.handle.MinWidth + second;
            }
        }

        public override float MinHeight
        {
            get
            {
                float first = firstWidget is ILayout ? (firstWidget as ILayout).MinHeight : 0;
                float second = secondWidget is ILayout ? (secondWidget as ILayout).MinHeight : 0;
                if (!vertical) return Math.Max(first, second);
                return first + style.handle.MinHeight + second;
            }
        }

        #endregion

        private void CalculateHorizBoundsAndPositions()
        {
            IDrawable handle = style.handle;

            float height = GetHeight();

            float availWidth = GetWidth() - handle.MinWidth;
            float leftAreaWidth = (int)(availWidth * splitAmount);
            float rightAreaWidth = availWidth - leftAreaWidth;
            float handleWidth = handle.MinWidth;

            firstWidgetBounds.Set(0, 0, leftAreaWidth, height);
            secondWidgetBounds.Set(leftAreaWidth + handleWidth, 0, rightAreaWidth, height);
            handleBounds.Set(leftAreaWidth, 0, handleWidth, height);
        }

        private void CalculateVertBoundsAndPositions()
        {
            IDrawable handle = style.handle;

            float width = GetWidth();
            float height = GetHeight();

            float availHeight = height - handle.MinHeight;
            float topAreaHeight = (int)(availHeight * splitAmount);
            float bottomAreaHeight = availHeight - topAreaHeight;
            float handleHeight = handle.MinHeight;

            firstWidgetBounds.Set(0, 0, width, topAreaHeight);
            secondWidgetBounds.Set(0, topAreaHeight + handle.MinHeight, width, bottomAreaHeight);
            handleBounds.Set(0, topAreaHeight, width, handleHeight);
        }
    }

    public class SplitPaneStyle
    {
        public IDrawable handle, handleOver;

        public SplitPaneStyle()
        {

        }
    }
}
