﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public class ScrollPane : Group
    {
        private ScrollPaneStyle style;
        private Element widget;

        Rect hScrollBounds = new Rect();
        Rect vScrollBounds = new Rect();
        Rect hKnobBounds = new Rect();
        Rect vKnobBounds = new Rect();
        private Rect widgetAreaBounds = new Rect();
        private Rect widgetCullingArea = new Rect();
        private readonly Rect scissorBounds = new Rect();
        private ElementGestureListener flickScrollListener;

        bool cursorOverHKnob, cursorOverVKnob;
        IDrawable hKnob, vKnob;

        bool scrollX, scrollY;
        bool vScrollOnRight = true, hScrollOnBottom = true;
        float amountX, amountY;
        float visualAmountX, visualAmountY;
        float maxX, maxY;
        bool touchScrollH, touchScrollV;
        Vec2 lastPoint = new Vec2();
        float areaWidth, areaHeight;
        bool fadeScrollBars = true, smoothScrolling = true, scrollBarTouch = true;
        float fadeAlpha, fadeAlphaSeconds = 1, fadeDelay, fadeDelaySeconds = 1;
        bool cancelTouchFocus = true;

        bool flickScroll = true;
        float velocityX, velocityY;
        float flingTimer;
        private bool overscrollX = true, overscrollY = true;
        float flingTime = 0.1f;
        private float overscrollDistance = 50, overscrollSpeedMin = 30, overscrollSpeedMax = 200;
        private bool forceScrollX, forceScrollY;
        protected bool disableX, disableY;
        private bool clamp = true;
        private bool scrollbarsOnTop;
        private bool variableSizeKnobs = true;
        int draggingPointer = -1;

        public ScrollPane(Element widget, Skin skin, string stylename = "default") : this(widget, skin.Get<ScrollPaneStyle>(stylename)) { }

        public ScrollPane(Element widget, ScrollPaneStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            SetElement(widget);
            SetSize(150, 150);

            AddCaptureListener(new InListener(this));
            AddListener(flickScrollListener = new FlickListener(this));
            AddListener(new ScrollListener(this));
            //listeners
        }

        #region Listeners

        protected class InListener : InputListener<ScrollPane>
        {
            private float handlePosition;

            public InListener(ScrollPane par) : base(par)
            {
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (par.draggingPointer != -1) return false;
                if (pointer == 0 && button != 0) return false;
                par.GetStage().SetScrollFocus(par);

                if (!par.flickScroll) par.ResetFade();

                if (par.fadeAlpha == 0) return false;

                if (par.scrollBarTouch && par.scrollX && par.hScrollBounds.Contains(x, y))
                {
                    ev.Stop();
                    par.ResetFade();
                    if (par.hKnobBounds.Contains(x, y))
                    {
                        par.lastPoint.Set(x, y);
                        handlePosition = par.hKnobBounds.X;
                        par.touchScrollH = true;
                        par.draggingPointer = pointer;
                        return true;
                    }
                    par.SetScrollX(par.amountX + par.areaWidth * (x < par.hKnobBounds.X ? -1 : 1));
                    return true;
                }
                if (par.scrollBarTouch && par.scrollY && par.vScrollBounds.Contains(x, y))
                {
                    ev.Stop();
                    par.ResetFade();
                    if (par.vKnobBounds.Contains(x, y))
                    {
                        par.lastPoint.Set(x, y);
                        handlePosition = par.vKnobBounds.Y;
                        par.touchScrollV = true;
                        par.draggingPointer = pointer;
                        return true;
                    }
                    par.SetScrollY(par.amountY + par.areaHeight * (y < par.vKnobBounds.Y ? -1 : 1));
                    return true;
                }
                return false;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != par.draggingPointer) return;
                par.Cancel();
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                if (pointer != par.draggingPointer) return;
                if (par.touchScrollH)
                {
                    float delta = x - par.lastPoint.X;
                    float scrollH = handlePosition + delta;
                    handlePosition = scrollH;
                    scrollH = Math.Max(par.hScrollBounds.X, scrollH);
                    scrollH = Math.Min(par.hScrollBounds.X + par.hScrollBounds.Width - par.hKnobBounds.Width, scrollH);
                    float total = par.hScrollBounds.Width - par.hKnobBounds.Width;
                    if (total != 0) par.SetScrollPercentX((scrollH - par.hScrollBounds.X) / total);
                    par.lastPoint.Set(x, y);
                }
                else if (par.touchScrollV)
                {
                    float delta = y - par.lastPoint.Y;
                    float scrollV = handlePosition + delta;
                    handlePosition = scrollV;
                    scrollV = Math.Max(par.vScrollBounds.Y, scrollV);
                    scrollV = Math.Min(par.vScrollBounds.Y + par.vScrollBounds.Height - par.vKnobBounds.Height, scrollV);
                    float total = par.vScrollBounds.Height - par.vKnobBounds.Height;
                    if (total != 0) par.SetScrollPercentY(/*1 - */((scrollV - par.vScrollBounds.Y) / total));
                    par.lastPoint.Set(x, y);
                }
                Render.Request();
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                par.cursorOverHKnob = par.hKnobBounds.Contains(x, y);
                par.cursorOverVKnob = par.vKnobBounds.Contains(x, y);

                if (!par.flickScroll) par.ResetFade();
                return false;
            }
        }

        protected class FlickListener : ElementGestureListener
        {
            private ScrollPane par;

            public FlickListener(ScrollPane p)
            {
                par = p;
            }

            public override void Pan(InputEvent ev, float x, float y, float deltaX, float deltaY)
            {
                par.ResetFade();
                par.amountX -= deltaX;
                par.amountY -= deltaY;
                par.Clamp();
                Render.Request();
                if (par.cancelTouchFocus && ((par.scrollX && deltaX != 0) || (par.scrollY && deltaY != 0))) par.CancelTouchFocus();
            }

            public override void Fling(InputEvent ev, float velocityX, float velocityY, int button)
            {
                if (Math.Abs(velocityX) > 150 && par.scrollX)
                {
                    par.flingTimer = par.flingTime;
                    par.velocityX = velocityX / 2;
                    if (par.cancelTouchFocus) par.CancelTouchFocus();
                }
                if (Math.Abs(velocityY) > 150 && par.scrollY)
                {
                    par.flingTimer = par.flingTime;
                    par.velocityY = velocityY / 2;
                    if (par.cancelTouchFocus) par.CancelTouchFocus();
                }
            }

            public override bool Handle(Event e)
            {
                if (base.Handle(e))
                {
                    if ((e as InputEvent).GetInputType() == InputType.touchDown) par.flingTimer = 0;
                    return true;
                }
                else if (e is InputEvent && (e as InputEvent).IsTouchFocusCancel())
                    par.Cancel();
                return false;
            }
        }

        protected class ScrollListener : InputListener<ScrollPane>
        {
            public ScrollListener(ScrollPane par) : base(par)
            {
            }

            public override bool Scrolled(InputEvent ev, float x, float y, float amountX, float amountY)
            {
                par.ResetFade();
                if (par.scrollY)
                    par.SetScrollY(par.amountY + par.GetMouseWheelY() * -amountY);
                else if (par.scrollX) //
                    par.SetScrollX(par.amountX + par.GetMouseWheelX() * amountY);
                else
                    return false;
                return true;
            }
        }

        #endregion

        void ResetFade()
        {
            fadeAlpha = fadeAlphaSeconds;
            fadeDelay = fadeDelaySeconds;
        }

        /** Cancels the stage's touch focus for all listeners except this scroll pane's flick scroll listener. This causes any widgets
	 * inside the scrollpane that have received touchDown to receive touchUp.
	 * @see #setCancelTouchFocus(bool) */
        public void CancelTouchFocus()
        {
            Stage stage = GetStage();
            if (stage != null) stage.CancelTouchFocusExcept(flickScrollListener, this);
        }

        /** If currently scrolling by tracking a touch down, stop scrolling. */
        public void Cancel()
        {
            draggingPointer = -1;
            touchScrollH = false;
            touchScrollV = false;
            flickScrollListener.GetGestureDetector().Cancel();
        }

        void Clamp()
        {
            if (!clamp) return;
            ScrollX(overscrollX ? MathF.Clamp(amountX, -overscrollDistance, maxX + overscrollDistance)
                : MathF.Clamp(amountX, 0, maxX));
            ScrollY(overscrollY ? MathF.Clamp(amountY, -overscrollDistance, maxY + overscrollDistance)
                : MathF.Clamp(amountY, 0, maxY));
        }

        public void SetStyle(ScrollPaneStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            InvalidateHierarchy();
        }

        /** Returns the scroll pane's style. Modifying the returned style may not have an effect until
	 * {@link #setStyle(ScrollPaneStyle)} is called. */
        public ScrollPaneStyle GetStyle()
        {
            return style;
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            UpdateKnobs();

            bool panning = flickScrollListener.GetGestureDetector().IsPanning();
            bool animating = false;

            if (fadeAlpha > 0 && fadeScrollBars && !panning && !touchScrollH && !touchScrollV)
            {
                fadeDelay -= delta;
                if (fadeDelay <= 0)
                    fadeAlpha = Math.Max(0, fadeAlpha - delta);
                animating = true;
            }

            if (flingTimer > 0)
            {
                ResetFade();

                float alpha = flingTimer / flingTime;
                amountX -= velocityX * alpha * delta;
                amountY -= velocityY * alpha * delta;
                Clamp();

                // Stop fling if hit overscroll distance.
                if (amountX == -overscrollDistance) velocityX = 0;
                if (amountX >= maxX + overscrollDistance) velocityX = 0;
                if (amountY == -overscrollDistance) velocityY = 0;
                if (amountY >= maxY + overscrollDistance) velocityY = 0;

                flingTimer -= delta;
                if (flingTimer <= 0)
                {
                    velocityX = 0;
                    velocityY = 0;
                }

                animating = true;
            }

            if (smoothScrolling && flingTimer <= 0 && !panning && //
                                                                  // Scroll smoothly when grabbing the scrollbar if one pixel of scrollbar movement is > 10% of the scroll area.
                ((!touchScrollH || (scrollX && maxX / (hScrollBounds.Width - hKnobBounds.Width) > areaWidth * 0.1f)) //
                    && (!touchScrollV || (scrollY && maxY / (vScrollBounds.Height - vKnobBounds.Height) > areaHeight * 0.1f))) //
            )
            {
                if (visualAmountX != amountX)
                {
                    if (visualAmountX < amountX)
                        VisualScrollX(Math.Min(amountX, visualAmountX + Math.Max(200 * delta, (amountX - visualAmountX) * 7 * delta)));
                    else
                        VisualScrollX(Math.Max(amountX, visualAmountX - Math.Max(200 * delta, (visualAmountX - amountX) * 7 * delta)));
                    animating = true;
                }
                if (visualAmountY != amountY)
                {
                    if (visualAmountY < amountY)
                        VisualScrollY(Math.Min(amountY, visualAmountY + Math.Max(200 * delta, (amountY - visualAmountY) * 7 * delta)));
                    else
                        VisualScrollY(Math.Max(amountY, visualAmountY - Math.Max(200 * delta, (visualAmountY - amountY) * 7 * delta)));
                    animating = true;
                }
            }
            else
            {
                if (visualAmountX != amountX) VisualScrollX(amountX);
                if (visualAmountY != amountY) VisualScrollY(amountY);
            }

            if (!panning)
            {
                if (overscrollX && scrollX)
                {
                    if (amountX < 0)
                    {
                        ResetFade();
                        amountX += (overscrollSpeedMin + (overscrollSpeedMax - overscrollSpeedMin) * -amountX / overscrollDistance)
                            * delta;
                        if (amountX > 0) ScrollX(0);
                        animating = true;
                    }
                    else if (amountX > maxX)
                    {
                        ResetFade();
                        amountX -= (overscrollSpeedMin
                            + (overscrollSpeedMax - overscrollSpeedMin) * -(maxX - amountX) / overscrollDistance) * delta;
                        if (amountX < maxX) ScrollX(maxX);
                        animating = true;
                    }
                }
                if (overscrollY && scrollY)
                {
                    if (amountY < 0)
                    {
                        ResetFade();
                        amountY += (overscrollSpeedMin + (overscrollSpeedMax - overscrollSpeedMin) * -amountY / overscrollDistance)
                            * delta;
                        if (amountY > 0) ScrollY(0);
                        animating = true;
                    }
                    else if (amountY > maxY)
                    {
                        ResetFade();
                        amountY -= (overscrollSpeedMin
                            + (overscrollSpeedMax - overscrollSpeedMin) * -(maxY - amountY) / overscrollDistance) * delta;
                        if (amountY < maxY) ScrollY(maxY);
                        animating = true;
                    }
                }
            }

            if (animating)
            {
                Render.Request();
            }
        }

        protected void UpdateKnobs()
        {
            IDrawable knob = null;
            if (touchScrollH && style.hScrollKnobDown != null)
                knob = style.hScrollKnobDown;
            else if (cursorOverHKnob && style.hScrollKnobOver != null)
                knob = style.hScrollKnobOver;
            else
                knob = style.hScrollKnob;
            if (knob != hKnob)
            {
                hKnob = knob;
                Render.Request();
            }

            knob = null;
            if (touchScrollV && style.vScrollKnobDown != null)
                knob = style.vScrollKnobDown;
            else if (cursorOverVKnob && style.vScrollKnobOver != null)
                knob = style.vScrollKnobOver;
            else
                knob = style.vScrollKnob;
            if (knob != vKnob)
            {
                vKnob = knob;
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
                    if (forceScrollY)
                    {
                        var scrollbarWidth = 0f;
                        if (style.vScrollKnob != null) scrollbarWidth = style.vScrollKnob.MinWidth;
                        if (style.vScroll != null) scrollbarWidth = Math.Max(scrollbarWidth, style.vScroll.MinWidth);
                        width += scrollbarWidth;
                    }
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
                    if (forceScrollX)
                    {
                        var scrollbarHeight = 0f;
                        if (style.hScrollKnob != null) scrollbarHeight = style.hScrollKnob.MinHeight;
                        if (style.hScroll != null) scrollbarHeight = Math.Max(scrollbarHeight, style.hScroll.MinHeight);
                        height += scrollbarHeight;
                    }
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

            float bgLeftWidth = 0, bgRightWidth = 0, bgTopHeight = 0, bgBottomHeight = 0;
            if (bg != null)
            {
                bgLeftWidth = bg.LeftWidth;
                bgRightWidth = bg.RightWidth;
                bgTopHeight = bg.TopHeight;
                bgBottomHeight = bg.BottomHeight;
            }

            float width = GetWidth();
            float height = GetHeight();

            float scrollbarHeight = 0;
            if (hScrollKnob != null) scrollbarHeight = hScrollKnob.MinHeight;
            if (style.hScroll != null) scrollbarHeight = Math.Max(scrollbarHeight, style.hScroll.MinHeight);
            float scrollbarWidth = 0;
            if (vScrollKnob != null) scrollbarWidth = vScrollKnob.MinWidth;
            if (style.vScroll != null) scrollbarWidth = Math.Max(scrollbarWidth, style.vScroll.MinWidth);

            // Get available space size by subtracting background's padded area.
            areaWidth = width - bgLeftWidth - bgRightWidth;
            areaHeight = height - bgTopHeight - bgBottomHeight;

            if (widget == null) return;

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

            // Determine if horizontal/vertical scrollbars are needed.
            scrollX = forceScrollX || (widgetWidth > areaWidth && !disableX);
            scrollY = forceScrollY || (widgetHeight > areaHeight && !disableY);

            bool fade = fadeScrollBars;
            if (!fade)
            {
                // Check again, now taking into account the area that's taken up by any enabled scrollbars.
                if (scrollY)
                {
                    areaWidth -= scrollbarWidth;
                    if (!scrollX && widgetWidth > areaWidth && !disableX) scrollX = true;
                }
                if (scrollX)
                {
                    areaHeight -= scrollbarHeight;
                    if (!scrollY && widgetHeight > areaHeight && !disableY)
                    {
                        scrollY = true;
                        areaWidth -= scrollbarWidth;
                    }
                }
            }

            // The bounds of the scrollable area for the widget.
            widgetAreaBounds.Set(bgLeftWidth, bgTopHeight, areaWidth, areaHeight);

            if (fade)
            {
                // Make sure widget is drawn under fading scrollbars.
                if (scrollX && scrollY)
                {
                    areaHeight -= scrollbarHeight;
                    areaWidth -= scrollbarWidth;
                }
            }
            else
            {
                if (scrollbarsOnTop)
                {
                    // Make sure widget is drawn under non-fading scrollbars.
                    if (scrollX) widgetAreaBounds./*Height*/Y += (int)scrollbarHeight;
                    //if (scrollY) widgetAreaBounds.Width += (int)scrollbarWidth;
                }
                else
                {
                    // Offset widget area y for horizontal scrollbar at bottom.
                    //if (scrollX && hScrollOnBottom) widgetAreaBounds.Y += (int)scrollbarHeight;
                    // Offset widget area x for vertical scrollbar at left.
                    if (scrollY && !vScrollOnRight) widgetAreaBounds.X += (int)scrollbarWidth;
                }
            }

            // If the widget is smaller than the available space, make it take up the available space.
            widgetWidth = disableX ? areaWidth : Math.Max(areaWidth, widgetWidth);
            widgetHeight = disableY ? areaHeight : Math.Max(areaHeight, widgetHeight);

            maxX = widgetWidth - areaWidth;
            maxY = widgetHeight - areaHeight;
            if (fade)
            {
                // Make sure widget is drawn under fading scrollbars.
                if (scrollX && scrollY)
                {
                    maxY -= scrollbarHeight;
                    maxX -= scrollbarWidth;
                }
            }
            if (!IsPanning())
            {
                ScrollX(MathF.Clamp(amountX, 0, maxX));
                ScrollY(MathF.Clamp(amountY, 0, maxY));
            }

            // Set the bounds and scroll knob sizes if scrollbars are needed.
            if (scrollX)
            {
                if (hScrollKnob != null)
                {
                    float hScrollHeight = style.hScroll != null ? style.hScroll.MinHeight : hScrollKnob.MinHeight;
                    // The corner gap where the two scroll bars intersect might have to flip from right to left.
                    float boundsX = vScrollOnRight ? bgLeftWidth : bgLeftWidth + scrollbarWidth;
                    // Scrollbar on the top or bottom.
                    float boundsY = hScrollOnBottom ? height + bgTopHeight - hScrollHeight : bgTopHeight;//bgBottomHeight : height - bgTopHeight - hScrollHeight;
                    hScrollBounds.Set(boundsX, boundsY, areaWidth, hScrollHeight);
                    if (variableSizeKnobs)
                        hKnobBounds.Width = (int)Math.Max(hScrollKnob.MinWidth, (int)(hScrollBounds.Width * areaWidth / widgetWidth));
                    else
                        hKnobBounds.Width = (int)hScrollKnob.MinWidth;

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
                    // the small gap where the two scroll bars intersect might have to flip from bottom to top
                    float boundsX, boundsY;
                    if (hScrollOnBottom)
                    {
                        boundsY = bgTopHeight;//height - bgTopHeight - areaHeight;
                    }
                    else
                    {
                        boundsY = bgTopHeight + scrollbarHeight;//bgTopHeight + areaHeight;//bgBottomHeight;
                    }
                    // bar on the left or right
                    if (vScrollOnRight)
                    {
                        boundsX = width - bgRightWidth - vScrollWidth;
                    }
                    else
                    {
                        boundsX = bgLeftWidth;
                    }
                    vScrollBounds.Set(boundsX, boundsY, vScrollWidth, areaHeight);
                    vKnobBounds.Width = (int)vScrollKnob.MinWidth;
                    if (variableSizeKnobs)
                        vKnobBounds.Height = (int)Math.Max(vScrollKnob.MinHeight, (int)(vScrollBounds.Height * areaHeight / widgetHeight));
                    else
                        vKnobBounds.Height = (int)vScrollKnob.MinHeight;

                    if (vScrollOnRight)
                    {
                        vKnobBounds.X = (int)(width - bgRightWidth - vScrollKnob.MinWidth);
                    }
                    else
                    {
                        vKnobBounds.X = (int)bgLeftWidth;
                    }
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
            // Calculate the widget's position depending on the scroll state and available widget area.
            float y = widgetAreaBounds.Y;
            /*if (!scrollY)
                y -= (int)maxY;
            else
                y -= (int)(/*maxY - *//*visualAmountY);*/
            if (scrollY) y -= (int)visualAmountY;

            float x = widgetAreaBounds.X;
            if (scrollX) x -= (int)visualAmountX;

            if (!fadeScrollBars && scrollbarsOnTop)
            {
                if (scrollX && hScrollOnBottom)
                {
                    float scrollbarHeight = 0;
                    if (style.hScrollKnob != null) scrollbarHeight = style.hScrollKnob.MinHeight;
                    if (style.hScroll != null) scrollbarHeight = Math.Max(scrollbarHeight, style.hScroll.MinHeight);
                    y += scrollbarHeight;
                }
                if (scrollY && !vScrollOnRight)
                {
                    float scrollbarWidth = 0;
                    if (style.hScrollKnob != null) scrollbarWidth = style.hScrollKnob.MinWidth;
                    if (style.hScroll != null) scrollbarWidth = Math.Max(scrollbarWidth, style.hScroll.MinWidth);
                    x += scrollbarWidth;
                }
            }

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

            float alpha = color.A * Interpolation.fade.Apply(fadeAlpha / fadeAlphaSeconds);
            color = new Col(color, alpha);
            if (alpha > 0f)
            {
                if (scrollX && scrollY)
                {
                    if (style.corner != null)
                    {
                        style.corner.Draw(graphics, /*hScrollBounds.X + hScrollBounds.Width*/vScrollBounds.X, hScrollBounds.Y, vScrollBounds.Width, vScrollBounds.Y, color);
                    }
                }
                if (scrollX)
                {
                    if (style.hScroll != null)
                        style.hScroll.Draw(graphics, hScrollBounds.X, hScrollBounds.Y, hScrollBounds.Width, hScrollBounds.Height, color);
                    if (style.hScrollKnob != null)
                        hKnob.Draw(graphics, hKnobBounds.X, hKnobBounds.Y, hKnobBounds.Width, hKnobBounds.Height, color);
                }
                if (scrollY)
                {
                    if (style.vScroll != null)
                        style.vScroll.Draw(graphics, vScrollBounds.X, vScrollBounds.Y, vScrollBounds.Width, vScrollBounds.Height, color);
                    if (style.vScrollKnob != null)
                        vKnob.Draw(graphics, vKnobBounds.X, vKnobBounds.Y, vKnobBounds.Width, vKnobBounds.Height, color);
                }
            }

            ResetTransform(graphics);
        }


        /** Generate fling gesture.
	 * @param flingTime Time in seconds for which you want to fling last.
	 * @param velocityX Velocity for horizontal direction.
	 * @param velocityY Velocity for vertical direction. */
        public void Fling(float flingTime, float velocityX, float velocityY)
        {
            flingTimer = flingTime;
            this.velocityX = velocityX;
            this.velocityY = velocityY;
        }

        #region Getters/Setters

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
                if (scrollX && touchScrollH && hScrollBounds.Contains(x, y)) return this;
                if (scrollY && touchScrollV && vScrollBounds.Contains(x, y)) return this;
            }
            return base.Hit(x, y, touchable);
        }

        /** Called whenever the x scroll amount is changed. */
        protected void ScrollX(float pixelsX)
        {
            amountX = pixelsX;
        }

        /** Called whenever the y scroll amount is changed. */
        protected void ScrollY(float pixelsY)
        {
            amountY = pixelsY;
        }

        /** Called whenever the visual x scroll amount is changed. */
        protected void VisualScrollX(float pixelsX)
        {
            visualAmountX = pixelsX;
        }

        /** Called whenever the visual y scroll amount is changed. */
        protected void VisualScrollY(float pixelsY)
        {
            visualAmountY = pixelsY;
        }

        /** Returns the amount to scroll horizontally when the mouse wheel is scrolled. */
        protected float GetMouseWheelX()
        {
            return Math.Min(areaWidth, Math.Max(areaWidth * 0.9f, maxX * 0.1f) / 4);
        }

        /** Returns the amount to scroll vertically when the mouse wheel is scrolled. */
        protected float GetMouseWheelY()
        {
            return Math.Min(areaHeight, Math.Max(areaHeight * 0.9f, maxY * 0.1f) / 4);
        }

        public void SetScrollX(float pixels)
        {
            ScrollX(MathF.Clamp(pixels, 0, maxX));
        }

        /** Returns the x scroll position in pixels, where 0 is the left of the scroll pane. */
        public float GetScrollX()
        {
            return amountX;
        }

        public void SetScrollY(float pixels)
        {
            ScrollY(MathF.Clamp(pixels, 0, maxY));
        }

        /** Returns the y scroll position in pixels, where 0 is the top of the scroll pane. */
        public float GetScrollY()
        {
            return amountY;
        }

        /** Sets the visual scroll amount equal to the scroll amount. This can be used when setting the scroll amount without
         * animating. */
        public void UpdateVisualScroll()
        {
            visualAmountX = amountX;
            visualAmountY = amountY;
        }

        public float GetVisualScrollX()
        {
            return !scrollX ? 0 : visualAmountX;
        }

        public float GetVisualScrollY()
        {
            return !scrollY ? 0 : visualAmountY;
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

        public void SetScrollPercentX(float percentX)
        {
            ScrollX(maxX * MathF.Clamp(percentX, 0, 1));
        }

        public float GetScrollPercentY()
        {
            return MathF.Clamp(amountY / maxY, 0, 1);
        }

        public void SetScrollPercentY(float percentY)
        {
            ScrollY(maxY * MathF.Clamp(percentY, 0, 1));
        }

        public void SetFlickScroll(bool flickScroll)
        {
            if (this.flickScroll == flickScroll) return;
            this.flickScroll = flickScroll;
            if (flickScroll)
                AddListener(flickScrollListener);
            else
                RemoveListener(flickScrollListener);
            Invalidate();
        }

        public void SetFlickScrollTapSquareSize(float halfTapSquareSize)
        {
            flickScrollListener.GetGestureDetector().SetTapSquareSize(halfTapSquareSize);
        }

        /** Sets the scroll offset so the specified rectangle is fully in view, if possible. Coordinates are in the scroll pane
         * widget's coordinate system. */
        public void ScrollTo(float x, float y, float width, float height)
        {
            ScrollTo(x, y, width, height, false, false);
        }

        /** Sets the scroll offset so the specified rectangle is fully in view, and optionally centered vertically and/or horizontally,
         * if possible. Coordinates are in the scroll pane widget's coordinate system. */
        public void ScrollTo(float x, float y, float width, float height, bool centerHorizontal, bool centerVertical)
        {
            float amountX = this.amountX;
            if (centerHorizontal)
            {
                amountX = x - areaWidth / 2 + width / 2;
            }
            else
            {
                if (x + width > amountX + areaWidth) amountX = x + width - areaWidth;
                if (x < amountX) amountX = x;
            }
            ScrollX(MathF.Clamp(amountX, 0, maxX));

            float amountY = this.amountY;
            if (centerVertical)
            {
                amountY = maxY - y + areaHeight / 2 - height / 2;
            }
            else
            {
                if (amountY > maxY - y - height + areaHeight) amountY = maxY - y - height + areaHeight;
                if (amountY < maxY - y) amountY = maxY - y;
            }
            ScrollY(MathF.Clamp(amountY, 0, maxY));
        }

        /** Returns the maximum scroll value in the x direction. */
        public float GetMaxX()
        {
            return maxX;
        }

        /** Returns the maximum scroll value in the y direction. */
        public float GetMaxY()
        {
            return maxY;
        }

        public float GetScrollBarHeight()
        {
            if (!scrollX) return 0;
            float height = 0;
            if (style.hScrollKnob != null) height = style.hScrollKnob.MinHeight;
            if (style.hScroll != null) height = Math.Max(height, style.hScroll.MinHeight);
            return height;
        }

        public float GetScrollBarWidth()
        {
            if (!scrollY) return 0;
            float width = 0;
            if (style.vScrollKnob != null) width = style.vScrollKnob.MinWidth;
            if (style.vScroll != null) width = Math.Max(width, style.vScroll.MinWidth);
            return width;
        }

        /** Returns the width of the scrolled viewport. */
        public float GetScrollWidth()
        {
            return areaWidth;
        }

        /** Returns the height of the scrolled viewport. */
        public float GetScrollHeight()
        {
            return areaHeight;
        }

        /** Returns true if the widget is larger than the scroll pane horizontally. */
        public bool IsScrollX()
        {
            return scrollX;
        }

        /** Returns true if the widget is larger than the scroll pane vertically. */
        public bool IsScrollY()
        {
            return scrollY;
        }

        /** Disables scrolling in a direction. The widget will be sized to the FlickScrollPane in the disabled direction. */
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

        public bool IsLeftEdge()
        {
            return !scrollX || amountX <= 0;
        }

        public bool IsRightEdge()
        {
            return !scrollX || amountX >= maxX;
        }

        public bool IsTopEdge()
        {
            return !scrollY || amountY <= 0;
        }

        public bool IsBottomEdge()
        {
            return !scrollY || amountY >= maxY;
        }

        public bool IsDragging()
        {
            return draggingPointer != -1;
        }

        public bool IsPanning()
        {
            return flickScrollListener.GetGestureDetector().IsPanning();
        }

        public bool IsFlinging()
        {
            return flingTimer > 0;
        }

        public void SetVelocityX(float velocityX)
        {
            this.velocityX = velocityX;
        }

        /** Gets the flick scroll x velocity. */
        public float GetVelocityX()
        {
            return velocityX;
        }

        public void SetVelocityY(float velocityY)
        {
            this.velocityY = velocityY;
        }

        /** Gets the flick scroll y velocity. */
        public float GetVelocityY()
        {
            return velocityY;
        }

        /** For flick scroll, if true the widget can be scrolled slightly past its bounds and will animate back to its bounds when
         * scrolling is stopped. Default is true. */
        public void SetOverscroll(bool overscrollX, bool overscrollY)
        {
            this.overscrollX = overscrollX;
            this.overscrollY = overscrollY;
        }

        /** For flick scroll, sets the overscroll distance in pixels and the speed it returns to the widget's bounds in seconds.
         * Default is 50, 30, 200. */
        public void SetupOverscroll(float distance, float speedMin, float speedMax)
        {
            overscrollDistance = distance;
            overscrollSpeedMin = speedMin;
            overscrollSpeedMax = speedMax;
        }

        public float GetOverscrollDistance()
        {
            return overscrollDistance;
        }

        /** Forces enabling scrollbars (for non-flick scroll) and overscrolling (for flick scroll) in a direction, even if the contents
         * do not exceed the bounds in that direction. */
        public void SetForceScroll(bool x, bool y)
        {
            forceScrollX = x;
            forceScrollY = y;
        }

        public bool IsForceScrollX()
        {
            return forceScrollX;
        }

        public bool IsForceScrollY()
        {
            return forceScrollY;
        }

        /** For flick scroll, sets the amount of time in seconds that a fling will continue to scroll. Default is 1. */
        public void SetFlingTime(float flingTime)
        {
            this.flingTime = flingTime;
        }

        /** For flick scroll, prevents scrolling out of the widget's bounds. Default is true. */
        public void SetClamp(bool clamp)
        {
            this.clamp = clamp;
        }

        /** Set the position of the vertical and horizontal scroll bars. */
        public void SetScrollBarPositions(bool bottom, bool right)
        {
            hScrollOnBottom = bottom;
            vScrollOnRight = right;
            scrollbarsOnTop = !bottom;
        }

        /** When true the scrollbars don't reduce the scrollable size and fade out after some time of not being used. */
        public void SetFadeScrollBars(bool fadeScrollBars)
        {
            if (this.fadeScrollBars == fadeScrollBars) return;
            this.fadeScrollBars = fadeScrollBars;
            if (!fadeScrollBars) fadeAlpha = fadeAlphaSeconds;
            Invalidate();
        }

        public void SetupFadeScrollBars(float fadeAlphaSeconds, float fadeDelaySeconds)
        {
            this.fadeAlphaSeconds = fadeAlphaSeconds;
            this.fadeDelaySeconds = fadeDelaySeconds;
        }

        /** When false, the scroll bars don't respond to touch or mouse events. Default is true. */
        public void SetScrollBarTouch(bool scrollBarTouch)
        {
            this.scrollBarTouch = scrollBarTouch;
        }

        public void SetSmoothScrolling(bool smoothScrolling)
        {
            this.smoothScrolling = smoothScrolling;
        }

        /** When false (the default), the widget is clipped so it is not drawn under the scrollbars. When true, the widget is clipped
         * to the entire scroll pane bounds and the scrollbars are drawn on top of the widget. If {@link #setFadeScrollBars(bool)}
         * is true, the scroll bars are always drawn on top. */
        public void SetScrollbarsOnTop(bool scrollbarsOnTop)
        {
            this.scrollbarsOnTop = scrollbarsOnTop;
            Invalidate();
        }

        public bool GetVariableSizeKnobs()
        {
            return variableSizeKnobs;
        }

        /** If true, the scroll knobs are sized based on {@link #getMaxX()} or {@link #getMaxY()}. If false, the scroll knobs are sized
         * based on {@link Drawable#MinWidth} or {@link Drawable#MinHeight}. Default is true. */
        public void SetVariableSizeKnobs(bool variableSizeKnobs)
        {
            this.variableSizeKnobs = variableSizeKnobs;
        }

        /** When true (default) and flick scrolling begins, {@link #cancelTouchFocus()} is called. This causes any widgets inside the
         * scrollpane that have received touchDown to receive touchUp when flick scrolling begins. */
        public void SetCancelTouchFocus(bool cancelTouchFocus)
        {
            this.cancelTouchFocus = cancelTouchFocus;
        }

        #endregion

        public override void DrawDebug(Graphics graphics)
        {
            ApplyTransform(graphics, ComputeTransform());
            if (ScissorStack.PushScissors(scissorBounds))
            {
                DrawDebugElements(graphics, color.A);
                ScissorStack.PopScissors();
            }
            ResetTransform(graphics);
        }
    }

    public class ScrollPaneStyle
    {
        public IDrawable background, corner;
        /** Optional. */
        public IDrawable hScroll, hScrollKnob, hScrollKnobOver, hScrollKnobDown;
        /** Optional. */
        public IDrawable vScroll, vScrollKnob, vScrollKnobOver, vScrollKnobDown;

        public ScrollPaneStyle()
        {

        }

        public ScrollPaneStyle(IDrawable background, IDrawable hScroll, IDrawable hScrollKnob, IDrawable vScroll, IDrawable vScrollKnob)
        {
            this.background = background;
            this.hScroll = hScroll;
            this.hScrollKnob = hScrollKnob;
            this.vScroll = vScroll;
            this.vScrollKnob = vScrollKnob;
        }

        public ScrollPaneStyle(ScrollPaneStyle style)
        {
            this.background = style.background;
            this.hScroll = style.hScroll;
            this.hScrollKnob = style.hScrollKnob;
            this.vScroll = style.vScroll;
            this.vScrollKnob = style.vScrollKnob;
        }
    }
}
