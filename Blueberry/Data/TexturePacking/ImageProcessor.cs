using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Blueberry.DataTools
{
    internal class ImageProcessor
    {
        private Settings settings;

        public ImageProcessor(Settings settings)
        {
            this.settings = settings;
        }

        public TextureArea CreateArea(string path)
        {
            var data = Image.Load(path);
            var name = Path.GetFileNameWithoutExtension(path);

            int width = data.Width, height = data.Height;

            var isPatch = name.EndsWith(".9");
            int[] splits = null, pads = null;
            TextureArea area = null;
            if (isPatch)
            {
                // Strip ".9" from file name, read ninepatch split pixels, and strip ninepatch split pixels.
                name = name.Substring(0, name.Length - 2);
                splits = GetSplits(data);
                pads = GetPads(data, splits);
                // Strip split pixels.
                width -= 2;
                height -= 2;
                data.Crop(1, 1, 1, 1);
                //BufferedImage newImage = new BufferedImage(width, height, BufferedImage.TYPE_4BYTE_ABGR);
                //newImage.getGraphics().drawImage(image, 0, 0, width, height, 1, 1, width + 1, height + 1, null);
                //image = newImage;
            }

            if (isPatch)
            {
                // Ninepatches aren't rotated or whitespace stripped.
                area = new TextureArea(data, 0, 0, width, height, true);
                area.data = data;
                area.splits = splits;
                area.pads = pads;
                //area.canRotate = false;
            }
            else
            {
                area = StripWhitespace(data);
                if (area == null) return null;
            }

            int index = -1;
            if (settings.useIndexes)
            {
                var str = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
                int.TryParse(str, out index);
                //Matcher matcher = indexPattern.matcher(name);
                //if (matcher.matches())
                //{
                //    name = matcher.group(1);
                //    index = System.Text.RegularExpressions.Regex.Match(name)//Integer.parseInt(matcher.group(2));
                //}
            }

            area.name = name;
            area.index = index;

            return area;
        }

        /** Strips whitespace and returns the rect, or null if the image should be ignored. */
        private TextureArea StripWhitespace(ImageData source)
        {
            //WritableRaster alphaRaster = source.getAlphaRaster();
            if (/*alphaRaster == null ||*/ (!settings.stripWhitespaceX && !settings.stripWhitespaceY))
                return new TextureArea(source, 0, 0, source.Width, source.Height, false);
            var col = new byte[4];
            int top = 0;
            int bottom = source.Height;
            if (settings.stripWhitespaceY)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        source.GetPixel(x, y, ref col);
                        int alpha = col[3];
                        if (alpha < 0) alpha += 256;
                        if (alpha > settings.alphaThreshold) goto outer1;
                    }
                    top++;
                }
                outer1:
                for (int y = source.Height; --y >= top;)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        source.GetPixel(x, y, ref col);
                        int alpha = col[3];
                        if (alpha < 0) alpha += 256;
                        if (alpha > settings.alphaThreshold) goto outer2;
                    }
                    bottom--;
                }
                outer2:
                // Leave 1px so nothing is copied into padding.
                if (settings.duplicatePadding)
                {
                    if (top > 0) top--;
                    if (bottom < source.Height) bottom++;
                }
            }
            int left = 0;
            int right = source.Width;
            if (settings.stripWhitespaceX)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    for (int y = top; y < bottom; y++)
                    {
                        source.GetPixel(x, y, ref col);
                        int alpha = col[3];
                        if (alpha < 0) alpha += 256;
                        if (alpha > settings.alphaThreshold) goto outer1;
                    }
                    left++;
                }
                outer1:
                for (int x = source.Width; --x >= left;)
                {
                    for (int y = top; y < bottom; y++)
                    {
                        source.GetPixel(x, y, ref col);
                        int alpha = col[3];
                        if (alpha < 0) alpha += 256;
                        if (alpha > settings.alphaThreshold) goto outer2;
                    }
                    right--;
                }
                outer2:
                // Leave 1px so nothing is copied into padding.
                if (settings.duplicatePadding)
                {
                    if (left > 0) left--;
                    if (right < source.Width) right++;
                }
            }
            int newWidth = right - left;
            int newHeight = bottom - top;
            if (newWidth <= 0 || newHeight <= 0)
            {
                if (settings.ignoreBlankImages)
                    return null;
                else
                    return new TextureArea(new ImageData(new byte[] {0, 0, 0, 0}, 1, 1), 0, 0, 1, 1, false);
            }
            return new TextureArea(source, left, top, newWidth, newHeight, false);
        }

        /** Returns the splits, or null if the image had no splits or the splits were only a single region. Splits are an int[4] that
	 * has left, right, top, bottom. */
        private int[] GetSplits(ImageData data)
        {
            //WritableRaster raster = image.getRaster();

            int startX = GetSplitPoint(data, 1, 0, true, true);
            int endX = GetSplitPoint(data, startX, 0, false, true);
            int startY = GetSplitPoint(data, 0, 1, true, false);
            int endY = GetSplitPoint(data, 0, startY, false, false);

            // Ensure pixels after the end are not invalid.
            GetSplitPoint(data, endX + 1, 0, true, true);
            GetSplitPoint(data, 0, endY + 1, true, false);

            // No splits, or all splits.
            if (startX == 0 && endX == 0 && startY == 0 && endY == 0) return null;

            // Subtraction here is because the coordinates were computed before the 1px border was stripped.
            if (startX != 0)
            {
                startX--;
                endX = data.Width - 2 - (endX - 1);
            }
            else
            {
                // If no start point was ever found, we assume full stretch.
                endX = data.Width - 2;
            }
            if (startY != 0)
            {
                startY--;
                endY = data.Height - 2 - (endY - 1);
            }
            else
            {
                // If no start point was ever found, we assume full stretch.
                endY = data.Height - 2;
            }

            /*if (scale != 1)
            {
                startX = (int)Math.round(startX * scale);
                endX = (int)Math.round(endX * scale);
                startY = (int)Math.round(startY * scale);
                endY = (int)Math.round(endY * scale);
            }*/

            return new int[] { startX, endX, startY, endY };
        }

        /** Returns the pads, or null if the image had no pads or the pads match the splits. Pads are an int[4] that has left, right,
         * top, bottom. */
        private int[] GetPads(ImageData data, int[] splits)
        {
            //WritableRaster raster = image.getRaster();

            int bottom = data.Height - 1;
            int right = data.Width - 1;

            int startX = GetSplitPoint(data, 1, bottom, true, true);
            int startY = GetSplitPoint(data, right, 1, true, false);

            // No need to hunt for the end if a start was never found.
            int endX = 0;
            int endY = 0;
            if (startX != 0) endX = GetSplitPoint(data, startX + 1, bottom, false, true);
            if (startY != 0) endY = GetSplitPoint(data, right, startY + 1, false, false);

            // Ensure pixels after the end are not invalid.
            GetSplitPoint(data, endX + 1, bottom, true, true);
            GetSplitPoint(data, right, endY + 1, true, false);

            // No pads.
            if (startX == 0 && endX == 0 && startY == 0 && endY == 0)
            {
                return null;
            }

            // -2 here is because the coordinates were computed before the 1px border was stripped.
            if (startX == 0 && endX == 0)
            {
                startX = -1;
                endX = -1;
            }
            else
            {
                if (startX > 0)
                {
                    startX--;
                    endX = data.Width - 2 - (endX - 1);
                }
                else
                {
                    // If no start point was ever found, we assume full stretch.
                    endX = data.Width - 2;
                }
            }
            if (startY == 0 && endY == 0)
            {
                startY = -1;
                endY = -1;
            }
            else
            {
                if (startY > 0)
                {
                    startY--;
                    endY = data.Height - 2 - (endY - 1);
                }
                else
                {
                    // If no start point was ever found, we assume full stretch.
                    endY = data.Height - 2;
                }
            }

            /*if (scale != 1)
            {
                startX = (int)Math.round(startX * scale);
                endX = (int)Math.round(endX * scale);
                startY = (int)Math.round(startY * scale);
                endY = (int)Math.round(endY * scale);
            }*/

            int[] pads = new int[] { startX, endX, startY, endY };

            if (splits != null && Equals(pads, splits))
            {
                return null;
            }

            return pads;
        }

        /** Hunts for the start or end of a sequence of split pixels. Begins searching at (startX, startY) then follows along the x or
         * y axis (depending on value of xAxis) for the first non-transparent pixel if startPoint is true, or the first transparent
         * pixel if startPoint is false. Returns 0 if none found, as 0 is considered an invalid split point being in the outer border
         * which will be stripped. */
        static private int GetSplitPoint(ImageData data, int startX, int startY, bool startPoint, bool xAxis)
        {
            var rgba = new byte[4];

            int next = xAxis ? startX : startY;
            int end = xAxis ? data.Width : data.Height;
            int breakA = startPoint ? 255 : 0;

            int x = startX;
            int y = startY;
            while (next != end)
            {
                if (xAxis)
                    x = next;
                else
                    y = next;

                data.GetPixel(x, y, ref rgba);
                if (rgba[3] == breakA) return next;

                if (!startPoint && (rgba[0] != 0 || rgba[1] != 0 || rgba[2] != 0 || rgba[3] != 255))
                {
                    //splitError(x, y, rgba, name);
                }

                next++;
            }

            return 0;
        }
    }
}
