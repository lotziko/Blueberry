using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class TextTooltip : Tooltip
    {
        public TextTooltip(String text, Skin skin) : this(text, TooltipManager.GetInstance(), skin.Get<TextTooltipStyle>())
        {

        }

        public TextTooltip(String text, Skin skin, string stylename) : this(text, TooltipManager.GetInstance(), skin.Get<TextTooltipStyle>(stylename))
        {

        }

        public TextTooltip(String text, TextTooltipStyle style) : this(text, TooltipManager.GetInstance(), style)
        {

        }

        public TextTooltip(String text, TooltipManager manager, Skin skin) : this(text, manager, skin.Get<TextTooltipStyle>())
        {

        }

        public TextTooltip(String text, TooltipManager manager, Skin skin, string stylename) : this(text, manager, skin.Get<TextTooltipStyle>(stylename))
        {

        }

        public TextTooltip(string text, TooltipManager manager, TextTooltipStyle style) : base(null, manager)
        {
            var label = new Label(text, style.label);
            label.SetWrap(true);

            container.SetElement(label);
            container.Width(new SizeValue(this, manager));

            SetStyle(style);
        }

        #region SizeValue

        private class SizeValue : Value
        {
            private TextTooltip tooltip;
            private TooltipManager manager;

            public SizeValue(TextTooltip tooltip, TooltipManager manager)
            {
                this.tooltip = tooltip;
                this.manager = manager;
            }

            public override float Get(Element context)
            {
                return Math.Min(manager.maxWidth, tooltip.container.GetElement().PreferredWidth);
            }
        }

        #endregion

        public void SetStyle(TextTooltipStyle style)
        {
            if (style == null) throw new ArgumentNullException("style cannot be null");
            if (!(style is TextTooltipStyle)) throw new ArgumentException("style must be a TextTooltipStyle.");
            ((Label)container.GetElement()).SetStyle(style.label);
            container.SetBackground(style.background);
            container.SetMaxWidth(style.wrapWidth);
        }

    }

    public class TextTooltipStyle
    {
        public LabelStyle label;
        /** Optional. */
        public IDrawable background;
        /** Optional, 0 means don't wrap. */
        public float wrapWidth;

        public TextTooltipStyle()
        {
        }

        public TextTooltipStyle(LabelStyle label, IDrawable background)
        {
            this.label = label;
            this.background = background;
        }

        public TextTooltipStyle(TextTooltipStyle style)
        {
            this.label = new LabelStyle(style.label);
            background = style.background;
        }
    }

}