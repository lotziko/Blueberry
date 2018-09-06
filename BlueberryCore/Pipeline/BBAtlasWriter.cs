using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace BlueberryCore.Pipeline
{
    [ContentTypeWriter]
    internal class BBAtlasWriter : ContentTypeWriter<AtlasContainer>
    {
        protected override void Write(ContentWriter output, AtlasContainer value)
        {
            var atlas = value.atlas;
            var textureBuffer = value.textureBuffer;
            
            output.Write(atlas.Count);

            for (int i = 0; i < atlas.Count; i++)
            {
                Region region = atlas._regions[i];
                output.Write(region.name);
                output.Write(region.rotate);

                output.Write(region.left);
                output.Write(region.top);

                output.Write(region.width);
                output.Write(region.height);

                if (region.splits == null)
                {
                    output.Write(false);
                }
                else
                {
                    output.Write(true);
                    output.Write(region.splits[0]);
                    output.Write(region.splits[1]);
                    output.Write(region.splits[2]);
                    output.Write(region.splits[3]);
                }

                if (region.pads == null)
                {
                    output.Write(false);
                }
                else
                {
                    output.Write(true);
                    output.Write(region.pads[0]);
                    output.Write(region.pads[1]);
                    output.Write(region.pads[2]);
                    output.Write(region.pads[3]);
                }

                output.Write(region.originalWidth);
                output.Write(region.originalHeight);

                output.Write(region.offsetX);
                output.Write(region.offsetY);

                output.Write(region.index);
            }

            output.Write(textureBuffer.Count);

            var tempFilePath = Environment.CurrentDirectory + "/temp.temp";

            foreach(byte[] buffer in textureBuffer)
            {
                //change bytes order to create a bitmap
                byte temp;
                for(long i = 0; i < buffer.LongLength; i += 4)
                {
                    temp = buffer[i];
                    buffer[i] = buffer[i + 2];
                    buffer[i + 2] = temp;
                }
                var bitmap = new System.Drawing.Bitmap(value.textureWidth, value.textureHeight, PixelFormat.Format32bppArgb);
                var bitmapВata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(buffer, 0, bitmapВata.Scan0, buffer.Length);
                bitmap.UnlockBits(bitmapВata);
                bitmap.Save(tempFilePath);

                var textureImporter = new TextureImporter();
                TextureContent content = textureImporter.Import(tempFilePath, null);

                output.WriteObject(content);
                bitmap.Dispose();
            }
            
            //File.Delete(tempFilePath);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(TextureAtlas).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(BBAtlasReader).AssemblyQualifiedName;
        }
    }
}
