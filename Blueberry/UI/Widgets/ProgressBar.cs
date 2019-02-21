using Blueberry.DataTools;
using System;
using static Blueberry.UI.ChangeListener;

namespace Blueberry.UI
{
    public class ProgressBar : Element, IDisablable
    {
        protected bool isDisabled;
        protected ProgressBarStyle style;
        private float Min, Max, stepSize;
        private float value, animateFromValue;
        protected float position;
        protected readonly bool vertical;
        private float animateDuration, animateTime;
        private Interpolation animateInterpolation = Interpolation.linear;
        private Interpolation visualInterpolation = Interpolation.linear;
        private bool Round = true;

        public event System.Action OnChange;

        #region Binding

        private IDataBinding binding;
        private object bindingData;

        public IDataBinding DataBinding
        {
            get => binding;
            set
            {
                if (binding != null)
                    binding.OnChange -= Binding_OnChange;

                binding = value;
                binding.OnChange += Binding_OnChange;
            }
        }

        private void Binding_OnChange(object obj)
        {
            if (obj is IDataBinding b && b.Value is float)
                SetValue((float)b.Value);
        }

        #endregion

        public ProgressBar(float Min, float Max, float stepSize, bool vertical, Skin skin, string stylename = "default") : this(Min, Max, stepSize, vertical, skin.Get<ProgressBarStyle>(stylename)) { }

        public ProgressBar(float Min, float Max, float stepSize, bool vertical, ProgressBarStyle style)
        {
            if (Min > Max) throw new ArgumentException("Max must be > Min. Min,Max: " + Min + ", " + Max);
            if (stepSize <= 0) throw new ArgumentException("stepSize must be > 0: " + stepSize);
            SetStyle(style);
            this.Min = Min;
            this.Max = Max;
            this.stepSize = stepSize;
            this.vertical = vertical;
            value = Min;
            SetSize(PreferredWidth, PreferredHeight);
        }

        public virtual void SetStyle(ProgressBarStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            InvalidateHierarchy();
        }

        public ProgressBarStyle GetStyle() => style;

        public override void Update(float delta)
        {
            base.Update(delta);
            if (animateTime > 0)
            {
                //var delta = Core.time.ElapsedGameTime.TotalSeconds;
                animateTime -= delta;//(float) delta * 100;
                Render.Request();
            }
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            ProgressBarStyle style = this.style;
            bool disabled = isDisabled;
            IDrawable knob = GetKnobDrawable();
            IDrawable bg = (disabled && style.disabledBackground != null) ? style.disabledBackground : style.background;
            IDrawable knobBefore = (disabled && style.disabledKnobBefore != null) ? style.disabledKnobBefore : style.knobBefore;
            IDrawable knobAfter = (disabled && style.disabledKnobAfter != null) ? style.disabledKnobAfter : style.knobAfter;

            var color = this.color;
            float x = GetX();
            float y = GetY();
            float width = GetWidth();
            float height = GetHeight();
            float knobHeight = knob == null ? 0 : knob.MinHeight;
            float knobWidth = knob == null ? 0 : knob.MinWidth;
            float percent = GetVisualPercent();


            if (vertical)
            {
                float positionHeight = height;

                float bgTopHeight = 0;
                if (bg != null)
                {
                    if (Round)
                        bg.Draw(graphics, MathF.Round(x + (width - bg.MinWidth) * 0.5f), y, MathF.Round(bg.MinWidth), height, color);
                    else
                        bg.Draw(graphics, x + width - bg.MinWidth * 0.5f, y, bg.MinWidth, height, color);
                    bgTopHeight = bg.TopHeight;
                    positionHeight -= bgTopHeight + bg.BottomHeight;
                }

                float knobHeightHalf = 0;
                if (knob == null)
                {
                    knobHeightHalf = knobBefore == null ? 0 : knobBefore.MinHeight * 0.5f;
                    position = (positionHeight - knobHeightHalf) * percent;
                    position = Math.Min(positionHeight - knobHeightHalf, position);
                }
                else
                {
                    knobHeightHalf = knobHeight * 0.5f;
                    position = (positionHeight - knobHeight) * percent;
                    position = Math.Min(positionHeight - knobHeight, position) + bg.BottomHeight;
                }
                position = Math.Max(0, position);

                if (knobBefore != null)
                {
                    if (Round)
                    {
                        knobBefore.Draw(graphics, MathF.Round(x + (width - knobBefore.MinWidth) * 0.5f), MathF.Round(y + bgTopHeight), MathF.Round(knobBefore.MinWidth), MathF.Round(position + knobHeightHalf), color);
                    }
                    else
                    {
                        knobBefore.Draw(graphics, x + (width - knobBefore.MinWidth) * 0.5f, y + bgTopHeight, knobBefore.MinWidth, position + knobHeightHalf, color);
                    }
                }
                if (knobAfter != null)
                {
                    if (Round)
                    {
                        knobAfter.Draw(graphics, MathF.Round(x + (width - knobAfter.MinWidth) * 0.5f), MathF.Round(y + position + knobHeightHalf), MathF.Round(knobAfter.MinWidth), MathF.Round(height - position - knobHeightHalf), color);
                    }
                    else
                    {
                        knobAfter.Draw(graphics, x + (width - knobAfter.MinWidth) * 0.5f, y + position + knobHeightHalf, knobAfter.MinWidth, height - position - knobHeightHalf, color);
                    }
                }
                if (knob != null)
                {
                    if (Round)
                    {
                        knob.Draw(graphics, MathF.Round(x + (width - knobWidth) * 0.5f), MathF.Round(y + position), MathF.Round(knobWidth), MathF.Round(knobHeight), color);
                    }
                    else
                        knob.Draw(graphics, x + (width - knobWidth) * 0.5f, y + position, knobWidth, knobHeight, color);
                }
            }
            else
            {
                float positionWidth = width;

                float bgLeftWidth = 0;
                if (bg != null)
                {
                    if (Round)
                        bg.Draw(graphics, x, MathF.Round(y + (height - bg.MinHeight) * 0.5f), width, MathF.Round(bg.MinHeight), color);
                    else
                        bg.Draw(graphics, x, y + (height - bg.MinHeight) * 0.5f, width, bg.MinHeight, color);
                    bgLeftWidth = bg.LeftWidth;
                    positionWidth -= bgLeftWidth + bg.RightWidth;
                }

                float knobWidthHalf = 0;
                if (knob == null)
                {
                    knobWidthHalf = knobBefore == null ? 0 : knobBefore.MinWidth * 0.5f;
                    position = (positionWidth - knobWidthHalf) * percent;
                    position = Math.Min(positionWidth - knobWidthHalf, position);
                }
                else
                {
                    knobWidthHalf = knobWidth * 0.5f;
                    position = (positionWidth - knobWidth) * percent;
                    position = Math.Min(positionWidth - knobWidth, position) + bgLeftWidth;
                }
                position = Math.Max(0, position);

                if (knobBefore != null)
                {
                    if (Round)
                    {
                        knobBefore.Draw(graphics, MathF.Round(x + bgLeftWidth), MathF.Round(y + (height - knobBefore.MinHeight) * 0.5f), MathF.Round(position + knobWidthHalf), MathF.Round(knobBefore.MinHeight), color);
                    }
                    else
                    {
                        knobBefore.Draw(graphics, x + bgLeftWidth, y + (height - knobBefore.MinHeight) * 0.5f, position + knobWidthHalf, knobBefore.MinHeight, color);
                    }
                }
                if (knobAfter != null)
                {
                    if (Round)
                    {
                        knobAfter.Draw(graphics, MathF.Round(x + position + knobWidthHalf), MathF.Round(y + (height - knobAfter.MinHeight) * 0.5f), MathF.Round(width - position - knobWidthHalf), MathF.Round(knobAfter.MinHeight), color);
                    }
                    else
                    {
                        knobAfter.Draw(graphics, x + position + knobWidthHalf, y + (height - knobAfter.MinHeight) * 0.5f, width - position - knobWidthHalf, knobAfter.MinHeight, color);
                    }
                }
                if (knob != null)
                {
                    if (Round)
                    {
                        knob.Draw(graphics, MathF.Round(x + position), MathF.Round(y + (height - knobHeight) * 0.5f), MathF.Round(knobWidth), MathF.Round(knobHeight), color);
                    }
                    else
                        knob.Draw(graphics, x + position, y + (height - knobHeight) * 0.5f, knobWidth, knobHeight, color);
                }
            }
        }

        public bool SetValue(object value)
        {
            if (binding != null && value != bindingData)
            {
                binding.Value = value;
            }
            if (value is float)
            {
                return SetValue((float)value);
            }
            else
            {
                return false;
            }
        }

        public bool SetValue(float value)
        {
            value = Clamp(MathF.Round(value / stepSize) * stepSize);
            float oldValue = this.value;
            if (value == oldValue) return false;
            float oldVisualValue = GetVisualValue();
            if (binding != null && value != oldValue)
            {
                binding.Value = value;
            }
            this.value = value;
            OnChange?.Invoke();
            var changeEvent = Pool<ChangeEvent>.Obtain();
            bool cancelled = Fire(changeEvent);
            if (cancelled)
                this.value = oldValue;
            else if (animateDuration > 0)
            {
                animateFromValue = oldVisualValue;
                animateTime = animateDuration;
            }
            Pool<ChangeEvent>.Free(changeEvent);
            return !cancelled;
        }

        public void SetRange(float Min, float Max)
        {
            if (Min > Max) throw new ArgumentException("Min must be <= Max: " + Min + " <= " + Max);
            this.Min = Min;
            this.Max = Max;
            if (value < Min)
                SetValue(Min);
            else if (value > Max) SetValue(Max);
        }

        public void SetStepSize(float stepSize)
        {
            if (stepSize <= 0) throw new ArgumentException("steps must be > 0: " + stepSize);
            this.stepSize = stepSize;
        }

        public float GetValue()
        {
            return value;
        }

        public float GetVisualValue()
        {
            if (animateTime > 0) return animateInterpolation.Apply(animateFromValue, value, 1 - animateTime / animateDuration);
            return value;
        }

        public float GetPercent()
        {
            if (Min == Max) return 0;
            return (value - Min) / (Max - Min);
        }

        public float GetVisualPercent()
        {
            if (Min == Max) return 0;
            return visualInterpolation.Apply((GetVisualValue() - Min) / (Max - Min));
        }

        protected virtual IDrawable GetKnobDrawable()
        {
            return (isDisabled && style.disabledKnob != null) ? style.disabledKnob : style.knob;
        }

        /** Returns progress bar visual position within the range. */
        protected float GetKnobPosition()
        {
            return position;
        }

        protected float Clamp(float value)
        {
            return MathF.Clamp(value, Min, Max);
        }

        #region IDisablable

        public bool IsDisabled()
        {
            return isDisabled;
        }

        public void SetDisabled(bool isDisabled)
        {
            this.isDisabled = isDisabled;
        }

        #endregion

        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                if (vertical)
                {
                    IDrawable knob = GetKnobDrawable();
                    IDrawable bg = (isDisabled && style.disabledBackground != null) ? style.disabledBackground : style.background;
                    return Math.Max(knob == null ? 0 : knob.MinWidth, bg.MinHeight);
                }
                else
                    return 140;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (vertical)
                    return 140;
                else
                {
                    IDrawable knob = GetKnobDrawable();
                    IDrawable bg = (isDisabled && style.disabledBackground != null) ? style.disabledBackground : style.background;
                    return Math.Max(knob == null ? 0 : knob.MinHeight, bg == null ? 0 : bg.MinHeight);
                }
            }
        }

        #endregion

        public float GetMinValue()
        {
            return Min;
        }

        public float GetMaxValue()
        {
            return Max;
        }

        public float GetStepSize()
        {
            return stepSize;
        }

        /** If > 0, changes to the progress bar value via {@link #setValue(float)} will happen over this duration in seconds. */
        public void SetAnimateDuration(float duration)
        {
            animateDuration = duration;
        }

        /** Sets the interpolation to use for {@link #setAnimateDuration(float)}. */
        public void SetAnimateInterpolation(Interpolation animateInterpolation)
        {
            this.animateInterpolation = animateInterpolation ?? throw new ArgumentNullException("animateInterpolation cannot be null.");
        }

        /** Sets the interpolation to use for display. */
        public void SetVisualInterpolation(Interpolation interpolation)
        {
            visualInterpolation = interpolation;
        }

        /** If true (the default), inner Drawable positions and sizes are rounded to integers. */
        public void SetRound(bool Round)
        {
            this.Round = Round;
        }

        /** True if the progress bar is vertical, false if it is horizontal. **/
        public bool IsVertical()
        {
            return vertical;
        }
    }

    public class ProgressBarStyle
    {
        /** The progress bar background, stretched only in one direction. Optional. */
        public IDrawable background;
        /** Optional. **/
        public IDrawable disabledBackground;
        /** Optional, centered on the background. */
        public IDrawable knob, disabledKnob;
        /** Optional. */
        public IDrawable knobBefore, knobAfter, disabledKnobBefore, disabledKnobAfter;

        public ProgressBarStyle()
        {

        }
    }
}
