using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.DataTools
{
    public class MaxRectsBinPack
    {
        protected int binWidth, binHeight;
        protected readonly List<PackerRect> usedRectangles = new List<PackerRect>();
        protected readonly List<PackerRect> freeRectangles = new List<PackerRect>();

        public MaxRectsBinPack()
        {

        }

        public MaxRectsBinPack(int width, int height)
        {
            Init(width, height);
        }

        public void Init(int width, int height)
        {
            binWidth = width;
            binHeight = height;

            usedRectangles.Clear();
            freeRectangles.Clear();
            PackerRect n = new PackerRect
            {
                x = 0,
                y = 0,
                width = width,
                height = height
            };
            freeRectangles.Add(n);
        }

        public PackerRect Insert(int width, int height, FreeRectChoiceHeuristic method)
        {
            return Insert(new PackerRect() { width = width, height = height }, method);
        }

        /** Packs a single image. Order is defined externally. */
        public PackerRect Insert(PackerRect rect, FreeRectChoiceHeuristic method)
        {
            PackerRect newNode = ScoreRect(rect, method);
            if (newNode.height == 0) return null;

            int numRectanglesToProcess = freeRectangles.Count;
            for (int i = 0; i < numRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(freeRectangles[i], newNode))
                {
                    freeRectangles.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            PackerRect bestNode = new PackerRect();
            bestNode = newNode;

            usedRectangles.Add(bestNode);
            return bestNode;
        }
        
        protected void PlaceRect(PackerRect node)
        {
            int numRectanglesToProcess = freeRectangles.Count;
            for (int i = 0; i < numRectanglesToProcess; i++)
            {
                if (SplitFreeNode(freeRectangles[i], node))
                {
                    freeRectangles.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            usedRectangles.Add(node);
        }

        protected PackerRect ScoreRect(PackerRect rect, FreeRectChoiceHeuristic method)
        {
            int width = rect.width;
            int height = rect.height;
            int rotatedWidth = 0;//height - settings.paddingY + settings.paddingX;
            int rotatedHeight = 0;//width - settings.paddingX + settings.paddingY;
            bool rotate = false;//rect.canRotate && settings.rotation;

            PackerRect newNode = null;
            switch (method)
            {
                case FreeRectChoiceHeuristic.BestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, rotatedWidth, rotatedHeight, rotate);
                    break;
                case FreeRectChoiceHeuristic.BottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, rotatedWidth, rotatedHeight, rotate);
                    break;
                case FreeRectChoiceHeuristic.ContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, rotatedWidth, rotatedHeight, rotate);
                    newNode.score1 = -newNode.score1; // Reverse since we are minimizing, but for contact point score bigger is better.
                    break;
                case FreeRectChoiceHeuristic.BestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, rotatedWidth, rotatedHeight, rotate);
                    break;
                case FreeRectChoiceHeuristic.BestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, rotatedWidth, rotatedHeight, rotate);
                    break;
            }

            // Cannot fit the current rectangle.
            if (newNode.height == 0)
            {
                newNode.score1 = int.MaxValue;
                newNode.score2 = int.MaxValue;
            }

            return newNode;
        }

        // / Computes the ratio of used surface area.
        protected float GetOccupancy()
        {
            int usedSurfaceArea = 0;
            for (int i = 0; i < usedRectangles.Count; i++)
                usedSurfaceArea += usedRectangles[i].width * usedRectangles[i].height;
            return (float)usedSurfaceArea / (binWidth * binHeight);
        }

        protected PackerRect FindPositionForNewNodeBottomLeft(int width, int height, int rotatedWidth, int rotatedHeight, bool rotate)
        {
            PackerRect bestNode = new PackerRect
            {
                score1 = int.MaxValue // best y, score2 is best x
            };

            for (int i = 0; i < freeRectangles.Count; i++)
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int topSideY = freeRectangles[i].y + height;
                    if (topSideY < bestNode.score1 || (topSideY == bestNode.score1 && freeRectangles[i].x < bestNode.score2))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestNode.score1 = topSideY;
                        bestNode.score2 = freeRectangles[i].x;
                        bestNode.rotated = false;
                    }
                }
                if (rotate && freeRectangles[i].width >= rotatedWidth && freeRectangles[i].height >= rotatedHeight)
                {
                    int topSideY = freeRectangles[i].y + rotatedHeight;
                    if (topSideY < bestNode.score1 || (topSideY == bestNode.score1 && freeRectangles[i].x < bestNode.score2))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = rotatedWidth;
                        bestNode.height = rotatedHeight;
                        bestNode.score1 = topSideY;
                        bestNode.score2 = freeRectangles[i].x;
                        bestNode.rotated = true;
                    }
                }
            }
            return bestNode;
        }

        protected PackerRect FindPositionForNewNodeBestShortSideFit(int width, int height, int rotatedWidth, int rotatedHeight,
            bool rotate)
        {
            var bestNode = new PackerRect
            {
                score1 = int.MaxValue
            };

            for (int i = 0; i < freeRectangles.Count; i++)
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestNode.score1 || (shortSideFit == bestNode.score1 && longSideFit < bestNode.score2))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestNode.score1 = shortSideFit;
                        bestNode.score2 = longSideFit;
                        bestNode.rotated = false;
                    }
                }

                if (rotate && freeRectangles[i].width >= rotatedWidth && freeRectangles[i].height >= rotatedHeight)
                {
                    int flippedLeftoverHoriz = Math.Abs(freeRectangles[i].width - rotatedWidth);
                    int flippedLeftoverVert = Math.Abs(freeRectangles[i].height - rotatedHeight);
                    int flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                    int flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                    if (flippedShortSideFit < bestNode.score1
                        || (flippedShortSideFit == bestNode.score1 && flippedLongSideFit < bestNode.score2))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = rotatedWidth;
                        bestNode.height = rotatedHeight;
                        bestNode.score1 = flippedShortSideFit;
                        bestNode.score2 = flippedLongSideFit;
                        bestNode.rotated = true;
                    }
                }
            }

            return bestNode;
        }

        protected PackerRect FindPositionForNewNodeBestLongSideFit(int width, int height, int rotatedWidth, int rotatedHeight,
            bool rotate)
        {
            PackerRect bestNode = new PackerRect
            {
                score2 = int.MaxValue
            };

            for (int i = 0; i < freeRectangles.Count; i++)
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestNode.score2 || (longSideFit == bestNode.score2 && shortSideFit < bestNode.score1))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestNode.score1 = shortSideFit;
                        bestNode.score2 = longSideFit;
                        bestNode.rotated = false;
                    }
                }

                if (rotate && freeRectangles[i].width >= rotatedWidth && freeRectangles[i].height >= rotatedHeight)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - rotatedWidth);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - rotatedHeight);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestNode.score2 || (longSideFit == bestNode.score2 && shortSideFit < bestNode.score1))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = rotatedWidth;
                        bestNode.height = rotatedHeight;
                        bestNode.score1 = shortSideFit;
                        bestNode.score2 = longSideFit;
                        bestNode.rotated = true;
                    }
                }
            }
            return bestNode;
        }

        protected PackerRect FindPositionForNewNodeBestAreaFit(int width, int height, int rotatedWidth, int rotatedHeight,
            bool rotate)
        {
            PackerRect bestNode = new PackerRect
            {
                score1 = int.MaxValue // best area fit, score2 is best short side fit
            };

            for (int i = 0; i < freeRectangles.Count; i++)
            {
                int areaFit = freeRectangles[i].width * freeRectangles[i].height - width * height;

                // Try to place the rectangle in upright (non-rotated) orientation.
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestNode.score1 || (areaFit == bestNode.score1 && shortSideFit < bestNode.score2))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestNode.score2 = shortSideFit;
                        bestNode.score1 = areaFit;
                        bestNode.rotated = false;
                    }
                }

                if (rotate && freeRectangles[i].width >= rotatedWidth && freeRectangles[i].height >= rotatedHeight)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - rotatedWidth);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - rotatedHeight);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestNode.score1 || (areaFit == bestNode.score1 && shortSideFit < bestNode.score2))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = rotatedWidth;
                        bestNode.height = rotatedHeight;
                        bestNode.score2 = shortSideFit;
                        bestNode.score1 = areaFit;
                        bestNode.rotated = true;
                    }
                }
            }
            return bestNode;
        }

        // / Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
        protected int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
        {
            if (i1end < i2start || i2end < i1start) return 0;
            return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
        }

        protected int ContactPointScoreNode(int x, int y, int width, int height)
        {
            int score = 0;

            if (x == 0 || x + width == binWidth) score += height;
            if (y == 0 || y + height == binHeight) score += width;

            var usedRectangles = this.usedRectangles;
            for (int i = 0, n = usedRectangles.Count; i < n; i++)
            {
                PackerRect rect = usedRectangles[i];
                if (rect.x == x + width || rect.x + rect.width == x)
                    score += CommonIntervalLength(rect.y, rect.y + rect.height, y, y + height);
                if (rect.y == y + height || rect.y + rect.height == y)
                    score += CommonIntervalLength(rect.x, rect.x + rect.width, x, x + width);
            }
            return score;
        }

        protected PackerRect FindPositionForNewNodeContactPoint(int width, int height, int rotatedWidth, int rotatedHeight,
            bool rotate)
        {

            PackerRect bestNode = new PackerRect
            {
                score1 = -1 // best contact score
            };

            var freeRectangles = this.freeRectangles;
            for (int i = 0, n = freeRectangles.Count; i < n; i++)
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                PackerRect free = freeRectangles[i];
                if (free.width >= width && free.height >= height)
                {
                    int score = ContactPointScoreNode(free.x, free.y, width, height);
                    if (score > bestNode.score1)
                    {
                        bestNode.x = free.x;
                        bestNode.y = free.y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestNode.score1 = score;
                        bestNode.rotated = false;
                    }
                }
                if (rotate && free.width >= rotatedWidth && free.height >= rotatedHeight)
                {
                    int score = ContactPointScoreNode(free.x, free.y, rotatedWidth, rotatedHeight);
                    if (score > bestNode.score1)
                    {
                        bestNode.x = free.x;
                        bestNode.y = free.y;
                        bestNode.width = rotatedWidth;
                        bestNode.height = rotatedHeight;
                        bestNode.score1 = score;
                        bestNode.rotated = true;
                    }
                }
            }
            return bestNode;
        }

        protected bool SplitFreeNode(PackerRect freeNode, PackerRect usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x
                || usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y) return false;

            if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
            {
                // New node at the top side of the used node.
                if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
                {
                    PackerRect newNode = new PackerRect(freeNode);
                    newNode.height = usedNode.y - newNode.y;
                    freeRectangles.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
                {
                    PackerRect newNode = new PackerRect(freeNode)
                    {
                        y = usedNode.y + usedNode.height,
                        height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height)
                    };
                    freeRectangles.Add(newNode);
                }
            }

            if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
            {
                // New node at the left side of the used node.
                if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
                {
                    PackerRect newNode = new PackerRect(freeNode);
                    newNode.width = usedNode.x - newNode.x;
                    freeRectangles.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
                {
                    PackerRect newNode = new PackerRect(freeNode)
                    {
                        x = usedNode.x + usedNode.width,
                        width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width)
                    };
                    freeRectangles.Add(newNode);
                }
            }

            return true;
        }

        protected void PruneFreeList()
        {
            // Go through each pair and remove any rectangle that is redundant.
            var freeRectangles = this.freeRectangles;
            for (int i = 0, n = freeRectangles.Count; i < n; i++)
                for (int j = i + 1; j < n; ++j)
                {
                    PackerRect rect1 = freeRectangles[i];
                    PackerRect rect2 = freeRectangles[j];
                    if (IsContainedIn(rect1, rect2))
                    {
                        freeRectangles.RemoveAt(i);
                        --i;
                        --n;
                        break;
                    }
                    if (IsContainedIn(rect2, rect1))
                    {
                        freeRectangles.RemoveAt(j);
                        --j;
                        --n;
                    }
                }
        }

        protected bool IsContainedIn(PackerRect a, PackerRect b)
        {
            return a.x >= b.x && a.y >= b.y && a.x + a.width <= b.x + b.width && a.y + a.height <= b.y + b.height;
        }
    }

    public class PackerRect : ICloneable
    {
        public int x, y, width, height, score1, score2;
        public bool rotated;

        public PackerRect()
        {

        }

        public PackerRect(PackerRect src)
        {
            x = src.x;
            y = src.y;
            width = src.width;
            height = src.height;
            score1 = src.score1;
            score2 = src.score2;
            rotated = src.rotated;
        }

        public virtual object Clone()
        {
            return new PackerRect()
            {
                x = x,
                y = y,
                width = width,
                height = height,
                score1 = score1,
                score2 = score2,
                rotated = rotated,
            };
        }
    }

    public enum FreeRectChoiceHeuristic
    {
        BestShortSideFit, ///< -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
        BestLongSideFit, ///< -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
        BestAreaFit, ///< -BAF: Positions the rectangle into the smallest free rect into which it fits.
        BottomLeftRule, ///< -BL: Does the Tetris placement.
        ContactPointRule ///< -CP: Choosest the placement where the rectangle touches other rects as much as possible.
    };
}
