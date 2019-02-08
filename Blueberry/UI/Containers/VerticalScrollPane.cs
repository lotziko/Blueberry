using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public class VerticalScrollPane : Group
    {
        private Element widget;
        private VerticalScrollPaneStyle style;

        private float amountX, amountY, visualAmountX, visualAmountY, maxX, maxY;
        private float areaWidth, areaHeight;
        private bool scrollX, scrollY, disableX, disableY;

        private Rect widgetAreaBounds = new Rect();
        private Rect widgetCullingArea = new Rect();
        private Rect vScrollBounds = new Rect(), hScrollBounds = new Rect();
        private Rect vKnobBounds = new Rect(), hKnobBounds = new Rect();
        private Vec2 lastPoint = new Vec2();

        public VerticalScrollPane(Element widget, Skin skin, string stylename = "default") : this(widget, skin.Get<VerticalScrollPaneStyle>(stylename)) { }

        public VerticalScrollPane(Element widget, VerticalScrollPaneStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            SetElement(widget);
            SetSize(150, 150);

            AddCaptureListener(new InListener(this));
            AddListener(new ScrollListener(this));
            //listeners
        }

        #region Listeners

        private class InListener : InputListener
        {
            private readonly VerticalScrollPane p;

            private float handlePosition;
            private bool dragH, dragV;

            public InListener(VerticalScrollPane p)
            {
                this.p = p;
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                p.GetStage().SetScrollFocus(p);
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                p.GetStage().SetScrollFocus(null);
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer == 0 && button != 0) return false;
                p.GetStage().SetScrollFocus(p);

                if (p.scrollX && p.hScrollBounds.Contains(x, y))
                {
                    ev.Stop();
                    dragH = true;
                    if (p.hKnobBounds.Contains(x, y))
                    {
                        p.lastPoint.Set(x, y);
                        handlePosition = p.hKnobBounds.X;
                        return true;
                    }
                    p.SetScrollX(p.amountX + p.areaWidth * (x < p.hKnobBounds.X ? -1 : 1));
                    return true;
                }
                else if (p.scrollY && p.vScrollBounds.Contains(x, y))
                {
                    ev.Stop();
                    dragV = true;
                    if (p.vKnobBounds.Contains(x, y))
                    {
                        p.lastPoint.Set(x, y);
                        handlePosition = p.vKnobBounds.Y;
                        return true;
                    }
                    p.SetScrollY(p.amountY + p.areaHeight * (y < p.vKnobBounds.Y ? -1 : 1));
                    return true;
                }
                return false;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                dragH = false;
                dragV = false;
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                if (dragH)
                {
                    float delta = x - p.lastPoint.X;
                    float scrollH = handlePosition + delta;
                    handlePosition = scrollH;
                    scrollH = Math.Max(p.hScrollBounds.X, scrollH);
                    scrollH = Math.Min(p.hScrollBounds.X + p.hScrollBounds.Width - p.hKnobBounds.Width, scrollH);
                    float total = p.hScrollBounds.Width - p.hKnobBounds.Width;
                    if (total != 0) p.SetScrollPercentX((scrollH - p.hScrollBounds.X) / total);
                    p.lastPoint.Set(x, y);
                    Render.Request();
                }
                else if (dragV)
                {
                    float delta = y - p.lastPoint.Y;
                    float scrollV = handlePosition + delta;
                    handlePosition = scrollV;
                    scrollV = Math.Max(p.vScrollBounds.Y, scrollV);
                    scrollV = Math.Min(p.vScrollBounds.Y + p.vScrollBounds.Height - p.vKnobBounds.Height, scrollV);
                    float total = p.vScrollBounds.Height - p.vKnobBounds.Height;
                    if (total != 0) p.SetScrollPercentY(/*1 - */((scrollV - p.vScrollBounds.Y) / total));
                    p.lastPoint.Set(x, y);
                    Render.Request();
                }
            }
        }

        protected class ScrollListener : InputListener
        {
            private VerticalScrollPane p;

            public ScrollListener(VerticalScrollPane p)
            {
                this.p = p;
            }

            public override bool Scrolled(InputEvent ev, float x, float y, float amountX, float amountY)
            {
                if (p.scrollX || p.scrollY)
                {
                    if (p.scrollY)
                        p.SetScrollY(p.amountY + p.GetMouseWheelY() * -amountY);
                    if (p.scrollX)
                        p.SetScrollX(p.amountX + p.GetMouseWheelX() * amountX);
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        #endregion

        public override void Update(float delta)
        {
            base.Update(delta);

            if (visualAmountX != amountX)
            {
                if (visualAmountX < amountX)
                    VisualScrollX(Math.Min(amountX, visualAmountX + Math.Max(200 * delta, (amountX - visualAmountX) * 7 * delta)));
                else
                    VisualScrollX(Math.Max(amountX, visualAmountX - Math.Max(200 * delta, (visualAmountX - amountX) * 7 * delta)));
                Render.Request();
            }
            if (visualAmountY != amountY)
            {
                if (visualAmountY < amountY)
                    VisualScrollY(Math.Min(amountY, visualAmountY + Math.Max(200 * delta, (amountY - visualAmountY) * 7 * delta)));
                else
                    VisualScrollY(Math.Max(amountY, visualAmountY - Math.Max(200 * delta, (visualAmountY - amountY) * 7 * delta)));
                Render.Request();
            }
        }

        #region ILayout

        public override float MinWidth { get; } = 0;

        public override float MinHeight { get; } = 0;

        public override float PreferredWidth
        {
            get
            {
                if (widget is ILayout)
                {
                    var width = ((ILayout)widget).PreferredWidth;
                    if (style.background != null) width += style.background.LeftWidth + style.background.RightWidth;
                    return width;
                }
                return 150;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (widget is ILayout)
                {
                    var height = ((ILayout)widget).PreferredHeight;
                    if (style.background != null) height += style.background.TopHeight + style.background.BottomHeight;
                    return height;
                }
                return 150;
            }
        }

        public override void Layout()
        {
            IDrawable bg = style.background;
            IDrawable hScrollKnob = style.hScrollKnob;
            IDrawable vScrollKnob = style.vScrollKnob;

            float bgLeft = 0, bgRight = 0, bgTop = 0, bgBottom = 0;
            if (bg != null)
            {
                bgLeft = bg.LeftWidth;
                bgRight = bg.RightWidth;
                bgTop = bg.TopHeight;
                bgBottom = bg.BottomHeight;
            }

            float width = GetWidth(), height = GetHeight();

            float scrollbarHeight = 0;
            if (hScrollKnob != null) scrollbarHeight = hScrollKnob.MinHeight;
            if (style.hScroll != null) scrollbarHeight = Math.Max(scrollbarHeight, style.hScroll.MinHeight);
            float scrollbarWidth = 0;
            if (vScrollKnob != null) scrollbarWidth = vScrollKnob.MinWidth;
            if (style.vScroll != null) scrollbarWidth = Math.Max(scrollbarWidth, style.vScroll.MinWidth);

            // Get available space size by subtracting background's padded area.
            areaWidth = width - bgLeft - bgRight;
            areaHeight = height - bgTop - bgBottom;

            if (widget == null)
                return;

            // Get widget's desired width.
            float widgetWidth, widgetHeight;
            if (widget is ILayout)
            {
                var layout = widget as ILayout;
                widgetWidth = layout.PreferredWidth;
                widgetHeight = layout.PreferredHeight;
            }
            else
            {
                widgetWidth = widget.GetWidth();
                widgetHeight = widget.GetHeight();
            }

            scrollX = !disableX && widgetWidth > areaWidth;
            scrollY = !disableY && widgetHeight > areaHeight;

            if (scrollY)
            {
                areaWidth -= scrollbarWidth;
            }
            if (scrollX)
            {
                areaHeight -= scrollbarHeight;
            }

            // The bounds of the scrollable area for the widget.
            widgetAreaBounds.Set(bgLeft, bgTop, areaWidth, areaHeight);

            widgetWidth = disableX ? areaWidth : Math.Max(areaWidth, widgetWidth);
            widgetHeight = disableY ? areaHeight : Math.Max(areaHeight, widgetHeight);

            maxX = widgetWidth - areaWidth;
            maxY = widgetHeight - areaHeight;

            ScrollX(MathF.Clamp(amountX, 0, maxX));
            ScrollY(MathF.Clamp(amountY, 0, maxY));

            if (scrollX)
            {
                if (hScrollKnob != null)
                {
                    float hScrollHeight = style.hScroll != null ? style.hScroll.MinHeight : hScrollKnob.MinHeight;

                    float boundsX = bgLeft;
                    float boundsY = height - bgBottom - hScrollHeight;
                    hScrollBounds.Set(boundsX, boundsY, areaWidth, hScrollHeight);

                    hKnobBounds.Width = (int)Math.Max(hScrollKnob.MinWidth, (int)(hScrollBounds.Width * areaWidth / widgetWidth));

                    hKnobBounds.Height = (int)hScrollKnob.MinHeight;

                    hKnobBounds.X = hScrollBounds.X + (int)((hScrollBounds.Width - hKnobBounds.Width) * GetScrollPercentX());
                    hKnobBounds.Y = hScrollBounds.Y;
                }
                else
                {
                    hScrollBounds.Set(0, 0, 0, 0);
                    hKnobBounds.Set(0, 0, 0, 0);
                }
            }
            if (scrollY)
            {
                if (vScrollKnob != null)
                {
                    float vScrollWidth = style.vScroll != null ? style.vScroll.MinWidth : vScrollKnob.MinWidth;

                    float boundsX = boundsX = width - bgRight - vScrollWidth;
                    float boundsY = bgTop;

                    vScrollBounds.Set(boundsX, boundsY, vScrollWidth, areaHeight);
                    vKnobBounds.Width = (int)vScrollKnob.MinWidth;

                    vKnobBounds.Height = (int)Math.Max(vScrollKnob.MinHeight, (int)(vScrollBounds.Height * areaHeight / widgetHeight));

                    vKnobBounds.X = (int)(width - bgRight - vScrollKnob.MinWidth);

                    vKnobBounds.Y = vScrollBounds.Y + (int)((vScrollBounds.Height - vKnobBounds.Height) * GetScrollPercentY());
                }
                else
                {
                    vScrollBounds.Set(0, 0, 0, 0);
                    vKnobBounds.Set(0, 0, 0, 0);
                }
            }

            UpdateWidgetPosition();
            widget.SetSize(widgetWidth, widgetHeight);
            if (widget is ILayout) ((ILayout)widget).Validate();
        }

        private void UpdateWidgetPosition()
        {
            float x = widgetAreaBounds.X;
            if (scrollX) x -= (int)visualAmountX;

            float y = widgetAreaBounds.Y;
            if (scrollY) y -= (int)visualAmountY;

            widget.SetPosition(x, y);

            if (widget is ICullable)
            {
                widgetCullingArea.X = (int)(widgetAreaBounds.X - x);
                widgetCullingArea.Y = (int)(widgetAreaBounds.Y - y);
                widgetCullingArea.Width = widgetAreaBounds.Width;
                widgetCullingArea.Height = widgetAreaBounds.Height;
                ((ICullable)widget).SetCullingArea(widgetCullingArea);
            }
        }

        #endregion

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            if (widget == null) return;

            Validate();

            // Setup transform for this group.
            ApplyTransform(graphics, ComputeTransform());

            if (scrollX)
                hKnobBounds.X = hScrollBounds.X + (int)((hScrollBounds.Width - hKnobBounds.Width) * GetVisualScrollPercentX());
            if (scrollY)
                vKnobBounds.Y = vScrollBounds.Y + (int)((vScrollBounds.Height - vKnobBounds.Height) * GetVisualScrollPercentY());

            UpdateWidgetPosition();

            // Draw the background ninepatch.
            var color = new Col(this.color, this.color.A * parentAlpha);

            if (style.background != null)
            {
                style.background.Draw(graphics, 0, 0, GetWidth(), GetHeight(), color);
                graphics.Flush();
            }

            // caculate the scissor bounds based on the batch transform, the available widget area and the camera transform. We need to
            // project those to screen coordinates for OpenGL to consume.
            var scissor = stage.CalculateScissors(widgetAreaBounds, this);//ScissorStack.CalculateScissors(stage?.entity?.scene?.camera, graphics.TransformMatrix, widgetAreaBounds);
            if (ScissorStack.PushScissors(scissor))
            {
                DrawElements(graphics, parentAlpha);
                graphics.Flush();
                ScissorStack.PopScissors();
            }

            float alpha = color.A;
            color = new Col(color, alpha);
            if (alpha > 0f)
            {
                if (scrollX)
                {
                    if (style.hScroll != null)
                        style.hScroll.Draw(graphics, hScrollBounds.X, hScrollBounds.Y, hScrollBounds.Width, hScrollBounds.Height, color);
                    if (style.hScrollKnob != null)
                        style.hScrollKnob.Draw(graphics, hKnobBounds.X, hKnobBounds.Y, hKnobBounds.Width, hKnobBounds.Height, color);
                }
                if (scrollY)
                {
                    if (style.vScroll != null)
                        style.vScroll.Draw(graphics, vScrollBounds.X, vScrollBounds.Y, vScrollBounds.Width, vScrollBounds.Height, color);
                    if (style.vScrollKnob != null)
                        style.vScrollKnob.Draw(graphics, vKnobBounds.X, vKnobBounds.Y, vKnobBounds.Width, vKnobBounds.Height, color);
                }
            }

            ResetTransform(graphics);
        }

        public void SetElement(Element widget)
        {
            if (widget == this) throw new Exception("widget cannot be the ScrollPane.");
            if (this.widget != null) base.RemoveElement(this.widget);
            this.widget = widget;
            if (widget != null) base.AddElement(widget);
        }

        public Element GetElement() => widget;

        public override bool RemoveElement(Element element)
        {
            if (element == null) throw new ArgumentNullException("element cannot be null.");
            if (element != widget) return false;
            SetElement(null);
            return true;
        }

        public override Element Hit(float x, float y, bool touchable = true)
        {
            if (x < 0 || x >= GetWidth() || y < 0 || y >= GetHeight()) return null;
            if (touchable && GetTouchable() == Touchable.Enabled && IsVisible())
            {
                if (scrollX && hScrollBounds.Contains(x, y)) return this;
                if (scrollY && vScrollBounds.Contains(x, y)) return this;
            }
            return base.Hit(x, y, touchable);
        }

        /// <summary>
        /// Disables scrolling in a direction. The widget will be sized to the FlickScrollPane in the disabled direction.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetScrollingDisabled(bool x, bool y)
        {
            disableX = x;
            disableY = y;
        }

        public bool IsScrollingDisabledX()
        {
            return disableX;
        }

        public bool IsScrollingDisabledY()
        {
            return disableY;
        }

        protected float GetMouseWheelX()
        {
            return Math.Min(areaWidth, Math.Max(areaWidth * 0.9f, maxX * 0.1f) / 4);
        }

        protected float GetMouseWheelY()
        {
            return Math.Min(areaHeight, Math.Max(areaHeight * 0.9f, maxY * 0.1f) / 4);
        }

        /// <summary>
        /// Called whenever the x scroll amount is changed.
        /// </summary>
        /// <param name="pixelsX"></param>
        protected void ScrollX(float pixelsX)
        {
            amountX = pixelsX;
        }

        /// <summary>
        /// Called whenever the y scroll amount is changed.
        /// </summary>
        /// <param name="pixelsY"></param>
        protected void ScrollY(float pixelsY)
        {
            amountY = pixelsY;
        }

        /// <summary>
        /// Called whenever the visual x scroll amount is changed.
        /// </summary>
        /// <param name="pixelsX"></param>
        protected void VisualScrollX(float pixelsX)
        {
            visualAmountX = pixelsX;
        }

        /// <summary>
        /// Called whenever the visual y scroll amount is changed.
        /// </summary>
        /// <param name="pixelsY"></param>
        protected void VisualScrollY(float pixelsY)
        {
            visualAmountY = pixelsY;
        }

        public void SetScrollX(float pixels)
        {
            ScrollX(MathF.Clamp(pixels, 0, maxX));
        }

        public void SetScrollY(float pixels)
        {
            ScrollY(MathF.Clamp(pixels, 0, maxY));
        }

        /// <summary>
        /// Returns the x scroll position in pixels, where 0 is the left of the scroll pane.
        /// </summary>
        /// <returns></returns>
        public float GetScrollX()
        {
            return amountX;
        }

        /// <summary>
        /// Returns the y scroll position in pixels, where 0 is the top of the scroll pane.
        /// </summary>
        /// <returns></returns>
        public float GetScrollY()
        {
            return amountY;
        }

        public float GetVisualScrollX()
        {
            return visualAmountX;
        }

        public float GetVisualScrollY()
        {
            return visualAmountY;
        }

        public float GetVisualScrollPercentX()
        {
            return MathF.Clamp(visualAmountX / maxX, 0, 1);
        }

        public float GetVisualScrollPercentY()
        {
            return MathF.Clamp(visualAmountY / maxY, 0, 1);
        }

        public float GetScrollPercentX()
        {
            return MathF.Clamp(amountX / maxX, 0, 1);
        }

        public float GetScrollPercentY()
        {
            return MathF.Clamp(amountY / maxY, 0, 1);
        }

        public void SetScrollPercentX(float percentX)
        {
            ScrollX(maxX * MathF.Clamp(percentX, 0, 1));
        }

        public void SetScrollPercentY(float percentY)
        {
            ScrollY(maxY * MathF.Clamp(percentY, 0, 1));
        }
    }

    public class VerticalScrollPaneStyle
    {
        public IDrawable background;

        public IDrawable vScroll, vScrollKnob, hScroll, hScrollKnob;
    }
}
