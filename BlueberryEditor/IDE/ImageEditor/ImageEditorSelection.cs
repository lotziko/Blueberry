using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE.ImageEditor
{
    class ImageEditorSelection
    {
        protected List<Point> selectedPixels;
        protected Rectangle imageBounds;

        public ImageEditorSelection(Rectangle imageBounds)
        {
            this.imageBounds = imageBounds;
            selectedPixels = new List<Point>();
        }

        public void AddRectangle(Rectangle bounds)
        {
            Point tmpPoint;
            for(int i = 0; i < bounds.Width; i++)
            {
                for(int j = 0; j < bounds.Height; j++)
                {
                    tmpPoint = new Point(i + bounds.X, j + bounds.Y);
                    if (imageBounds.Contains(tmpPoint) && !selectedPixels.Contains(tmpPoint))
                    {
                        selectedPixels.Add(tmpPoint);
                    }
                }
            }
        }
    }
}
