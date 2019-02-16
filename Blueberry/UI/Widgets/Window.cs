using System;

namespace Blueberry.UI
{
    public class Window : Table
    {
        private static Vec2 tmpPosition = new Vec2();
        private static Vec2 tmpSize = new Vec2();
        private static readonly int MOVE = 1 << 5;

        public static float FADE_TIME = 0.4f;

        private WindowStyle style;
        private bool isMovable = true, isModal, isResizable;
        private int resizeBorder = 6;
        private bool keepWithinStage = true;
        private Label titleLabel;
        private Table titleTable;
        private bool drawTitleTable;

        protected int edge;
        protected bool dragging;

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
            private Window par;

            public TitleTable(Window par) : base()
            {
                this.par = par;
            }

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                if (par.drawTitleTable) base.Draw(graphics, parentAlpha);
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
            float parentWidth = stage.Width;
            float parentHeight = stage.Height;

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
                StageToLocalCoordinates(ref tmpPosition.X, ref tmpPosition.Y);
                tmpSize.Set(stage.Width, stage.Height);
                StageToLocalCoordinates(ref tmpSize.X, ref tmpSize.Y);
                DrawStageBackground(graphics, parentAlpha, GetX() + tmpPosition.X, GetY() + tmpPosition.Y, GetX() + tmpSize.X, GetY() + tmpSize.Y);
            }

            base.Draw(graphics, parentAlpha);

            // graphics.DrawRectangleBorder(x + resizeBorder, y + resizeBorder, width - resizeBorder * 2, height - resizeBorder * 2, Color.Red);
        }

        protected void DrawStageBackground(Graphics graphics, float parentAlpha, float x, float y, float width, float height)
        {
            var color = GetColor();
            style.stageBackground.Draw(graphics, x, y, width, height, new Col(color, color.A * parentAlpha));
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

        public override Element Hit(float x, float y, bool touchable = true)
        {
            if (!IsVisible()) return null;
            var hit = base.Hit(x, y, touchable);
            if (hit == null && isModal && (!touchable || GetTouchable() == Touchable.Enabled)) return this;
            float height = GetHeight();
            if (hit == null || hit == this) return hit;
            if (y >= 0 && y <= GetPadTop() && x >= 0 && x <= GetWidth())
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

        private class CaptureInputListener : InputListener<Window>
        {
            public CaptureInputListener(Window par) : base(par)
            {
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                par.ToFront();
                return false;
            }
        }

        private class Listener : InputListener<Window>
        {
            float startX, startY, lastX, lastY;

            public Listener(Window par) : base(par)
            {
            }

            private void UpdateEdge(float x, float y)
            {
                float border = par.resizeBorder;// / 2f;
                float width = par.GetWidth(), height = par.GetHeight();
                float padTop = par.GetPadTop(), padLeft = par.GetPadLeft(), padBottom = par.GetPadBottom(), padRight = par.GetPadRight();
                float left = padLeft, right = width - padRight, bottom = height - padBottom;
                par.edge = 0;
                if (par.isResizable && x > 0 && x < width && y > 0 && y < height)// && x >= left - border && x <= right + border && y >= bottom + border/*bottom - border*/)
                {
                    if (x < border)
                        par.edge |= AlignInternal.left;
                    if (x > width - border)
                        par.edge |= AlignInternal.right;
                    if (y < border)
                        par.edge |= AlignInternal.top;
                    if (y > height - border)
                        par.edge |= AlignInternal.bottom;
                    /*if (par.edge != 0) border += 25;
                    if (x < border)
                        par.edge |= AlignInternal.left;
                    if (x > width - border)
                        par.edge |= AlignInternal.right;
                    if (y < border)
                        par.edge |= AlignInternal.top;
                    if (y > height - border)
                        par.edge |= AlignInternal.bottom;*/
                }
                if (par.isMovable && par.edge == 0 && y <= /*height*/padTop && y >= /*height - padTop*/ 0 && x >= left && x <= right) par.edge = MOVE;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (button == 0)
                {
                    UpdateEdge(x, y);
                    par.dragging = par.edge != 0;
                    startX = x;
                    startY = y;
                    lastX = x - par.GetWidth();
                    lastY = y - par.GetHeight();
                }
                return par.edge != 0 || par.isModal;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                par.dragging = false;
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                //if (!par.dragging) return;

                float width = par.GetWidth(), height = par.GetHeight();
                float windowX = par.GetX(), windowY = par.GetY();

                float minWidth = Math.Max(par.MinWidth, par.titleTable.MinWidth), maxWidth = par.MaxWidth;
                float minHeight = par.MinHeight, maxHeight = par.MaxHeight;
                var stage = par.GetStage();
                float parentWidth = stage.Width;
                float parentHeight = stage.Height;
                bool clampPosition = par.keepWithinStage && par.GetParent() == stage.Root;

                if ((par.edge & MOVE) != 0)
                {
                    float amountX = x - startX, amountY = y - startY;
                    windowX += amountX;
                    windowY += amountY;
                }
                if ((par.edge & AlignInternal.left) != 0)
                {
                    float amountX = x - startX;
                    if (width - amountX < minWidth)
                        amountX = -(minWidth - width);
                    if (clampPosition && windowX + amountX < 0)
                        amountX = -windowX;
                    width -= amountX;
                    windowX += amountX;
                }
                if ((par.edge & AlignInternal.top) != 0)
                {
                    float amountY = y - startY;
                    if (height - amountY < minHeight)
                        amountY = -(minHeight - height);
                    if (clampPosition && windowY + amountY < 0)
                        amountY = -windowY;
                    height -= amountY;
                    windowY += amountY;
                }
                if ((par.edge & AlignInternal.right) != 0)
                {
                    float amountX = x - lastX - width;
                    if (width + amountX < minWidth)
                        amountX = minWidth - width;
                    if (clampPosition && windowX + width + amountX > parentWidth)
                        amountX = parentWidth - windowX - width;
                    width += amountX;
                }
                if ((par.edge & AlignInternal.bottom) != 0)
                {
                    float amountY = y - lastY - height;
                    if (height + amountY < minHeight)
                        amountY = minHeight - height;
                    if (clampPosition && windowY + height + amountY > parentHeight)
                        amountY = parentHeight - windowY - height;
                    height += amountY;
                }

                par.SetBounds((float)Math.Round(windowX), (float)Math.Round(windowY), (float)Math.Round(width), (float)Math.Round(height));
                Render.Request();
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                if (par.dragging || !par.isResizable)
                    return false;
                /*if (x < par.resizeBorder)
                {
                    if (y < par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else if (y > par.height - par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else
                        Mouse.SetCursor(MouseCursor.SizeWE);
                }
                else if (x > par.width - par.resizeBorder)
                {
                    if (y < par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else if (y > par.height - par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else
                        Mouse.SetCursor(MouseCursor.SizeWE);
                }
                else if (y < par.resizeBorder)
                {
                    if (x < par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else if (x > par.width - par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else
                        Mouse.SetCursor(MouseCursor.SizeNS);
                }
                else if (y > par.height - par.resizeBorder)
                {
                    if (x < par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNESW);
                    else if (x > par.width - par.resizeBorder)
                        Mouse.SetCursor(MouseCursor.SizeNWSE);
                    else
                        Mouse.SetCursor(MouseCursor.SizeNS);
                }
                else
                {
                    Mouse.SetCursor(MouseCursor.Arrow);
                }*/
                //UpdateEdge(x, y);
                return par.isModal;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (par.dragging)
                    return;
                //Mouse.SetCursor(MouseCursor.Arrow);
            }

            public override bool Scrolled(InputEvent ev, float x, float y, float amountX, float amountY)
            {
                return par.isModal;
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                return par.isModal;
            }

            public override bool KeyUp(InputEvent ev, int keycode)
            {
                return par.isModal;
            }

            public override bool KeyTyped(InputEvent ev, int keycode, char character)
            {
                return par.isModal;
            }
        }

        private class CloseChangeListener : ChangeListener
        {
            private Window par;

            public CloseChangeListener(Window par)
            {
                this.par = par;
            }

            public override void Changed(ChangeEvent ev, Element element)
            {
                par.Close();
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
            if (parent != null) SetPosition((parent.Width - GetWidth()) / 2, (parent.Height - GetHeight()) / 2);
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
            AddAction(Actions.Sequence(Actions.FadeOut(time, Interpolation.fade),
                new WindowFadeAction(this)
                {
                    previousTouchable = previousTouchable
                }
            ));

        }

        private class WindowFadeAction : Action
        {
            private Window par;

            public WindowFadeAction(Window par)
            {
                this.par = par;
            }

            public Touchable previousTouchable;

            public override bool Update(float delta)
            {
                par.SetTouchable(previousTouchable);
                par.Remove();
                par.color.A = 1;
                par.fadeOutActionRunning = false;
                return true;
            }
        }

        /** @return this window for the purpose of chaining methods eg. stage.addActor(new MyWindow(stage).fadeIn(0.3f)); */
        public Window FadeIn(float time)
        {
            SetColor(new Col(1.0f, 1.0f, 1.0f, 0));
            AddAction(Actions.FadeIn(time, Interpolation.fade));
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
            throw new NotImplementedException();
            /*var titleLabel = GetTitleLabel();
            var titleTable = GetTitleTable();

            var closeButton = new ImageButton(skin.Get<ImageButtonStyle>("close-window"));

            titleTable.Add(closeButton).PadRight(-GetPadRight() + 0.7f).Size(closeButton.GetImage().PreferredWidth, closeButton.GetImage().PreferredHeight);

            closeButton.AddListener(new CloseChangeListener(this));
            closeButton.AddListener(new CloseClickListener());

            if (titleLabel.GetAlign() == AlignInternal.center && titleTable.GetElements().Count == 2)
                titleTable.GetCell(titleLabel).PadLeft(closeButton.GetWidth() * 2);*/
        }
    }

    public class WindowStyle
    {
        /** Optional. */
        public IDrawable background;
        public IFont titleFont;
        /** Optional. */
        public Col titleFontColor = new Col(1.0f, 1.0f, 1.0f, 1.0f);
        /** Optional. */
        public IDrawable stageBackground;

        public WindowStyle()
        {
        }

        public WindowStyle(IFont titleFont, Col titleFontColor, IDrawable background)
        {
            this.background = background;
            this.titleFont = titleFont;
            this.titleFontColor = titleFontColor;
        }

        public WindowStyle(WindowStyle style)
        {
            this.background = style.background;
            this.titleFont = style.titleFont;
            this.titleFontColor = new Col(style.titleFontColor);
        }
    }
}
