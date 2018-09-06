using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore.Fonts
{
    class FntReader : ContentTypeReader<BitmapFont>
    {
        protected override BitmapFont Read(ContentReader input, BitmapFont existingInstance)
        {
            var result = new BitmapFont
            {
                assetName = input.ReadString(),
                padLeft = input.ReadInt32(),
                padTop = input.ReadInt32(),
                padRight = input.ReadInt32(),
                padBottom = input.ReadInt32(),
                lineHeight = input.ReadInt32(),
                lineSpacing = input.ReadInt32(),
                spacing = input.ReadInt32(),
                pages = input.ReadInt32()
            };
            result.glyphPages = new GlyphPage[result.pages];
            for (int i = 0; i < result.pages; i++)
            {
                var name = input.ReadString();
                var page = new GlyphPage
                {
                    name = name,
                    texture = input.ContentManager.Load<Texture2D>(name)
                };
                result.glyphPages[i] = page;
            }

            int symbolsCount = input.ReadInt32();
            for(int i = 0; i < symbolsCount; i++)
            {
                var symbol = input.ReadChar();
                var bounds = new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
                var offset = new Vector2(input.ReadInt32(), input.ReadInt32());
                var xadvance = input.ReadInt32();
                var page = input.ReadInt32();
                var glyph = new Glyph(result.glyphPages[page].texture, bounds, offset, xadvance, page);
                
                result.AddGlyph(symbol, glyph);
            }

            return result;
        }
    }
}
