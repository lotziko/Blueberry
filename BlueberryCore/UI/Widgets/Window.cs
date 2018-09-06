using BlueberryCore.UI.Actions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace BlueberryCore.UI
{
    public class Window : Table
    {
        private static Vector2 tmpPosition = new Vector2();
        private static Vector2 tmpSize = new Vector2();
        private static readonly int MOVE = 1 << 5;

        private WindowStyle style;
        bool isMovable = true, isModal, isResizable;
        int resizeBorder = 6;
        bool keepWithinStage = true;
        Label titleLabel;
        Table titleTable;
        bool drawTitleTable;

        protected int edge;
        protected bool dragging;






        public static float FADE_TIME = 0.4f;

        private bool centerOnAdd;
        private bool keepWithinParent = false;

        private bool fadeOutActionRunning;


        public Window(string title, Skin skin, string stylename = "default") : this(title, skin.Get<WindowStyle>(stylename)) { }

        public Window(string title, WindowStyle style)
        {
            if (title == null) throw new ArgumentNullException("title cannot be null.");
            SetTouchable(Touchable.Enabled);
            SetClip(true);

            titleLabel = new Label(title, new LabelStyle(style.titleFont, style.titleFontColor));
            titleLabel.SetAlign(UI.Align.left);
            titleLabel.SetEllipsis(true);

            titleTable = new TitleTable(this);
            titleTable.Add(titleLabel).ExpandX().FillX().MinWidth(30);
            
            AddElement(titleTable);

            SetStyle(style);
            SetWidth(150);
            SetHeight(150);

            AddCaptureListener(new CaptureInputListener(this));
            AddListener(new Listener(this));
        }

        #region Title class

        private class TitleTable : Table
        {
            private Window w;

            public TitleTable(Window w) : base()
            {
                this.w = w;
            }

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                if (w.drawTitleTable) base.Draw(graphics, parentAlpha);
            }
        }

        #endregion

        public void SetStyle(WindowStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            SetBackground(style.background);
            titleLabel.SetStyle(new LabelStyle(style.titleFont, style.titleFontColor));
            InvalidateHierarchy();
        }

        /** Returns the window's style. Modifying the returned style may not have an effect until {@link #setStyle(WindowStyle)} is
	    * called. */
        public virtual WindowStyle GetStyle()
        {
            return style;
        }

        public void KeepWithinStage()
        {
            float parentWidth = stage.GetWidth();
            float parentHeight = stage.GetHeight();

            if (GetX() < 0) SetX(0);
            if (GetRight() > parentWidth) SetX(parentWidth - GetWidth());
            if (GetY() < 0) SetY(0);
            if (GetBottom() > parentHeight) SetY(parentHeight - GetHeight());
        }


        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();

            if (keepWithinParent && GetParent() != null)
            {
                float parentWidth = GetParent().GetWidth();
                float parentHeight = GetParent().GetHeight();
                if (GetX() < 0) SetX(0);
                if (GetRight() > parentWidth) SetX(parentWidth - GetWidth());
                if (GetY() < 0) SetY(0);
                if (GetBottom() > parentHeight) SetY(parentHeight - GetHeight());
            }

            if (stage.GetKeyboardFocus() == null) stage.SetKeyboardFocus(this);

            KeepWithinStage();

            if (style.stageBackground != null)
            {
                tmpPosition.Set(0, 0);
                tmpPosition = StageToLocalCoordinates(tmpPosition);
                tmpSize.Set(stage.GetWidth(), stage.GetHeight());
                tmpSize = StageToLocalCoordinates(tmpSize);
                DrawStageBackground(graphics, parentAlpha, GetX() + tmpPosition.X, GetY() + tmpPosition.Y, GetX() + tmpSize.X, GetY() + tmpSize.Y);
            }

            base.Draw(graphics, parentAlpha);

           // graphics.DrawRectangleBorder(x + resizeBorder, y + resizeBorder, width - resizeBorder * 2, height - resizeBorder * 2, Color.Red);
        }

        protected void DrawStageBackground(Graphics graphics, float parentAlpha, float x, float y, float width, float height)
        {
            var color = GetColor();
            style.stageBackground.Draw(graphics, x, y, width, height, new Color(color.R, color.G, color.B, (int)(color.A * parentAlpha)));
        }

        protected override void DrawBackground(Graphics graphics, float parentAlpha, float x, float y)
        {
            base.DrawBackground(graphics, parentAlpha, x, y);
            graphics.Flush();

            // Manually draw the title table before clipping is done.
            titleTable.color.A = GetColor().A;
            float padTop = GetPadTop(), padLeft = GetPadLeft();
            titleTable.SetSize(GetWidth() - padLeft - GetPadRight(), padTop);
            titleTable.SetPosition(padLeft, /*GetHeight() - padTop*/0);
            drawTitleTable = true;
            titleTable.Draw(graphics, parentAlpha);
            drawTitleTable = false; // Avoid drawing the title table again in drawChildren.
        }

        public override Element Hit(Vector2 point, bool touchable = true)
        {
            if (!IsVisible()) return null;
            var hit = base.Hit(point, touchable);
            if (hit == null && isModal && (!touchable || GetTouchable() == Touchable.Enabled)) return this;
            float height = GetHeight();
            if (hit == null || hit == this) return hit;
            if (point.Y >= 0 && point.Y <= GetPadTop() && point.X >= 0 && point.X <= GetWidth())
            {
                // Hit the title bar, don't use the hit child if it is in the Window's table.
                var current = hit;
                while (current.GetParent() != this)
                    current = current.GetParent();
                if (GetCell(current) != null) return this;
            }
            return hit;
        }

        public bool IsMovable()
        {
            return isMovable;
        }

        public void SetMovable(bool isMovable)
        {
            this.isMovable = isMovable;
        }

        public bool IsModal()
        {
            return isModal;
        }

        public void SetModal(bool isModal)
        {
            this.isModal = isModal;
        }

        public void SetKeepWithinStage(bool keepWithinStage)
        {
            this.keepWithinStage = keepWithinStage;
        }

        public bool IsResizable()
        {
            return isResizable;
        }

        public void SetResizable(bool isResizable)
        {
            this.isResizable = isResizable;
        }

        public void SetResizeBorder(int resizeBorder)
        {
            this.resizeBorder = resizeBorder;
        }

        public bool IsDragging()
        {
            return dragging;
        }

        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                return Math.Max(base.PreferredWidth, titleTable.PreferredWidth + GetPadLeft() + GetPadRight());
            }
        }

        #endregion

        public Table GetTitleTable()
        {
            return titleTable;
        }

        public Label GetTitleLabel()
        {
            return titleLabel;
        }

        #region Listeners

        private class CaptureInputListener : InputListener
        {
            private Window w;

            public CaptureInputListener(Window w)
            {
                this.w = w;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                w.ToFront();
                return false;
            }
        }

        private class Listener : InputListener
        {
            private Window w;

            public Listener(Window w)
            {
                this.w = w;
            }

            float startX, startY, lastX, lastY;

            private void UpdateEdge(float x, float y)
            {
                float border = w.resizeBorder;// / 2f;
                float width = w.GetWidth(), height = w.GetHeight();
                float padTop = w.GetPadTop(), padLeft = w.GetPadLeft(), padBottom = w.GetPadBottom(), padRight = w.GetPadRight();
                float left = padLeft, right = width - padRight, bottom = height - padBottom;
                w.edge = 0;
                if (w.isResizable && x > 0 && x < width && y > 0 && y < height)// && x >= left - border && x <= right + border && y >= bottom + border/*bottom - border*/)
                {
                    if (x < border)
                        w.edge |= AlignInternal.left;
                    if (x > width - border)
                        w.edge |= AlignInternal.right;
                    if (y < border)
                        w.edge |= AlignInternal.top;
                    if (y > height - border)
                        w.edge |= AlignInternal.bottom;
                    /*if (w.edge != 0) border += 25;
                    if (x < border)
                        w.edge |= AlignInternal.left;
                    if (x > width - border)
                        w.edge |= AlignInternal.right;
                    if (y < border)
                        w.edge |= AlignInternal.top;
                    if (y > height - border)
                        w.edge |= AlignInternal.bottom;*/
                }
                if (w.isMovable && w.edge == 0 && y <= /*height*/padTop && y >= /*height - padTop*/ 0 && x >= left && x <= right) w.edge = MOVE;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (button == 0)
                {
                    UpdateEdge(x, y);
                    w.dragging = w.edge != 0;
                    startX = x;
                    startY = y;
                    lastX = x - w.GetWidth();
                    lastY = y - w.GetHeight();
                }
                return w.edge != 0 || w.isModal;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                w.dragging = false;
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                //if (!w.dragging) return;
                
                float width = w.GetWidth(), height = w.GetHeight();
                float windowX = w.GetX(), windowY = w.GetY();

                float minWidth = w.MinWidth, maxWidth = w.MaxWidth;
                float minHeight = w.MinHeight, maxHeight = w.MaxHeight;
                var stage = w.GetStage();
                float parentWidth = stage.GetWidth();
                float parentHeight = stage.GetHeight();
                bool clampPosition = w.keepWithinStage && w.GetParent() == stage.GetRoot();

                if ((w.edge & MOVE) != 0)
                {
                    float amountX = x - startX, amountY = y - startY;
                    windowX += amountX;
                    windowY += amountY;
                }
                if ((w.edge & AlignInternal.left) != 0)
                {
                    float amountX = x - startX;
                    if (width - amountX < minWidth)
                        amountX = -(minWidth - width);
                    if (clampPosition && windowX + amountX < 0)
                        amountX = -windowX;
                    width -= amountX;
                    windowX += amountX;
                }
                if ((w.edge & AlignInternal.top) != 0)
                {
                    float amountY = y - startY;
                    if (height - amountY < minHeight)
                        amountY = -(minHeight - height);
                    if (clampPosition && windowY + amountY < 0)
                        amountY = -windowY;
                    height -= amountY;
                    windowY += amountY;
                }
                if ((w.edge & AlignInternal.right) != 0)
                {
                    float amountX = x - lastX - width;
                    if (width + amountX < minWidth)
                        amountX = minWidth - width;
                    if (clampPosition && windowX + width + amountX > parentWidth)
                        amountX = parentWidth - windowX - width;
                    width += amountX;
                }
                if ((w.edge & AlignInternal.bottom) != 0)
                {
                    float amountY = y - lastY - height;
                    if (height + amountY < minHeight)
                        amountY = minHeight - height;
                    if (clampPosition && windowY + height + amountY > parentHeight)
                        amountY = parentHeight - windowY - height;
                    height += amountY;
                }

                w.SetBounds((float)Math.Round(windowX), (float)Math.Round(windowY), (float)Math.Round(width), (float)Math.Round(height));
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                if (w.dragging || !w.isResizable)
                    return false;
                if (x < w.resizeBorder)
                {
                    if (y < w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else if (y > w.height - w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else
                        Mouse.SetCursor(MouseCursor.SizeWE);
                }
                else if (x > w.width - w.resizeBorder)
                {
                    if (y < w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else if (y > w.height - w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else
                        Mouse.SetCursor(MouseCursor.SizeWE);
                }
                else if (y < w.resizeBorder)
                {
                    if (x < w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else if (x > w.width - w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else
                        Mouse.SetCursor(MouseCursor.SizeNS);
                }
                else if (y > w.height - w.resizeBorder)
                {
                    if (x < w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else if (x > w.width - w.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else
                        Mouse.SetCursor(MouseCursor.SizeNS);
                }
                else
                {
                    Mouse.SetCursor(MouseCursor.Arrow);
                }
                //UpdateEdge(x, y);
                return w.isModal;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (w.dragging)
                    return;
                Mouse.SetCursor(MouseCursor.Arrow);
            }

            public override bool Scrolled(InputEvent ev, float x, float y, int amount)
            {
                return w.isModal;
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                return w.isModal;
            }

            public override bool KeyUp(InputEvent ev, int keycode)
            {
                return w.isModal;
            }

            public override bool KeyTyped(InputEvent ev, int keycode, char character)
            {
                return w.isModal;
            }
        }

        private class CloseChangeListener : ChangeListener
        {
            private Window w;

            public CloseChangeListener(Window w)
            {
                this.w = w;
            }

            public override void Changed(ChangeEvent ev, Element element)
            {
                w.Close();
            }
        }

        //is used to avoid window from being dragged

        private class CloseClickListener : ClickListener
        {
            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                ev.Cancel();
				return true;
            }
        }

        #endregion






        /**
	     * Centers this window, if it has parent it will be done instantly, if it does not have parent it will be centered when it will
	     * be added to stage
	     * @return true when window was centered, false when window will be centered when added to stage
	     */
        public bool CenterWindow()
        {
            if (parent == null)
            {
                centerOnAdd = true;
                return false;
            }
            else
            {
                MoveToCenter();
                return true;
            }
        }

        /**
	     * @param centerOnAdd if true window position will be centered on screen after adding to stage
	     * @see #centerWindow()
	     */
        public void SetCenterOnAdd(bool centerOnAdd)
        {
            this.centerOnAdd = centerOnAdd;
        }

        public override void SetStage(Stage stage)
        {
            base.SetStage(stage);

            if (stage != null)
            {
                stage.SetKeyboardFocus(this); //issue #10, newly created window does not acquire keyboard focus

                if (centerOnAdd)
                {
                    centerOnAdd = false;
                    MoveToCenter();
                }
            }
        }

        private void MoveToCenter()
        {
            var parent = GetStage();
            if (parent != null) SetPosition((parent.GetWidth() - GetWidth()) / 2, (parent.GetHeight() - GetHeight()) / 2);
        }

        #region FadeOut Action

        /**
	     * Fade outs this window, when fade out animation is completed, window is removed from Stage. Calling this for the
	     * second time won't have any effect if previous animation is still running.
	     */
        public void FadeOut(float time)
        {
            if (fadeOutActionRunning) return;
            fadeOutActionRunning = true;
            Touchable previousTouchable = GetTouchable();
            SetTouchable(Touchable.Disabled);
            Stage stage = GetStage();
            if (stage != null && stage.GetKeyboardFocus() != null && stage.GetKeyboardFocus().IsDescendantOf(this))
            {
                FocusManager.ResetFocus(stage);
            }
            AddAction(SAction.Sequence(SAction.FadeOut(time, Interpolation.fade),
                new WindowFadeAction(this)
                {
                    previousTouchable = previousTouchable
                }
            ));

        }

        private class WindowFadeAction : Action
        {
            private Window w;

            public WindowFadeAction(Window w)
            {
                this.w = w;
            }

            public Touchable previousTouchable;

            public override bool Update(float delta)
            {
                w.SetTouchable(previousTouchable);
                w.Remove();
                w.color.A = 255;
                w.fadeOutActionRunning = false;
                return true;
            }
        }

        /** @return this window for the purpose of chaining methods eg. stage.addActor(new MyWindow(stage).fadeIn(0.3f)); */
        public Window FadeIn(float time)
        {
            SetColor(new Color(255, 255, 255, 0));
            AddAction(SAction.FadeIn(time, Interpolation.fade));
            return this;
        }

        /** Fade outs this window, when fade out animation is completed, window is removed from Stage */
        public void FadeOut()
        {
            FadeOut(FADE_TIME);
        }

        /** @return this window for the purpose of chaining methods eg. stage.addActor(new MyWindow(stage).fadeIn()); */
        public Window FadeIn()
        {
            return FadeIn(FADE_TIME);
        }

        /**
	     * Called by window when close button was pressed (added using {@link #addCloseButton()})
	     * or escape key was pressed (for close on escape {@link #closeOnEscape()} have to be called).
	     * Default close behaviour is to fade out window, this can be changed by overriding this function.
	     */
        protected void Close()
        {
            FadeOut();
        }

        #endregion

        public bool IsKeepWithinParent()
        {
            return keepWithinParent;
        }

        public void SetKeepWithinParent(bool keepWithinParent)
        {
            this.keepWithinParent = keepWithinParent;
        }

        /**
	     * Adds close button to window, next to window title. After pressing that button, {@link #close()} is called. If nothing
	     * else was added to title table, and current title alignment is center then the title will be automatically centered.
	     */
        public void AddCloseButton(Skin skin)
        {
            var titleLabel = GetTitleLabel();
            var titleTable = GetTitleTable();
            
            var closeButton = new ImageButton(skin.Get<ImageButtonStyle>("close-window"));

            titleTable.Add(closeButton).PadRight(-GetPadRight() + 0.7f).Size(closeButton.GetImage().PreferredWidth, closeButton.GetImage().PreferredHeight);

            closeButton.AddListener(new CloseChangeListener(this));
            closeButton.AddListener(new CloseClickListener());

            if (titleLabel.GetAlign() == AlignInternal.center && titleTable.GetElements().Count == 2)
                titleTable.GetCell(titleLabel).PadLeft(closeButton.GetWidth() * 2);
        }

    }

    public class WindowStyle
    {
        /** Optional. */
        public IDrawable background;
        public BitmapFont titleFont;
        /** Optional. */
        public Color titleFontColor = new Color(1, 1, 1, 1);
        /** Optional. */
        public IDrawable stageBackground;

        public WindowStyle()
        {
        }

        public WindowStyle(BitmapFont titleFont, Color titleFontColor, IDrawable background)
        {
            this.background = background;
            this.titleFont = titleFont;
            this.titleFontColor = titleFontColor;
        }

        public WindowStyle(WindowStyle style)
        {
            this.background = style.background;
            this.titleFont = style.titleFont;
            this.titleFontColor = new Color(style.titleFontColor, style.titleFontColor.A);
        }
    }
}
