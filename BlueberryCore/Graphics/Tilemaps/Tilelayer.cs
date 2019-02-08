using Newtonsoft.Json;
using System;

namespace BlueberryCore.Tilemaps
{
    public class Tilelayer
    {
        [JsonProperty]
        internal int[,] tileIDs;
        [JsonProperty]
        internal String name;
        internal Tile[,] tileObjects;
        [JsonProperty]
        internal int width, height, cellWidth, cellHeight;

        #region Getters

        public String GetName() => name;

        public int GetWidth() => width;

        public int GetHeight() => height;

        public int GetCellWidth() => cellWidth;

        public int GetCellHeight() => cellHeight;

        public int[,] GetTileIDs() => tileIDs;

        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return tileObjects[x, y];
            }
            return null;
        }

        #endregion

        public void SetTile(int x, int y, Tile tile)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                tileObjects[x, y] = tile;
            }
        }

        public void Draw(Graphics graphics, float x, float y)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (tileObjects[i, j] != null)
                    {
                        tileObjects[i, j].Draw(graphics, x + i * cellWidth, y + j * cellHeight);
                    }
                }
            }
        }

        public void FillRectangle(int x1, int y1, int x2, int y2, Tile tile)
        {
            for (int i = Math.Max(0, x1); i < Math.Max(width, x2); i++)
            {
                for (int j = Math.Max(0, y1); j < Math.Max(height, y2); j++)
                {
                    tileObjects[i, j] = tile;
                }
            }
        }

        public void Resize(int newWidth, int newHeight)
        {
            Tile[,] newObjects = new Tile[newWidth, newHeight];
            for (int i = 0; i < newWidth; i++)
            {
                for (int j = 0; j < newHeight; j++)
                {
                    newObjects[i, j] = tileObjects[SimpleMath.FloorDiv(width * i, newWidth), SimpleMath.FloorDiv(height * j, newHeight)];
                }
            }
            tileObjects = newObjects;
            width = newWidth;
            height = newHeight;
        }

        public void ReplaceTilesWithIDs()
        {
            tileIDs = new Int32[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (tileObjects[i, j] != null)
                        tileIDs[i, j] = tileObjects[i, j].GetID();
                }
            }
        }

        public void ReplaceIDsWithTiles(TileDictionary dictionary)
        {
            tileObjects = new Tile[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (tileIDs[i, j] != 0)
                        tileObjects[i, j] = dictionary[tileIDs[i, j]];
                }
            }
        }

        public bool PointCollide(int x, int y)
        {
            if ((x = SimpleMath.FloorDiv(x, cellWidth)) > 0 && (y = SimpleMath.FloorDiv(y, cellHeight)) > 0 && x < width && y < height)
            {
                if (tileObjects[x, y] != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool PointCollide(double x, double y)
        {
            int x1, y1;
            if ((x1 = SimpleMath.FloorDiv((int)x, cellWidth)) >= 0 && (y1 = SimpleMath.FloorDiv((int)y, cellHeight)) >= 0 && x1 < width && y1 < height)
            {
                if (tileObjects[x1, y1] != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool LineCollide(double x1, double y1, double x2, double y2)
        {
            double angleRadians = Math.Atan2(y2 - y1, x2 - x1);
            double xOffset = Math.Cos(angleRadians) * cellWidth;
            double yOffset = Math.Sin(angleRadians) * cellHeight;
            if (xOffset > 0)
            {
                double x, y;
                for (x = x1, y = y2; x < x2; x += xOffset, y += yOffset)
                {
                    if (PointCollide(x, y))
                    {
                        return true;
                    }
                }
            }
            else
            {
                double x, y;
                for (x = x1, y = y2; x > x2; x += xOffset, y += yOffset)
                {
                    if (PointCollide(x, y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Constructor
        /// </summary>

        internal Tilelayer() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="cellWidth"></param>
        /// <param name="cellHeight"></param>
        /// <param name="tileObjects"></param>

        public Tilelayer(String name, int width, int height, int cellWidth, int cellHeight, Tile[,] tileObjects)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            if (tileObjects != null)
            {
                this.tileObjects = tileObjects;
            }
            else
            {
                this.tileObjects = new Tile[width, height];
            }
        }
    }
}
