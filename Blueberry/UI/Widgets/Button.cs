using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public partial class Button : Table, IFocusable, IDisablable
    {
        public event System.Action OnClicked;
        internal ButtonGroup buttonGroup;
        internal bool isChecked, isDisabled, focusBorderEnabled = true, drawBorder;
        private ButtonStyle style;
        private ClickListener listener;
        private bool programmaticChangeEvents = true;

        private IDrawable tempBackground;

        protected float BackgroundMinWidth
        {
            get
            {
                var width = base.PreferredWidth;
                if (style.up != null)
                    width = Math.Max(width, style.up.MinWidth);
                if (style.down != null)
                    width = Math.Max(width, style.down.MinWidth);
                if (style.checkked != null)
                    width = Math.Max(width, style.checkked.MinWidth);
                return width;
            }
        }

        protected float BackgroundMinHeight
        {
            get
            {
                var height = base.PreferredHeight;
                if (style.up != null)
                    height = Math.Max(height, style.up.MinHeight);
                if (style.down != null)
                    height = Math.Max(height, style.down.MinHeight);
                if (style.checkked != null)
                    height = Math.Max(height, style.checkked.MinHeight);
                return height;
            }
        }

        public Button(ButtonStyle style)
        {
            touchable = Touchable.Enabled;
            listeners.Add(listener = new ButtonListener(this));

            SetStyle(style);
        }

        public Button(Skin skin, string stylename = "default") : this(skin.Get<ButtonStyle>(stylename)) { }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();

            base.Draw(graphics, parentAlpha);

            if (focusBorderEnabled && drawBorder && style.focusedBorder != null)
            {
                DrawFocusBorder(graphics, parentAlpha);
            }
        }

        protected virtual void DrawFocusBorder(Graphics graphics, float parentAlpha)
        {
            style.focusedBorder.Draw(graphics, GetX(), GetY(), GetWidth(), GetHeight(), new Col(color, color.A * parentAlpha));
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            RefreshBackground();
        }

        protected virtual void RefreshBackground()
        {
            bool isOver = IsOver();
            bool isPressed = IsPressed();

            if (isPressed && isOver && !isDisabled)
                tempBackground = style.down ?? style.up;
            else if (isDisabled && style.disabled != null)
                tempBackground = style.disabled;
            else if (isChecked && style.checkked != null)
            {
                if (isOver && style.checkedOver != null)
                    tempBackground = style.checkedOver;
                else
                    tempBackground = style.checkked;
            }
            else if (isOver && style.over != null)
                tempBackground = style.over;
            else
                tempBackground = style.up;

            if (tempBackground != background)
            {
                SetBackground(tempBackground);
                Render.Request();
            }
        }

        public virtual void SetStyle(ButtonStyle style)
        {
            this.style = style;
            InvalidateHierarchy();
        }

        public virtual ButtonStyle GetStyle() => style;

        public void SetChecked(bool isChecked)
        {
            SetChecked(isChecked, programmaticChangeEvents);
        }

        public void SetChecked(bool isChecked, bool fireEvent)
        {
            if (this.isChecked == isChecked) return;
            this.isChecked = isChecked;

            if (fireEvent)
            {
                var changeEv = Pool<ChangeListener.ChangeEvent>.Obtain();
                if (Fire(changeEv)) this.isChecked = !isChecked;
                Pool<ChangeListener.ChangeEvent>.Free(changeEv);
            }
        }

        /// <summary>
        /// Toggles the checked state. This method changes the checked state, which fires a {@link ChangeEvent} (if programmatic change events are enabled), so can be used to simulate a button click.
        /// </summary>
        public void Toggle()
        {
            SetChecked(!isChecked);
        }

        public virtual bool IsOver() => listener.IsOver();

        public virtual bool IsPressed() => listener.IsVisualPressed();

        public virtual bool IsChecked() => isChecked;

        public bool IsFocusBorderEnabled()
        {
            return focusBorderEnabled;
        }

        public void SetFocusBorderEnabled(bool focusBorderEnabled)
        {
            this.focusBorderEnabled = focusBorderEnabled;
        }

        public void FocusGained()
        {
            drawBorder = true;
        }

        public void FocusLost()
        {
            drawBorder = false;
        }

        public void SetDisabled(bool isDisabled)
        {
            this.isDisabled = isDisabled;
        }

        public bool IsDisabled()
        {
            return isDisabled;
        }

        #region ILayout

        public override float PreferredWidth
        {
            get { return BackgroundMinWidth; }
        }

        public override float PreferredHeight
        {
            get { return BackgroundMinHeight; }
        }

        public override float MinWidth
        {
            get { return PreferredWidth; }
        }

        public override float MinHeight
        {
            get { return PreferredHeight; }
        }

        #endregion

        #region Listener

        private class ButtonListener : ClickListener
        {
            private readonly Button _b;

            public ButtonListener(Button b)
            {
                _b = b;
            }

            public override void Clicked(InputEvent ev, float x, float y)
            {
                if (_b.IsDisabled())
                    return;
                _b.SetChecked(!_b.isChecked, true);
                _b.OnClicked?.Invoke();
                FocusManager.SwitchFocus(_b.GetStage(), _b);
            }
        }

        #endregion
    }
}
