using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlueberryCore
{
    public class BitmapFont
    {
        internal Dictionary<char, Glyph> glyphs;

        internal String assetName;

        internal GlyphPage[] glyphPages;

        //TODO descent, fix capHeight yOffset of drawing text
        internal int lineHeight, spacing, descent , ascent, capHeight , lineSpacing, spaceWidth, pages, padLeft, padTop, padRight, padBottom;

        private static Vector2 pos = Vector2.Zero;
        private static char unknownSymbol = (char)10, spaceSymbol = (char)32;
        private static Glyph tmpGlyph, defaultGlyph, spaceGlyph;

        public int GetAscent() => ascent;
        public int GetDescent() => descent;
        public int GetCapHeight() => capHeight;

        public int ComputeWidth(string text)
        {
            int width = 0;
            int count = text.Length;
            for (int i = 0; i < count; i++)
            {
                glyphs.TryGetValue(text[i], out tmpGlyph);
                if (tmpGlyph == null)
                    tmpGlyph = defaultGlyph;
                width += tmpGlyph._xadvance + spacing + padRight + padLeft;
            }
            return width;
        }

        public int ComputeWidth(string text, int begin, int end)
        {
            int width = 0;
            for (int i = begin; i <= end; i++)
            {
                glyphs.TryGetValue(text[i], out tmpGlyph);
                if (tmpGlyph == null)
                    tmpGlyph = defaultGlyph;
                width += tmpGlyph._xadvance + spacing + padRight + padLeft;
            }
            return width;
        }

        public string TruncateText(string text, string ellipsis, float maxLineWidth)
        {
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

                    if (glyph != null)
                        offsetX += spacing + glyph._xadvance;

                    if (!glyphs.TryGetValue(c, out glyph))
                        glyph = defaultGlyph;

                    var proposedWidth = offsetX + glyph._xadvance + spacing;
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

        public string WrapText(string text, float maxLineWidth)
        {
            var words = text.Split(' ');
            var sb = new StringBuilder();
            var lineWidth = 0f;

            if (maxLineWidth < spaceGlyph._source.Width)
                return string.Empty;

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

        public bool HasCharacter(char c)
        {
            return glyphs.ContainsKey(c);
        }

        public Glyph GetGlyph(char c)
        {
            glyphs.TryGetValue(c, out Glyph res);
            return res;
        }

        public Vector2 MeasureString(string text)
        {
            if (text.Length == 0)
            {
                return Vector2.Zero;
            }
            
            var result = Vector2.Zero;
            var width = 0.0f;
            var finalLineHeight = (float)lineHeight;
            var fullLineCount = 0;
            Glyph glyph = null;
            var offset = Vector2.Zero;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    fullLineCount++;
                    finalLineHeight = lineHeight;

                    offset.X = 0;
                    offset.Y = lineHeight * fullLineCount;
                    glyph = null;
                    continue;
                }

                if (glyph != null)
                    offset.X += spacing + glyph._xadvance + padLeft + padRight;

                if (!glyphs.TryGetValue(c, out glyph))
                    glyph = defaultGlyph;

                var proposedWidth = offset.X + glyph._xadvance + spacing + padLeft + padRight;
                if (proposedWidth > width)
                    width = proposedWidth;

                if (glyph._source.Width + glyph._offset.Y > finalLineHeight)
                    finalLineHeight = glyph._source.Height + glyph._offset.Y + padTop + padBottom;
            }

            result.X = width;
            result.Y = fullLineCount * lineHeight + finalLineHeight;

            return result;
        }

        public void Draw(Graphics graphics, string text, float x, float y, int begin, int end, Color? color = null)
        {
            pos.X = x;
            pos.Y = y;
            //if (!graphics.spriteBatch.HasBegun)
              //  graphics.spriteBatch.Begin();
            for (int i = begin; i < end; i++)
            {
                if (text[i] == '\n')
                {
                    pos.X = x;
                    pos.Y += lineHeight + padTop + padBottom;
                    continue;
                }
                pos.X += padLeft;
                glyphs.TryGetValue(text[i], out tmpGlyph);
                if (tmpGlyph == null)
                    tmpGlyph = defaultGlyph;
                tmpGlyph.Draw(graphics, pos, color);
                //graphics.DrawRectangleBorder(pos.X, pos.Y, tmpGlyph._source.Width, tmpGlyph._source.Height, Color.Red);
                pos.X += tmpGlyph._xadvance + spacing + padRight;
            }
            //graphics.spriteBatch.End();
        }

        public void Draw(Graphics graphics, string text, float x, float y, Color? color = null)
        {
            int count = text.Length;
            pos.X = x;
            pos.Y = y;
            //if (!graphics.spriteBatch.HasBegun)
              //  graphics.spriteBatch.Begin();
            for(int i = 0; i < count; i++)
            {
                if (text[i] == '\n')
                {
                    pos.X = x;
                    pos.Y += lineHeight + padTop + padBottom;
                    continue;
                }
                pos.X += padLeft;
                glyphs.TryGetValue(text[i], out tmpGlyph);
                if (tmpGlyph == null)
                    tmpGlyph = defaultGlyph;
                tmpGlyph.Draw(graphics, pos, color);
                //graphics.DrawRectangleBorder(pos.X, pos.Y, tmpGlyph._source.Width, tmpGlyph._source.Height, Color.Red);
                pos.X += tmpGlyph._xadvance + spacing + padRight;
            }
            //graphics.spriteBatch.End();
        }

        public void Draw(Graphics graphics, string text, float x, float y, int width, Color? color = null)
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
                        y += lineHeight + padTop + padBottom;
                    }
                    Draw(graphics, text, x, y, curIndex, i, color);
                    x += computedWidth;
                    curIndex = i + 1;
                }
                else if (text[i] == '\n')
                {
                    pos.X = x;
                    pos.Y += lineHeight + padTop + padBottom;
                    continue;
                }
            }
        }

        internal void AddGlyph(char character, Glyph glyph)
        {
            if (character == unknownSymbol)
                defaultGlyph = glyph;
            if (character == spaceSymbol)
            {
                spaceGlyph = glyph;
                spaceWidth = glyph == null ? glyph._source.Width : 3;
            }

            glyphs.Add(character, glyph);
        }

        /*public void SetSpacing(int spacing)
        {
            userSpacing = spacing;
        }*/

        public BitmapFont()
        {
            glyphs = new Dictionary<char, Glyph>();
        }
    }

    public struct GlyphPage
    {
        public Texture2D texture;
        public string name;
    }

    public class Glyph
    {
        internal int _xadvance, _page;

        internal Texture2D _texture;

        internal Vector2 _offset;

        internal Rectangle _source;
        
        public int Width { get { return _source.Width; } }
        public int Height { get { return _source.Height; } }

        public Glyph(Texture2D texture, Rectangle source, Vector2 offset, int xadvance, int page)
        {
            _texture = texture;
            _source = source;
            _offset = offset;
            _xadvance = xadvance;
            _page = page;
        }

        public void Draw(Graphics graphics, Vector2 pos, Color? color = null)
        {
            graphics.Draw(_texture, pos + _offset, _source, color ?? Color.White);
        }
    }
}
