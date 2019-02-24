using System;
using System.Collections.Generic;

namespace Blueberry
{
    public partial class TextureAtlas : IDisposable
    {
        internal List<Texture2D> texture = new List<Texture2D>();

        public int PageCount => texture.Count;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="texture"></param>

        public TextureAtlas(params Texture2D[] texture)
        {
            this.texture.AddRange(texture);
        }

        public Texture2D GetTexture(int page = 0) => texture[page];

        /// <summary>
        /// Dispose an atlas
        /// </summary>

        public void Dispose()
        {
            foreach (Texture2D page in texture)
            {
                page.Dispose();
            }
        }
    }
}
