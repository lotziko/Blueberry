
namespace Blueberry.UI
{
    public class AlphaAction : TemporalAction
    {
        private float start, end;
        private Col color;

        public AlphaAction()
        {

        }

        protected override void Begin()
        {
            if (color == default) color = target.GetColor();
            start = color.A;
        }

        protected override void LocalUpdate(float percent)
        {
            color.A = start + (end - start) * percent;
            target.SetColor(color);
            Render.Request();
        }

        public override void Reset()
        {
            base.Reset();
            color = default;
        }

        public Col GetColor()
        {
            return color;
        }

        /** Sets the color to modify. If null (the default), the {@link #getActor() actor's} {@link Actor#getColor() color} will be
	    * used. */
        public void SetColor(Col color)
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
