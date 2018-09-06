using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;

namespace BlueberryCore.TextureAtlases
{
    internal class Region
    {
        internal String name;
        internal int index = -1, left, top, width, height, originalWidth, originalHeight, offsetX, offsetY, page;
        internal int[] splits, pads;
        internal bool rotate;

        /// <summary>
        /// Get name
        /// </summary>
        /// <returns></returns>

        public String GetName() => name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>

        public Region(String name, int left, int top, int width, int height)
        {
            this.name = name;
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Constructor
        /// </summary>

        public Region() { }

        public override string ToString()
        {
            return name;
        }
    }
}
