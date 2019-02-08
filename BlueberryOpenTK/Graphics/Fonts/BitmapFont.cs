using Blueberry;
using Blueberry.DataTools;
using System;
using System.Collections.Generic;
using System.Text;
using TrueTypeSharp;

namespace BlueberryOpenTK
{
    public class BitmapFont : IFont, IDisposable
    {
        protected Dictionary<char, Glyph> avalibleGlyphs = new Dictionary<char, Glyph>();
        protected float scale;
        protected List<Texture2D> textures = new List<Texture2D>();
        protected MaxRectsBinPack packer;
        protected TrueTypeFont font;

        protected static char unknownSymbol = (char)10, spaceSymbol = (char)32;
        protected static Glyph tmpGlyph, defaultGlyph, spaceGlyph;
        protected static Vec2 pos;

        public float Ascent { get; }
        public float Descent { get; }
        public float LineHeight { get; }
        public float SpaceWidth { get; }

        const int textureSize = 512;
        const float lineScale = 1.2f;

        public BitmapFont(string path, float size)
        {
            float dpi = 1f;

            font = new TrueTypeFont(path);
            scale = font.GetScaleForPixelHeight(size * dpi);
            packer = new MaxRectsBinPack(textureSize, textureSize);

            font.GetFontVMetrics(out int asc, out int desc, out int gap);

            Ascent = asc * scale;
            Descent = desc * scale;
            LineHeight = size * dpi;

            SpaceWidth = 0;//GetGlyph(' ').xadvance;

            textures.Add(new Texture2D(textureSize, textureSize, true));
        }

        ~BitmapFont()
        {
            Dispose();
        }

        public void Dispose()
        {
            foreach (var texture in textures)
                texture.Dispose();
        }

        protected Glyph GetGlyph(char character)
        {
            if (!avalibleGlyphs.TryGetValue(character, out tmpGlyph))
            {
                AddGlyph(character);
                return avalibleGlyphs[character];
            }
            return tmpGlyph;
        }

        public void AddGlyph(char character)
        {
            if (avalibleGlyphs.ContainsKey(character))
                return;

            uint index = font.FindGlyphIndex(character);

            var result = font.GetGlyphBitmap(index, scale, scale, out int width, out int height, out int xOffset, out int yOffset);
            font.GetGlyphHMetrics(index, out int advance, out int leftBearing);

            advance = (int)(advance * scale);

            while (true)
            {
                var rect = packer.Insert(width + 2, height + 2, FreeRectChoiceHeuristic.BottomLeftRule);

                if (width == 0 || rect.width != 0)
                {
                    int x = rect.x, y = rect.y;
                    var texture = textures[textures.Count - 1];

                    if (width != 0)
                    {
                        var gray = result;
                        var buffer = new byte[width * height * 4];
                        int pointer = 0;
                        for (int i = 0, len = gray.Length; i < len; i++)
                        {
                            if (gray[i] > 0)
                            {
                                buffer[pointer] = 255;
                                buffer[pointer + 1] = 255;
                                buffer[pointer + 2] = 255;
                                buffer[pointer + 3] = gray[i];
                            }
                            pointer += 4;
                        }
                        texture.SetData(buffer, x + 1, y + 1, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte);

                    }
                    avalibleGlyphs.Add(character, new Glyph()
                    {
                        region = new TextureRegion(texture, x + 1, y + 1, width, height),
                        xadvance = advance,
                        yadvance = 0,
                        xbearing = xOffset,
                        ybearing = yOffset,
                    });
                    break;
                }
                else if (rect.width == 0)
                {
                    textures.Add(new Texture2D(textureSize, textureSize, true));
                    packer.Init(textureSize, textureSize);
                    continue;
                }
            }
        }

        public void Draw(Graphics graphics, string text, float x, float y, Col? color = null)
        {
            int count = text.Length;
            pos.X = x;
            pos.Y = y;
            for (int i = 0; i < count; i++)
            {
                if (text[i] == '\n')
                {
                    pos.X = x;
                    pos.Y += LineHeight;
                    continue;
                }
                tmpGlyph = GetGlyph(text[i]);
                if (tmpGlyph == null)
                    tmpGlyph = defaultGlyph;
                tmpGlyph.region.Draw(graphics, pos.X + tmpGlyph.xbearing, pos.Y + tmpGlyph.ybearing + Ascent, color);
                pos.X += tmpGlyph.xadvance;
            }
            //var m = MeasureString(text);
            //graphics.DrawRectangle(x, y, m.X, m.Y, true, Col.Red);
            //for(int i = 0; i < textures.Count; i++)
            //{
            //    graphics.DrawTexture(textures[i], i * textureSize, 0);
            //}
        }

        public void Draw(Graphics graphics, string text, float x, float y, int begin, int end, Col? color = null)
        {
            pos.X = x;
            pos.Y = y;
            for (int i = begin; i < end; i++)
            {
                if (text[i] == '\n')
                {
                    pos.X = x;
                    pos.Y += LineHeight;
                    continue;
                }
                tmpGlyph = GetGlyph(text[i]);
                if (tmpGlyph == null)
                    tmpGlyph = defaultGlyph;
                tmpGlyph.region.Draw(graphics, pos.X + tmpGlyph.xbearing, pos.Y + tmpGlyph.ybearing + Ascent, color);
                pos.X += tmpGlyph.xadvance;
            }
        }

        public void Draw(Graphics graphics, string text, float x, float y, int width, Col? color = null)
        {
            int len = text.Length, curIndex = 0, computedWidth;
            float beginX = x;
            for (int i = 0; i < len; i++)
            {
                if (i == len - 1)
                {
                    Draw(graphics, text, x, y, curIndex, i + 1, color);
                }
                else if (text[i] == ' ')
                {
                    computedWidth = ComputeWidth(text, curIndex, i);
                    if (x + computedWidth > beginX + width && computedWidth < width)
                    {
                        x = beginX;
                        y += LineHeight;
                    }
                    Draw(graphics, text, x, y, curIndex, i, color);
                    x += computedWidth;
                    curIndex = i + 1;
                }
                else if (text[i] == '\n')
                {
                    pos.X = x;
                    pos.Y += LineHeight;
                    continue;
                }
            }
        }

        public string WrapText(string text, float maxLineWidth)
        {
            var words = text.Split(' ');
            var sb = new StringBuilder();
            var lineWidth = 0f;

            if (maxLineWidth < spaceGlyph.region.Width)
                return string.Empty;

            int spaceWidth = avalibleGlyphs[' '].xadvance;

            foreach (var word in words)
            {
                var size = MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += spaceWidth;
                }
                else
                {
                    if (size.X > maxLineWidth)
                    {
                        if (sb.ToString() == "")
                            sb.Append(WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                        else
                            sb.Append("\n" + WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                    }
                    else
                    {
                        sb.Append("\n" + word + " ");
                        lineWidth = size.X + spaceWidth;
                    }
                }
            }

            return sb.ToString();
        }


        public string TruncateText(string text, string ellipsis, float maxLineWidth)
        {
            int spaceWidth = GetGlyph(' ').xadvance;

            if (maxLineWidth < spaceWidth)
                return string.Empty;

            var size = MeasureString(text);

            // do we even need to truncate?
            var ellipsisWidth = MeasureString(ellipsis).X;
            if (size.X > maxLineWidth)
            {
                var sb = new StringBuilder();

                var width = 0.0f;
                Glyph glyph = null;
                var offsetX = 0.0f;

                // determine how many chars we can fit in maxLineWidth - ellipsisWidth
                for (var i = 0; i < text.Length; i++)
                {
                    var c = text[i];

                    // we dont deal with line breaks or tabs
                    if (c == '\r' || c == '\n')
                        continue;

                    glyph = GetGlyph(c);

                    offsetX += glyph.xadvance;  

                    var proposedWidth = offsetX + glyph.xadvance;
                    if (proposedWidth > width)
                        width = proposedWidth;

                    if (width < maxLineWidth - ellipsisWidth)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        // no more room. append our ellipsis and get out of here
                        sb.Append(ellipsis);
                        break;
                    }
                }

                return sb.ToString();
            }

            return text;
        }

        public int ComputeWidth(string text)
        {
            return ComputeWidth(text, 0, text.Length);
        }

        public int ComputeWidth(string text, int begin, int end)
        {
            int width = 0;
            for (int i = begin; i <= end; i++)
            {
                tmpGlyph = GetGlyph(text[i]);
                width += tmpGlyph.xadvance - tmpGlyph.xbearing;// + spacing + padRight + padLeft;
            }
            return width;
        }

        public Vec2 MeasureString(string text)
        {
            if (text.Length == 0)
                return Vec2.Zero;

            var result = Vec2.Zero;
            var width = 0.0f;
            var finalLineHeight = (float)LineHeight;
            var fullLineCount = 0;
            Glyph glyph = null;
            var offset = Vec2.Zero;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    fullLineCount++;
                    finalLineHeight = LineHeight;

                    offset.X = 0;
                    offset.Y = LineHeight * fullLineCount;
                    glyph = null;
                    continue;
                }

                glyph = GetGlyph(text[i]);
                offset.X += glyph.xadvance;// - glyph.xbearing;//spacing + padLeft + padRight;

                var proposedWidth = offset.X + glyph.xadvance;// - glyph.xbearing;// + spacing + padLeft + padRight;
                if (proposedWidth > width)
                    width = proposedWidth;

                if (glyph.region.Height > finalLineHeight)
                    finalLineHeight = glyph.region.Height;// + padTop + padBottom;
            }

            result.X = width;
            result.Y = fullLineCount * LineHeight + finalLineHeight;

            return result;
        }

        public bool HasCharacter(char character)
        {
            return avalibleGlyphs.ContainsKey(character);
        }

        public float GetCharacterAdvance(char character)
        {
            return GetGlyph(character).xadvance;
        }

        protected class Glyph
        {
            public TextureRegion region;
            public int xadvance, yadvance, xbearing, ybearing, xoffset, yoffset;
        }
    }
}
