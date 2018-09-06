using Microsoft.Xna.Framework;

namespace BlueberryCore.UI.Actions
{
    public class AlphaAction : TemporalAction
    {
        private float start, end;
        private Color color;

        protected override void Begin()
        {
            if (color == default) color = target.GetColor();
            start = color.A;
        }

        protected override void LocalUpdate(float percent)
        {
            color.A = (byte)(start + (end - start) * percent);
            target.SetColor(color);
            Core.RequestRender();
        }

        public override void Reset()
        {
            base.Reset();
            color = default;
        }

        public Color GetColor()
        {
            return color;
        }

        /** Sets the color to modify. If null (the default), the {@link #getActor() actor's} {@link Actor#getColor() color} will be
	    * used. */
        public void SetColor(Color color)
        {
            this.color = color;
        }

        public float GetAlpha()
        {
            return end;
        }

        public void SetAlpha(float alpha)
        {
            end = alpha;
        }
    }
}
