using BlueberryCore.Fonts;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;

namespace BlueberryCore.Pipeline
{
    [ContentTypeWriter]
    class FntWriter : ContentTypeWriter<BitmapFont>
    {
        protected override void Write(ContentWriter output, BitmapFont value)
        {
            output.Write(value.assetName);

            output.Write(value.padLeft);
            output.Write(value.padTop);
            output.Write(value.padRight);
            output.Write(value.padBottom);

            output.Write(value.lineHeight);
            output.Write(value.lineSpacing);
            output.Write(value.spacing);

            int pagesCount = value.pages;
            output.Write(pagesCount);
            for(int i = 0; i < pagesCount; i++)
            {
                output.Write(value.glyphPages[i].name);
            }

            int symbolsCount = value.glyphs.Count;
            output.Write(symbolsCount);
            foreach (KeyValuePair<char, Glyph> p in value.glyphs)
            {
                var glyph = p.Value;
                output.Write(p.Key);
                output.Write(glyph._source.X);
                output.Write(glyph._source.Y);
                output.Write(glyph._source.Width);
                output.Write(glyph._source.Height);
                output.Write((int)glyph._offset.X);
                output.Write((int)glyph._offset.Y);
                output.Write(glyph._xadvance);
                output.Write(glyph._page);
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(BitmapFont).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(FntReader).AssemblyQualifiedName;
        }
    }
}
