using System;
using System.IO;

namespace Blueberry.DataTools
{
    [ContentExporter("BBAtlas")]
    public class BBAtlasExporter : ContentExporter<AtlasExportData>
    {
        public override void Export(string output, string name, AtlasExportData data)
        {
            var file = File.Create(output + "/" + name + ".bba");
            var settings = data.settings;
            using (var writer = new BinaryWriter(file))
            {
                writer.Write(data.pages.Count);
                foreach (var page in data.pages)
                {
                    int width = page.width, height = page.height;
                    int edgePadX = 0, edgePadY = 0;
                    if (settings.edgePadding)
                    {
                        edgePadX = settings.paddingX;
                        edgePadY = settings.paddingY;
                        if (settings.duplicatePadding)
                        {
                            edgePadX /= 2;
                            edgePadY /= 2;
                        }
                        page.x = edgePadX;
                        page.y = edgePadY;
                        width += edgePadX * 2;
                        height += edgePadY * 2;
                    }
                    if (settings.pot)
                    {
                        width = MathF.NextPowerOfTwo(width);
                        height = MathF.NextPowerOfTwo(height);
                    }
                    if (settings.multipleOfFour)
                    {
                        width = width % 4 == 0 ? width : width + 4 - (width % 4);
                        height = height % 4 == 0 ? height : height + 4 - (height % 4);
                    }
                    width = Math.Max(settings.minWidth, width);
                    height = Math.Max(settings.minHeight, height);
                    page.imageWidth = width;
                    page.imageHeight = height;

                    var pageData = new ImageData(new byte[page.imageWidth * page.imageHeight * 4], page.imageWidth, page.imageHeight);

                    writer.Write(page.outputRects.Count);
                    foreach (var r in page.outputRects)
                    {
                        int x = r.x + page.x;
                        int y = r.y + page.y;
                        int w = r.width - settings.paddingX;
                        int h = r.height - settings.paddingY;

                        //BGRA to RGBA
                        byte tmp;
                        var m = r.data.bitmap;
                        for(int i = 0, n = r.data.Width * r.data.Height * 4; i < n;)
                        {
                            tmp = m[i];
                            m[i] = m[i + 2];
                            m[i + 2] = tmp;
                            i += 4;
                        }

                        //y = page.imageHeight - (y + h);

                        for (int i = 0; i < h; i++)
                        {
                            int pos = (x + (i + y) * page.imageWidth) * 4;
                            Array.Copy(r.data.bitmap, (w * i) * 4, pageData.bitmap, pos, w * 4);
                        }

                        WriteArea(writer, page, r, r.name);
                    }

                    WriteImage(writer, page, pageData);
                }

                writer.Close();
                file.Close();
            }
        }

        private void WriteArea(BinaryWriter writer, Page page, TextureArea region, string name)
        {
            writer.Write(region.name);
            writer.Write(region.rotated);

            writer.Write(page.x + region.x);
            writer.Write(page.y + region.y);
            writer.Write(region.regionWidth);
            writer.Write(region.regionHeight);

            //splits
            if (region.splits != null)
            {
                writer.Write(true);
                var splits = region.splits;
                writer.Write(splits[0]);
                writer.Write(splits[1]);
                writer.Write(splits[2]);
                writer.Write(splits[3]);
            }
            else
                writer.Write(false);

            //pads
            if (region.pads != null)
            {
                writer.Write(true);
                var pads = region.pads;
                writer.Write(pads[0]);
                writer.Write(pads[1]);
                writer.Write(pads[2]);
                writer.Write(pads[3]);
            }
            else
                writer.Write(false);

            writer.Write(region.originalWidth);
            writer.Write(region.originalHeight);

            writer.Write(region.offsetX);
            writer.Write(region.offsetY);

            writer.Write(region.index);
        }

        private void WriteImage(BinaryWriter writer, Page page, ImageData data)
        {
            var bytes = data.bitmap;
            var compressed = Data.Compress(bytes);
            writer.Write(page.imageWidth);
            writer.Write(page.imageHeight);
            writer.Write(compressed.Length);
            writer.Write(compressed);
        }
    }
}
