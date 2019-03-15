
namespace Blueberry.UI
{
    public partial class Stage
    {
        public void Draw(Graphics graphics)
        {
            Camera.ForceCalculate();

            //graphics.Projection = Camera.ProjectionMatrix;
            graphics.Transform = Camera.TransformMatrix;
            graphics.Begin();
            root.Draw(graphics, _alpha);
            graphics.End();
            if (_debug)
                root.DrawDebug(graphics);
        }
    }
}
