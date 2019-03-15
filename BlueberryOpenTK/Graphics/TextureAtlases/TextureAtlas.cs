using System;
using System.Collections.Generic;

namespace Blueberry
{
    public partial class TextureAtlas : IDisposable
    {
        internal List<Texture> texture = new List<Texture>();

        public int PageCount => texture.Count;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="texture"></param>

        public TextureAtlas(params Texture[] texture)
        {
            this.texture.AddRange(texture);
        }

        public Texture GetTexture(int page = 0) => texture[page];

        /// <summary>
        /// Dispose an atlas
        /// </summary>

        public void Dispose()
        {
            foreach (Texture page in texture)
            {
                page.Dispose();
            }
        }
    }
}
