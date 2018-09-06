using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI.Actions
{
    public class ScaleToAction : TemporalAction
    {
        private float startX, startY;
        private float endX, endY;

        protected override void Begin()
        {
            startX = target.GetScaleX();
            startY = target.GetScaleY();
        }

        protected override void LocalUpdate(float percent)
        {
            target.SetScale(startX + (endX - startX) * percent, startY + (endY - startY) * percent);
        }

        public void SetScale(float x, float y)
        {
            endX = x;
            endY = y;
        }

        public void SetScale(float scale)
        {
            endX = scale;
            endY = scale;
        }

        public float GetX()
        {
            return endX;
        }

        public void SetX(float x)
        {
            this.endX = x;
        }

        public float GetY()
        {
            return endY;
        }

        public void SetY(float y)
        {
            this.endY = y;
        }
    }
}
