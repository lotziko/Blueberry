using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blueberry.DataTools
{
    [AtlasExporter("Libgdx atlas")]
    public class LibgdxAtlasExporter : ContentExporter<AtlasExportData>
    {
        private Settings settings;

        public override void Export(string output, string name, AtlasExportData data)
        {
            settings = data.settings;
            WriteImages(output, name, data.pages);
            WritePackFile(output, name, data.pages);
        }

        private void WriteImages(string output, string name, List<Page> pages)
        {
            int fileIndex = 0;
            foreach (var page in pages)
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

                /////

                var data = new ImageData(new byte[page.imageWidth * page.imageHeight * 4], page.imageWidth, page.imageHeight);
                foreach (var r in page.outputRects)
                {
                    int x = r.x + page.x;
                    int y = r.y + page.y;
                    int w = r.width - settings.paddingX;
                    int h = r.height - settings.paddingY;

                    //y = page.imageHeight - (y + h);

                    for (int i = 0; i < h; i++)
                    {
                        int pos = (x + (i + y) * page.imageWidth) * 4;
                        Array.Copy(r.data.bitmap, (w * i) * 4, data.bitmap, pos, w * 4);
                    }
                }
                page.imageName = name + (fileIndex++ == 0 ? "" : fileIndex + "");
                Image.Save(data, output + page.imageName + ".png");
            }
        }

        private void WritePackFile(string output, string name, List<Page> pages)
        {
            var file = output + "/" + name + ".atlas";
            using (var writer = new StreamWriter(file))
            {
                foreach (var page in pages)
                {
                    writer.Write("\n" + page.imageName + ".png" + "\n");
                    writer.Write("size: " + page.imageWidth + "," + page.imageHeight + "\n");
                    writer.Write("format: " /*+ settings.format*/ + "\n");
                    writer.Write("filter: " /*+ settings.filterMin + "," + settings.filterMag*/ + "\n");
                    writer.Write("repeat: " /*+ getRepeatValue()*/ + "\n");

                    foreach (var area in page.outputRects)
                    {
                        WriteArea(writer, page, area, area.name);
                    }
                }
                writer.Close();
            }
        }

        private void WriteArea(StreamWriter writer, Page page, TextureArea rect, string name)
        {
            writer.Write(rect.name + "\n");
            writer.Write("  rotate: " + rect.rotated + "\n");
            writer.Write("  xy: " + (page.x + rect.x) + ", " + /*(page.y + page.height - rect.y - (rect.regionHeight - settings.paddingY))*/(page.y + rect.y) + "\n");

            writer.Write("  size: " + rect.regionWidth + ", " + rect.regionHeight + "\n");
            if (rect.splits != null)
            {
                writer.Write("  split: " //
                    + rect.splits[0] + ", " + rect.splits[1] + ", " + rect.splits[2] + ", " + rect.splits[3] + "\n");
            }
            if (rect.pads != null)
            {
                if (rect.splits == null) writer.Write("  split: 0, 0, 0, 0\n");
                writer.Write("  pad: " + rect.pads[0] + ", " + rect.pads[1] + ", " + rect.pads[2] + ", " + rect.pads[3] + "\n");
            }
            writer.Write("  orig: " + rect.originalWidth + ", " + rect.originalHeight + "\n");
            writer.Write("  offset: " + rect.offsetX + ", " + rect.offsetY + "\n");
            writer.Write("  index: " + rect.index + "\n");
        }
    }
}
