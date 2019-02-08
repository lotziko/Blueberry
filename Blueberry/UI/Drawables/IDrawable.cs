﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.UI
{
    /// <summary>
	/// A drawable knows how to draw itself at a given rectangular size. It provides border sizes and a minimum size so that other code
	/// can determine how to size and position content.
	/// </summary>
	public interface IDrawable
    {
        float LeftWidth { get; set; }
        float RightWidth { get; set; }
        float TopHeight { get; set; }
        float BottomHeight { get; set; }
        float MinWidth { get; set; }
        float MinHeight { get; set; }

        void SetPadding(float top, float bottom, float left, float right);

        void Draw(Graphics graphics, float x, float y, float width, float height, Col color);
    }
}