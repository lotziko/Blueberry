using System;
using System.Collections.Generic;
using System.Text;
using static Blueberry.UI.ChangeListener;

namespace Blueberry.UI
{
    public class Slider : ProgressBar
    {
        int draggingPointer = -1;
        bool mouseOver;
        private Interpolation visualInterpolationInverse = Interpolation.linear;
        private float[] snapValues;
        private float threshold;

        public Slider(float min, float max, float stepSize, bool vertical, Skin skin, string stylename = "default") : this(min, max, stepSize, vertical, skin.Get<SliderStyle>(stylename)) { }

        public Slider(float min, float max, float stepSize, bool vertical, SliderStyle style) : base(min, max, stepSize, vertical, style)
        {
            listeners.Add(new Listener(this));
        }

        #region Listener

        private class Listener : InputListener
        {
            private Slider _slider;

            public Listener(Slider slider)
            {
                _slider = slider;
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (_slider.isDisabled) return false;
                if (_slider.draggingPointer != -1) return false;
                _slider.draggingPointer = pointer;
                _slider.CalculatePositionAndValue(x, y);
                return true;
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (pointer != _slider.draggingPointer) return;
                _slider.draggingPointer = -1;
                if (!_slider.CalculatePositionAndValue(x, y))
                {
                    var changeEv = Pool<ChangeEvent>.Obtain();
                    _slider.Fire(changeEv);
                    Pool<ChangeEvent>.Free(changeEv);
                }
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                _slider.CalculatePositionAndValue(x, y);
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                if (pointer == -1) _slider.mouseOver = true;
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                if (pointer == -1) _slider.mouseOver = false;
            }
        }

        #endregion

        public override void SetStyle(ProgressBarStyle style)
        {
            if (style == null)
                throw new ArgumentNullException("Style can't be null");
            if (!(style is SliderStyle))
                throw new ArgumentException("Can only pass SliderStyle");
            base.SetStyle(style);
        }

        public new SliderStyle GetStyle() => (SliderStyle)style;

        protected override IDrawable GetKnobDrawable()
        {
            SliderStyle style = GetStyle();
            return (isDisabled && style.disabledKnob != null) ? style.disabledKnob
                : (IsDragging() && style.knobDown != null) ? style.knobDown
                    : ((mouseOver && style.knobOver != null) ? style.knobOver : style.knob);
        }

        bool CalculatePositionAndValue(float x, float y)
        {
            SliderStyle style = GetStyle();
            IDrawable knob = GetKnobDrawable();
            IDrawable bg = (isDisabled && style.disabledBackground != null) ? style.disabledBackground : style.background;

            float value;
            float oldPosition = position;

            float min = GetMinValue();
            float max = GetMaxValue();

            if (vertical)
            {
                float height = GetHeight() - bg.TopHeight - bg.BottomHeight;
                float knobHeight = knob == null ? 0 : knob.MinHeight;
                position = y - bg.BottomHeight - knobHeight * 0.5f;
                value = min + (max - min) * visualInterpolationInverse.Apply(position / (height - knobHeight));
                position = Math.Max(0, position);
                position = Math.Min(height - knobHeight, position);
            }
            else
            {
                float width = GetWidth() - bg.LeftWidth - bg.RightWidth;
                float knobWidth = knob == null ? 0 : knob.MinWidth;
                position = x - bg.LeftWidth - knobWidth * 0.5f;
                value = min + (max - min) * visualInterpolationInverse.Apply(position / (width - knobWidth));
                position = Math.Max(0, position);
                position = Math.Min(width - knobWidth, position);
            }

            float oldValue = value;
            if (!Input.IsShiftDown()) value = Snap(value);
            bool valueSet = SetValue(value);
            if (value == oldValue) position = oldPosition;
            Render.Request();
            return valueSet;
        }

        /** Returns a snapped value. */
        protected float Snap(float value)
        {
            if (snapValues == null || snapValues.Length == 0) return value;
            float bestDiff = -1, bestValue = 0;
            for (int i = 0; i < snapValues.Length; i++)
            {
                float snapValue = snapValues[i];
                float diff = Math.Abs(value - snapValue);
                if (diff <= threshold)
                {
                    if (bestDiff == -1 || diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestValue = snapValue;
                    }
                }
            }
            return bestDiff == -1 ? value : bestValue;
        }

        /** Will make this progress bar snap to the specified values, if the knob is within the threshold.
         * @param values May be null. */
        public void SetSnapToValues(float[] values, float threshold)
        {
            this.snapValues = values;
            this.threshold = threshold;
        }

        /** Returns true if the slider is being dragged. */
        public bool IsDragging()
        {
            return draggingPointer != -1;
        }

        /** Sets the inverse interpolation to use for display. This should perform the inverse of the
         * {@link #setVisualInterpolation(Interpolation) visual interpolation}. */
        public void SetVisualInterpolationInverse(Interpolation interpolation)
        {
            visualInterpolationInverse = interpolation;
        }
    }

    public class SliderStyle : ProgressBarStyle
    {
        /** Optional. */
        public IDrawable knobOver, knobDown;

        public SliderStyle() : base()
        {

        }

    }
}
