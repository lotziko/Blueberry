﻿using Blueberry;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlueberryOpenTK.PipelineTools
{
    [ContentImporter(".bba")]
    public class BBAtlasImporter : ContentImporter<TextureAtlas>
    {
        public override TextureAtlas Import(string filename)
        {
            try
            {
                var stream = new FileStream(filename, FileMode.Open);
                using (BinaryReader br = new BinaryReader(stream))
                {
                    var atlas = new TextureAtlas
                    {
                        regions = new List<Region>()
                    };
                    int pagesCount = br.ReadInt32();
                    for(int i = 0; i < pagesCount; i++)
                    {
                        int regionCount = br.ReadInt32();
                        for(int j = 0; j < regionCount; j++)
                        {
                            var region = new Region()
                            {
                                name = br.ReadString(),
                                rotate = br.ReadBoolean(),
                                left = br.ReadInt32(),
                                top = br.ReadInt32(),
                                width = br.ReadInt32(),
                                height = br.ReadInt32(),
                                page = i
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

                            atlas.regions.Add(region);
                        }

                        int width = br.ReadInt32(), height = br.ReadInt32(), compressedSize = br.ReadInt32();
                        byte[] buffer = new byte[compressedSize];
                        for (int j = 0; j < compressedSize; j++)
                        {
                            buffer[j] = br.ReadByte();
                        }
                        buffer = Data.Decompress(buffer);
                        atlas.texture.Add(new Texture2D(buffer, width, height));
                    }

                    br.Close();
                    stream.Close();
                    return atlas;
                }
            }
            catch (Exception ex)
            {
                //context.Logger.LogMessage("Error {0}", ex);
                throw;
            }
        }
    }
}
