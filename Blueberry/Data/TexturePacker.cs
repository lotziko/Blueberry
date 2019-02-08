using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blueberry.DataTools
{
    public class TexturePacker
    {
        private readonly FreeRectChoiceHeuristic[] methods = Enum.GetValues(typeof(FreeRectChoiceHeuristic)).Cast<FreeRectChoiceHeuristic>().ToArray();
        private readonly Settings settings;
        private readonly TextureBinPacker maxRects;
        private readonly ImageProcessor processor;

        public TexturePacker(Settings settings)
        {
            this.settings = settings;
            maxRects = new TextureBinPacker();
            processor = new ImageProcessor(settings);
            if (settings.minWidth > settings.maxWidth) throw new Exception("Page Min width cannot be higher than Max width.");
            if (settings.minHeight > settings.maxHeight)
                throw new Exception("Page Min height cannot be higher than Max height.");
        }

        public void Pack(string input, string output, string name, bool recursive)
        {
            if (!Directory.Exists(input))
                throw new Exception("Input directory do not exists");

            if (!Directory.Exists(output))
                throw new Exception("Output directory do not exists");

            if (!output.EndsWith(@"\"))
            {
                output += @"\";
            }

            var areas = new List<TextureArea>();
            foreach(string file in Directory.EnumerateFiles(input, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                if (Path.GetExtension(file) == ".png")
                {
                    areas.Add(processor.CreateArea(file));
                }
            }
            var pages = Pack(areas);
            WriteImages(output, name, pages);
            WritePackFile(output, name, pages);
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
            var file = output + "\\" + name + ".atlas";
            using (var writer = new StreamWriter(file))
            {
                foreach(var page in pages)
                {
                    writer.Write("\n" + page.imageName + ".png" + "\n");
                    writer.Write("size: " + page.imageWidth + "," + page.imageHeight + "\n");
                    writer.Write("format: " /*+ settings.format*/ + "\n");
                    writer.Write("filter: " /*+ settings.filterMin + "," + settings.filterMag*/ + "\n");
                    writer.Write("repeat: " /*+ getRepeatValue()*/ + "\n");

                    foreach(var area in page.outputRects)
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
            writer.Write("  xy: " + (page.x + rect.x) + ", " + (page.y + page.height - rect.y - (rect.height - settings.paddingY)) + "\n");

            writer.Write("  size: " + rect.width + ", " + rect.height + "\n");
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

        public List<Page> Pack(List<TextureArea> inputRects)
        {
            int n = inputRects.Count;
            for (int i = 0; i < n; i++)
            {
                var rect = inputRects[i];
                rect.width += settings.paddingX;
                rect.height += settings.paddingY;
            }

            if (settings.fast)
            {
                if (settings.rotation)
                {
                    // Sort by longest side if rotation is enabled.
                    inputRects.Sort(new RotationComparer());
                }
            }
            else
            {
                // Sort only by width (largest to smallest) if rotation is disabled.
                inputRects.Sort(new NonRotationComparer());
            }

            var pages = new List<Page>();
            while (inputRects.Count > 0)
            {
                //progress.count = n - inputRects.Count + 1;
                //if (progress.update(progress.count, n)) break;

                Page result = PackPage(inputRects);
                pages.Add(result);
                inputRects = result.remainingRects;
            }
            return pages;
        }

        private Page PackPage(List<TextureArea> inputRects)
        {
            int paddingX = settings.paddingX, paddingY = settings.paddingY;
            float maxWidth = settings.maxWidth, maxHeight = settings.maxHeight;
            bool edgePadX = false, edgePadY = false;
            if (settings.edgePadding)
            {
                if (settings.duplicatePadding)
                {
                    maxWidth -= paddingX;
                    maxHeight -= paddingY;
                }
                else
                {
                    maxWidth -= paddingX * 2;
                    maxHeight -= paddingY * 2;
                }
                edgePadX = paddingX > 0;
                edgePadY = paddingY > 0;
            }

            // Find Min size.
            int minWidth = int.MaxValue, minHeight = int.MaxValue;
            for (int i = 0, nn = inputRects.Count; i < nn; i++)
            {
                TextureArea rect = inputRects[i];
                int width = rect.width - paddingX, height = rect.height - paddingY;
                minWidth = Math.Min(minWidth, width);
                minHeight = Math.Min(minHeight, height);
                if (settings.rotation)
                {
                    if ((width > maxWidth || height > maxHeight) && (width > maxHeight || height > maxWidth))
                    {
                        var paddingMessage = (edgePadX || edgePadY) ? (" and edge padding " + paddingX + "*2," + paddingY + "*2") : "";
                        throw new Exception("Image does not fit with Max page size " + settings.maxWidth + "x" + settings.maxHeight
                            + paddingMessage + ": " + rect.name + "[" + width + "," + height + "]");
                    }
                }
                else
                {
                    if (width > maxWidth)
                    {
                        var paddingMessage = edgePadX ? (" and X edge padding " + paddingX + "*2") : "";
                        throw new Exception("Image does not fit with Max page width " + settings.maxWidth + paddingMessage + ": "
                            + rect.name + "[" + width + "," + height + "]");
                    }
                    if (height > maxHeight && (!settings.rotation || width > maxHeight))
                    {
                        var paddingMessage = edgePadY ? (" and Y edge padding " + paddingY + "*2") : "";
                        throw new Exception("Image does not fit in Max page height " + settings.maxHeight + paddingMessage + ": "
                            + rect.name + "[" + width + "," + height + "]");
                    }
                }
            }
            minWidth = Math.Max(minWidth, settings.minWidth);
            minHeight = Math.Max(minHeight, settings.minHeight);

            // BinarySearch uses the Max size. Rects are packed with right and top padding, so the Max size is increased to match.
            // After packing the padding is subtracted from the page size.
            int adjustX = paddingX, adjustY = paddingY;
            if (settings.edgePadding)
            {
                if (settings.duplicatePadding)
                {
                    adjustX -= paddingX;
                    adjustY -= paddingY;
                }
                else
                {
                    adjustX -= paddingX * 2;
                    adjustY -= paddingY * 2;
                }
            }

            if (!settings.silent) Console.WriteLine("Packing");

            // Find the minimal page size that fits all rects.
            Page bestResult = null;
            if (settings.square)
            {
                int minSize = Math.Max(minWidth, minHeight);
                int maxSize = Math.Min(settings.maxWidth, settings.maxHeight);
                BinarySearch sizeSearch = new BinarySearch(minSize, maxSize, settings.fast ? 25 : 15, settings.pot,
                    settings.multipleOfFour);
                int size = sizeSearch.Reset(), i = 0;
                while (size != -1)
                {
                    Page result = PackAtSize(true, size + adjustX, size + adjustY, inputRects);
                    if (!settings.silent)
                    {
                        if (++i % 70 == 0) Console.WriteLine();
                        Console.Write(".");
                    }
                    bestResult = GetBest(bestResult, result);
                    size = sizeSearch.Next(result == null);
                }
                if (!settings.silent) Console.WriteLine();
                // Rects don't fit on one page. Fill a whole page and return.
                if (bestResult == null) bestResult = PackAtSize(false, maxSize + adjustX, maxSize + adjustY, inputRects);
                //sort.sort(bestResult.outputRects, rectComparator);
                bestResult.width = Math.Max(bestResult.width, bestResult.height) - paddingX;
                bestResult.height = Math.Max(bestResult.width, bestResult.height) - paddingY;
                return bestResult;
            }
            else
            {
                BinarySearch widthSearch = new BinarySearch(minWidth, settings.maxWidth, settings.fast ? 25 : 15, settings.pot,
                    settings.multipleOfFour);
                BinarySearch heightSearch = new BinarySearch(minHeight, settings.maxHeight, settings.fast ? 25 : 15, settings.pot,
                    settings.multipleOfFour);
                int width = widthSearch.Reset(), i = 0;
                int height = settings.square ? width : heightSearch.Reset();
                while (true)
                {
                    Page bestWidthResult = null;
                    while (width != -1)
                    {
                        Page result = PackAtSize(true, width + adjustX, height + adjustY, inputRects);
                        if (!settings.silent)
                        {
                            if (++i % 70 == 0) Console.WriteLine();
                            Console.Write(".");
                        }
                        bestWidthResult = GetBest(bestWidthResult, result);
                        width = widthSearch.Next(result == null);
                        if (settings.square) height = width;
                    }
                    bestResult = GetBest(bestResult, bestWidthResult);
                    if (settings.square) break;
                    height = heightSearch.Next(bestWidthResult == null);
                    if (height == -1) break;
                    width = widthSearch.Reset();
                }
                if (!settings.silent) Console.WriteLine();
                // Rects don't fit on one page. Fill a whole page and return.
                if (bestResult == null)
                    bestResult = PackAtSize(false, settings.maxWidth + adjustX, settings.maxHeight + adjustY, inputRects);
                //sort.sort(bestResult.outputRects, rectComparator);
                bestResult.width -= paddingX;
                bestResult.height -= paddingY;
                return bestResult;
            }
        }

        /** @param fully If true, the only results that pack all rects will be considered. If false, all results are considered, not
	 *           all rects may be packed. */
        private Page PackAtSize(bool fully, int width, int height, List<TextureArea> inputRects)
        {
            Page bestResult = null;
            for (int i = 0, n = methods.Length; i < n; i++)
            {
                maxRects.Init(width, height);
                Page result;
                if (!settings.fast)
                {
                    result = maxRects.Pack(inputRects, methods[i]);
                }
                else
                {
                    var remaining = new List<TextureArea>();
                    for (int ii = 0, nn = inputRects.Count; ii < nn; ii++)
                    {
                        TextureArea rect = inputRects[ii];
                        if (maxRects.Insert(rect, methods[i]) == null)
                        {
                            while (ii < nn)
                                remaining.Add(inputRects[ii++]);
                        }
                    }
                    result = maxRects.GetResult();
                    result.remainingRects = remaining;
                }
                if (fully && result.remainingRects.Count > 0) continue;
                if (result.outputRects.Count == 0) continue;
                bestResult = GetBest(bestResult, result);
            }
            return bestResult;
        }

        private Page GetBest(Page result1, Page result2)
        {
            if (result1 == null) return result2;
            if (result2 == null) return result1;
            return result1.occupancy > result2.occupancy ? result1 : result2;
        }

        #region Comparers

        private class RotationComparer : Comparer<TextureArea>
        {
            public override int Compare(TextureArea o1, TextureArea o2)
            {
                int n1 = o1.width > o1.height ? o1.width : o1.height;
                int n2 = o2.width > o2.height ? o2.width : o2.height;
                return n2 - n1;
            }
        }

        private class NonRotationComparer : Comparer<TextureArea>
        {
            public override int Compare(TextureArea o1, TextureArea o2)
            {
                return o2.width - o1.width;
            }
        }

        #endregion
    }

    public class Page
    {
        public string imageName;
        public List<TextureArea> outputRects, remainingRects;
        public float occupancy;
        public int x, y, width, height, imageWidth, imageHeight;
    }

    public class TextureArea : PackerRect
    {
        public string name;
        public ImageData data;
        public int index;
        public int[] splits;
        public int[] pads;
        public int offsetX, offsetY, originalWidth, originalHeight;
        private bool isPatch;

        public TextureArea() : base()
        {
            
        }

        public TextureArea(ImageData data, int left, int top, int newWidth, int newHeight, bool isPatch) : base()
        {
            this.data = data;
            offsetX = left;
            offsetY = top;
            width = newWidth;
            height = newHeight;
        }

        public TextureArea(TextureArea src) : base(src)
        {
            name = src.name;
            data = src.data;
        }

        public override object Clone()
        {
            return new TextureArea(this);
        }
    }

    internal class TextureBinPacker : MaxRectsBinPack
    {
        /** For each rectangle, packs each one then chooses the best and packs that. Slow! */
        public Page Pack(List<TextureArea> rects, FreeRectChoiceHeuristic method)
        {
            rects = new List<TextureArea>(rects);
            while (rects.Count > 0)
            {
                int bestRectIndex = -1;
                PackerRect bestNode = new PackerRect
                {
                    score1 = int.MaxValue,
                    score2 = int.MaxValue
                };

                // Find the next rectangle that packs best.
                for (int i = 0; i < rects.Count; i++)
                {
                    PackerRect newNode = ScoreRect(rects[i], method);
                    if (newNode.score1 < bestNode.score1 || (newNode.score1 == bestNode.score1 && newNode.score2 < bestNode.score2))
                    {
                        bestNode = (PackerRect)rects[i].Clone();
                        bestNode.score1 = newNode.score1;
                        bestNode.score2 = newNode.score2;
                        bestNode.x = newNode.x;
                        bestNode.y = newNode.y;
                        bestNode.width = newNode.width;
                        bestNode.height = newNode.height;
                        bestNode.rotated = newNode.rotated;
                        bestRectIndex = i;
                    }
                }

                if (bestRectIndex == -1) break;

                PlaceRect(bestNode);
                rects.RemoveAt(bestRectIndex);
            }

            Page result = GetResult();
            result.remainingRects = rects;
            return result;
        }

        public Page GetResult()
        {
            int w = 0, h = 0;
            for (int i = 0; i < usedRectangles.Count; i++)
            {
                PackerRect rect = usedRectangles[i];
                w = Math.Max(w, rect.x + rect.width);
                h = Math.Max(h, rect.y + rect.height);
            }
            Page result = new Page
            {
                outputRects = new List<TextureArea>(),
                occupancy = GetOccupancy(),
                width = w,
                height = h
            };
            foreach (var r in usedRectangles)
                result.outputRects.Add((TextureArea)r);
            return result;
        }
    }

    class BinarySearch
    {
        readonly bool pot, mod4;
        readonly int Min, Max, fuzziness;
        int low, high, current;

        public BinarySearch(int Min, int Max, int fuzziness, bool pot, bool mod4)
        {
            if (pot)
            {
                this.Min = (int)(Math.Log(MathF.NextPowerOfTwo(Min)) / Math.Log(2));
                this.Max = (int)(Math.Log(MathF.NextPowerOfTwo(Max)) / Math.Log(2));
            }
            else if (mod4)
            {
                this.Min = Min % 4 == 0 ? Min : Min + 4 - (Min % 4);
                this.Max = Max % 4 == 0 ? Max : Max + 4 - (Max % 4);
            }
            else
            {
                this.Min = Min;
                this.Max = Max;
            }
            this.fuzziness = pot ? 0 : fuzziness;
            this.pot = pot;
            this.mod4 = mod4;
        }

        public int Reset()
        {
            low = Min;
            high = Max;
            current = (int)((uint)(low + high) >> 1);
            if (pot) return (int)Math.Pow(2, current);
            if (mod4) return current % 4 == 0 ? current : current + 4 - (current % 4);
            return current;
        }

        public int Next(bool result)
        {
            if (low >= high) return -1;
            if (result)
                low = current + 1;
            else
                high = current - 1;
            current = (int)((uint)(low + high) >> 1);
            if (Math.Abs(low - high) < fuzziness) return -1;
            if (pot) return (int)Math.Pow(2, current);
            if (mod4) return current % 4 == 0 ? current : current + 4 - (current % 4);
            return current;
        }
    }

    public class Settings
    {
        public bool pot = true;
        public bool multipleOfFour;
        public int paddingX = 2, paddingY = 2;
        public bool edgePadding = true;
        public bool duplicatePadding = false;
        public bool rotation;
        public int minWidth = 16, minHeight = 16;
        public int maxWidth = 1024, maxHeight = 1024;
        public bool square = false;
        public bool stripWhitespaceX, stripWhitespaceY;
        public int alphaThreshold;
        //public TextureFilter filterMin = TextureFilter.Nearest, filterMag = TextureFilter.Nearest;
        //public TextureWrap wrapX = TextureWrap.ClampToEdge, wrapY = TextureWrap.ClampToEdge;
        //public Format format = Format.RGBA8888;
        //public boolean alias = true;
        public string outputFormat = "png";
        //public float jpegQuality = 0.9f;
        public bool ignoreBlankImages = true;
        public bool fast;
        public bool debug;
        public bool silent;
        public bool combineSubdirectories;
        public bool ignore;
        public bool flattenPaths;
        public bool premultiplyAlpha;
        public bool useIndexes = true;
        public bool bleed = true;
        public int bleedIterations = 2;
        public bool limitMemory = true;
        //public bool grid;
        //public float[] scale = { 1 };
        //public string[] scaleSuffix = { "" };
        //public Resampling[] scaleResampling = { Resampling.bicubic };
        //public string atlasExtension = ".atlas";

        public Settings()
        {
        }

        public Settings(Settings settings)
        {
            Set(settings);
        }

        public void Set(Settings settings)
        {
            fast = settings.fast;
            rotation = settings.rotation;
            pot = settings.pot;
            multipleOfFour = settings.multipleOfFour;
            minWidth = settings.minWidth;
            minHeight = settings.minHeight;
            maxWidth = settings.maxWidth;
            maxHeight = settings.maxHeight;
            paddingX = settings.paddingX;
            paddingY = settings.paddingY;
            edgePadding = settings.edgePadding;
            duplicatePadding = settings.duplicatePadding;
            alphaThreshold = settings.alphaThreshold;
            ignoreBlankImages = settings.ignoreBlankImages;
            stripWhitespaceX = settings.stripWhitespaceX;
            stripWhitespaceY = settings.stripWhitespaceY;
            //alias = settings.alias;
            //format = settings.format;
            //jpegQuality = settings.jpegQuality;
            outputFormat = settings.outputFormat;
            //filterMin = settings.filterMin;
            //filterMag = settings.filterMag;
            //wrapX = settings.wrapX;
            //wrapY = settings.wrapY;
            debug = settings.debug;
            silent = settings.silent;
            combineSubdirectories = settings.combineSubdirectories;
            ignore = settings.ignore;
            flattenPaths = settings.flattenPaths;
            premultiplyAlpha = settings.premultiplyAlpha;
            square = settings.square;
            useIndexes = settings.useIndexes;
            bleed = settings.bleed;
            bleedIterations = settings.bleedIterations;
            limitMemory = settings.limitMemory;
            //grid = settings.grid;
            //scale = Arrays.copyOf(settings.scale, settings.scale.length);
            //scaleSuffix = Arrays.copyOf(settings.scaleSuffix, settings.scaleSuffix.length);
            //scaleResampling = Arrays.copyOf(settings.scaleResampling, settings.scaleResampling.length);
            //atlasExtension = settings.atlasExtension;
        }
    }
}
