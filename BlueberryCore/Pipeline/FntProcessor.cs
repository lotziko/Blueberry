using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.IO;

namespace BlueberryCore.Pipeline
{
    [ContentProcessor(DisplayName = "Fnt file processor - BlueberryEngine")]
    public class FntProcessor : ContentProcessor<FileStream, BitmapFont>
    {
        public override BitmapFont Process(FileStream input, ContentProcessorContext context)
        {
            //try
            {
                context.Logger.LogMessage("Processing");
                var result = new BitmapFont
                {
                    assetName = input.Name.Substring(input.Name.LastIndexOf("\\") + 1).Replace(".fnt", "")
                };
                result.capHeight = 1;

                using (StreamReader reader = new StreamReader(input))
                {
                    string line = reader.ReadLine(); // info
                    if (line == null) throw new Exception("File is empty");

                    line = line.Substring(line.IndexOf("padding=") + 8);
                    var padding = line.Substring(0, line.IndexOf(' ')).Split(new char[] { ',' }, 4);
                    if (padding.Length != 4) throw new Exception("Invalid padding.");
                    result.padTop = int.Parse(padding[0]);
                    result.padRight = int.Parse(padding[1]);
                    result.padBottom = int.Parse(padding[2]);
                    result.padLeft = int.Parse(padding[3]);
                    int padY = result.padTop + result.padBottom;

                    line = reader.ReadLine();
                    if (line == null) throw new Exception("Missing common header.");
                    var common = line.Split(new char[] { ' ' }, 7); // At most we want the 6th element; i.e. "page=N"

                    // At least lineHeight and base are required.
                    if (common.Length < 3) throw new Exception("Invalid common header.");

                    if (!common[1].StartsWith("lineHeight=")) throw new Exception("Missing: lineHeight");
                    result.lineHeight = int.Parse(common[1].Substring(11));

                    if (!common[2].StartsWith("base=")) throw new Exception("Missing: base");
                    int baseLine = int.Parse(common[2].Substring(5));

                    int pageCount = 1;
                    if (common.Length >= 6 && common[5] != null && common[5].StartsWith("pages="))
                    {
                        try
                        {
                            pageCount = Math.Max(1, int.Parse(common[5].Substring(6)));
                        }
                        catch (Exception ex)
                        { // Use one page.
                        }
                    }

                    result.glyphPages = new GlyphPage[pageCount];

                    // Read each page definition.
                    for (int p = 0; p < pageCount; p++)
                    {
                        if (result.glyphPages[p].Equals(default(GlyphPage)))
                        {
                            var page = new GlyphPage
                            {
                                //TODO rewrite
                                name = GetValue("file", line).Trim('"').Replace(".png", "")
                            };
                            result.glyphPages[p] = page;
                        }
                    }
                    result.descent = 0;

                    while (true)
                    {
                        line = reader.ReadLine();
                        if (line == null) break; // EOF
                        if (line.StartsWith("kernings ")) break; // Starting kernings block.
                        if (!line.StartsWith("char ")) continue;

                        var symbol = (char)int.Parse(GetValue("id", line));

                        var bounds = new Rectangle(int.Parse(GetValue("x", line)), int.Parse(GetValue("y", line)), int.Parse(GetValue("width", line)), int.Parse(GetValue("height", line)));
                        var offset = new Vector2(int.Parse(GetValue("xoffset", line)), int.Parse(GetValue("yoffset", line)));
                        var xadvance = int.Parse(GetValue("xadvance", line));
                        var page = int.Parse(GetValue("page", line));
                        var glyph = new Glyph(null, bounds, offset, xadvance, page);

                        result.AddGlyph(symbol, glyph);

                        if (bounds.Width > 0 && bounds.Height > 0) result.descent = (int)Math.Min(baseLine + glyph._offset.Y, result.descent);
                    }

                    result.descent += result.padBottom;

                    //////
                    ///
                    var capChars = new char[]{'M', 'N', 'B', 'D', 'C', 'E', 'F', 'K', 'A', 'G', 'H', 'I', 'J', 'L', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

                    Glyph capGlyph = null;
                    foreach (var capChar in capChars)
                    {
                        capGlyph = result.GetGlyph(capChar);
                        if (capGlyph != null) break;
                    }

                    if (capGlyph == null)
                    {
                        foreach(var glyphPair in result.glyphs)
                        {
                            var glyph = glyphPair.Value;
                            if (glyph == null || glyph.Height == 0 || glyph.Width == 0) continue;
							result.capHeight = Math.Max(result.capHeight, glyph.Height);
                        }
                    }
                    else
                        result.capHeight = capGlyph.Height;
                    result.capHeight -= padY;

                    result.ascent = baseLine - result.capHeight;

                    reader.Close();
                    return result;

                    #region old

                    /*while (true)
                    {
                        String line = reader.ReadLine();
                        if (line == null)
                            break;
                        //if (line.Trim().Length == 0)
                        //    continue;
                        if (line.StartsWith("info"))
                        {
                            var padding = GetValue("padding", line).Split(',');
                            result.padTop = int.Parse(padding[0]);
                            result.padRight = int.Parse(padding[1]);
                            result.padBottom = int.Parse(padding[2]);
                            result.padLeft = int.Parse(padding[3]);

                            var spacing = GetValue("spacing", line).Split(',');
                            result.spacing = int.Parse(spacing[0]);
                            result.lineSpacing = int.Parse(spacing[1]);
                        }
                        else if (line.StartsWith("common"))
                        {
                            result.lineHeight = int.Parse(GetValue("lineHeight", line));
                            result.pages = int.Parse(GetValue("pages", line));
                            result.glyphPages = new GlyphPage[result.pages];
                        }
                        else if (line.StartsWith("page"))
                        {
                            for (int i = 0; i < result.pages; i++)
                            {
                                if (result.glyphPages[i].Equals(default(GlyphPage)))
                                {
                                    var page = new GlyphPage
                                    {
                                        name = GetValue("file", line).Trim('"').Replace(".png", "")
                                    };
                                    result.glyphPages[i] = page;
                                }
                            }
                        }
                        else if (line.StartsWith("chars"))
                        {
                            
                        }
                        else if (line.StartsWith("char"))
                        {
                            var symbol = (char)int.Parse(GetValue("id", line));
                            
                            var bounds = new Rectangle(int.Parse(GetValue("x", line)), int.Parse(GetValue("y", line)), int.Parse(GetValue("width", line)), int.Parse(GetValue("height", line)));
                            var offset = new Vector2(int.Parse(GetValue("xoffset", line)), int.Parse(GetValue("yoffset", line)));
                            var xadvance = int.Parse(GetValue("xadvance", line));
                            var page = int.Parse(GetValue("page", line));
                            var glyph = new Glyph(null, bounds, offset, xadvance, page);

                            result.AddGlyph(symbol, glyph);
                        }
                    }
                    reader.Close();
                    return result;*/

                    #endregion
                }
            }
            //catch (Exception ex)
            {
                //context.Logger.LogMessage("Error {0}", ex);
                //throw;
            }
        }

        private string GetValue(string key, string str)
        {
            int pos = str.IndexOf(key + '=') + (key + '=').Length;
            if (pos == -1)
                throw new Exception("value not found.");
            int end = str.IndexOf(' ', pos);
            if (end == -1)
                end = str.Length;
            if (end - pos <= 0)
                return "";
            return str.Substring(pos, end - pos);
        }
    }
}
