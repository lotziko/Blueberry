using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.DataTools
{
    public class BinTexturePacker : TexturePacker
    {
        /// <summary>
        /// Places regions into pages
        /// </summary>
        /// <param name="data">List of regions</param>
        /// <param name="preferredWidth">Value is a power of 2</param>
        /// <param name="preferredHeight">Value is a power of 2</param>
        /// <returns></returns>

        internal override List<PackerRegionData> CalculateRectangles(List<PackerRegionData> data, int? preferredWidth = null, int? preferredHeight = null)
        {
            var result = new List<PackerRegionData>();

            if (!preferredWidth.HasValue || !preferredHeight.HasValue)
            {
                int size = 2;
                while (true)
                {
                    var packer = new MaxRectsBinPack(size, size, false);
                    bool fits = true;
                    result.Clear();
                    for (int i = 0; i < data.Count; i++)
                    {
                        var rect = packer.Insert(data[i]._region.width, data[i]._region.height, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule);
                        if (rect.width == 0)
                        {
                            size *= 2;
                            fits = false;
                            break;
                        }
                        else
                        {
                            data[i]._region.left = rect.x;
                            data[i]._region.top = rect.y;
                            result.Add(data[i]);
                        }
                    }
                    if (fits)
                        break;
                }
                Width = Height = size;
            }
            else
            {
                if ((preferredWidth & (preferredWidth - 1)) != 0 || (preferredHeight & (preferredHeight - 1)) != 0)
                    throw new Exception("Size values have to be a power of 2");

                Width = preferredWidth.Value;
                Height = preferredHeight.Value;

                int curPage = 0;
                while (data.Count > 0)
                {
                    var packer = new MaxRectsBinPack(Width, Height, false);
                    var toRemove = new List<PackerRegionData>();
                    for (int i = 0; i < data.Count; i++)
                    {
                        var rect = packer.Insert(data[i]._region.width, data[i]._region.height, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule);
                        if (rect.width != 0)
                        {
                            data[i]._region.left = rect.x;
                            data[i]._region.top = rect.y;
                            data[i]._region.page = curPage;
                            toRemove.Add(data[i]);
                            result.Add(data[i]);
                        }
                    }
                    if (toRemove.Count == 0)
                        throw new Exception("Can't fit a region inside the page");
                    foreach(PackerRegionData prd in toRemove)
                    {
                        data.Remove(prd);
                    }
                    ++curPage;
                }
            }
            return result;
        }

        public BinTexturePacker(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }
    }
}
