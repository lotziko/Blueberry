using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using BlueberryCore.DataTools;

namespace BlueberryCore.Pipeline
{
    [ContentProcessor(DisplayName = "BBAtlas processor - BlueberryEngine")]
    public class BBAtlasProcessor : ContentProcessor<FileStream, AtlasContainer>
    {
        public override AtlasContainer Process(FileStream input, ContentProcessorContext context)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(input))
                {
                    var atlas = new TextureAtlas
                    {
                        _regions = new List<Region>()
                    };
                    int regionCount = br.ReadInt32();
                    for (int i = 0; i < regionCount; i++)
                    {
                        var region = new Region()
                        {
                            name = br.ReadString(),
                            rotate = br.ReadBoolean(),
                            left = br.ReadInt32(),
                            top = br.ReadInt32(),
                            width = br.ReadInt32(),
                            height = br.ReadInt32()
                        };

                        if (br.ReadBoolean() == true)
                        {
                            region.splits = new int[] { br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32() };
                        }

                        if (br.ReadBoolean() == true)
                        {
                            region.pads = new int[] { br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32() };
                        }

                        region.originalWidth = br.ReadInt32();
                        region.originalHeight = br.ReadInt32();

                        region.offsetX = br.ReadInt32();
                        region.offsetY = br.ReadInt32();

                        region.index = br.ReadInt32();

                        atlas._regions.Add(region);
                    }

                    int textureCount = br.ReadInt32();
                    int width = br.ReadInt32(), height = br.ReadInt32();

                    var container = new AtlasContainer
                    {
                        atlas = atlas,
                        textureBuffer = new List<byte[]>(),
                        textureWidth = width,
                        textureHeight = height
                    };

                    for (int i = 0; i < textureCount; i++)
                    {
                        int compressedSize = br.ReadInt32();
                        byte[] buffer = new byte[compressedSize];
                        for (int j = 0; j < compressedSize; j++)
                        {
                            buffer[j] = br.ReadByte();
                        }
                        buffer = Data.Decompress(buffer);
                        container.textureBuffer.Add(buffer);
                    }

                    br.Close();
                    return container;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogMessage("Error {0}", ex);
                throw;
            }
        }
    }

    public struct AtlasContainer
    {
        public TextureAtlas atlas;
        public List<byte[]> textureBuffer;
        public int textureWidth, textureHeight;
    }
}
