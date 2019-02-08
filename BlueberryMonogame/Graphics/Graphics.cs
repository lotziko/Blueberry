using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public partial class Graphics
    {
        public static int CurrentTargetWidth { get { return 0; } }
        public static int CurrentTargetHeigth { get { return 0; } }

        private Mat transform, projection;

        public Mat Transform
        {
            get => transform;
            set
            {
                transform = value;
                //batch.Transform = transform.m;
            }
        }

        public Mat Projection
        {
            get => projection;
            set
            {
                projection = value;
                //batch.Projection = projection.m;
            }
        }

        public void Flush() { }

        public void Begin() { }//=> batch.Begin();

        public void End() { }//=> batch.End()

        public void DrawRectangle(float x, float y, float width, float height, bool border = false, Col? color = null)
        {

        }
    }
}
