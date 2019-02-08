using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.UI
{
    public partial class VerticalGroup : Group
    {
        //TODO
        public override void DrawDebug(Graphics graphics)
        {
            base.DrawDebug(graphics);
            if (!GetDebug()) return;
            //graphics.DrawRectangleBorder(GetX() + padLeft, GetY() + padBottom, GetWidth() - padLeft - padRight, GetHeight() - padBottom - padTop, Table.debugElementColor);
        }
    }
}
