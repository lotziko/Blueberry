using BlueberryCore.SMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.IO.Compression;

namespace BlueberryCore.DataTools
{
    class Data
    {
        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static Color[,] GetColorArrayFromTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }
            int width = texture.Width, height = texture.Height;
            var result = new Color[width, height];
            var buffer = new Color[width * height];
            texture.GetData(buffer);
            for(int i = 0; i < buffer.Length; i++)
            {
                result[SimpleMath.FloorMod(i, width), SimpleMath.FloorDiv(i, width)] = buffer[i];
            }
            return result;
        }

        /// <summary>
        /// Calculate transparent space around texture to trim
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="trimOffset"></param>
        /// <returns>Result int[4] array is like ninepatch array</returns>

        public static int[] CalculateTextureTrim(Color[,] texture, int trimOffset = 0)
        {
            var result = new int[4];

            int width = texture.GetLength(0);
            int height = texture.GetLength(1);

            //int left = 0;
            bool edgeFound = false;
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    if (texture[i, j] != Color.Transparent)
                    {
                        edgeFound = true;
                        break;
                    }
                }
                if (edgeFound)
                    break;
                ++result[0];
            }
            result[0] = SimpleMath.Clamp(result[0], 0, result[0] - trimOffset);

            //int right = 0;
            edgeFound = false;
            for (int i = width - 1; i >= 0; i--)
            {
                for (int j = 0; j < height; j++)
                {
                    if (texture[i, j] != Color.Transparent)
                    {
                        edgeFound = true;
                        break;
                    }
                }
                if (edgeFound)
                    break;
                ++result[1];
            }
            result[1] = SimpleMath.Clamp(result[1], 0, result[1] - trimOffset);

            //int top = 0;
            edgeFound = false;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (texture[i, j] != Color.Transparent)
                    {
                        edgeFound = true;
                        break;
                    }
                }
                if (edgeFound)
                    break;
                ++result[2];
            }
            result[2] = SimpleMath.Clamp(result[2], 0, result[2] - trimOffset);

            //int bottom = 0;
            edgeFound = false;
            for (int j = height - 1; j >= 0; j--)
            {
                for (int i = 0; i < width; i++)
                {
                    if (texture[i, j] != Color.Transparent)
                    {
                        edgeFound = true;
                        break;
                    }
                }
                if (edgeFound)
                    break;
                ++result[3];
            }
            result[3] = SimpleMath.Clamp(result[3], 0, result[3] - trimOffset);

            return result;
        }
    }
}
