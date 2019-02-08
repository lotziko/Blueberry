using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    public partial class Stage
    {
        public void Draw(Graphics graphics)
        {
            root.Draw(graphics, _alpha);
            if (_debug)
                root.DrawDebug(graphics);
        }
    }
}
