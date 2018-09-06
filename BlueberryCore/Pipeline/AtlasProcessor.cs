using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.IO;

namespace BlueberryCore.Pipeline
{
    [ContentProcessor(DisplayName = "TextureAtlas processor - BlueberryEngine")]
    public class AtlasProcessor : ContentProcessor<FileStream, TextureAtlas>
    {
        public override TextureAtlas Process(FileStream input, ContentProcessorContext context)
        {
            try
            {
                context.Logger.LogMessage("Processing");

                String name = null;
                var result = new TextureAtlas();

                using(StreamReader reader = new StreamReader(input)) { 
                    while(true)
                    {
                        String line = reader.ReadLine();
                        if (line == null)
                            break;
                        if (line.Trim().Length == 0)
                            continue;
                        else if (name == null)
                        {
                            name = line;
                            
                            result.assetName = name;

                            float width = 0, height = 0;
                            if (ReadArguments(reader) == 2)
                            {
                                width = Int32.Parse(arguments[0]);
                                height = Int32.Parse(arguments[1]);
                                ReadArguments(reader);
                            }
                            String format = arguments[0];

                            ReadArguments(reader);
                            String minFilter = arguments[0];
                            String maxFilter = arguments[1];

                            String repeat = ReadValue(reader);
                        }
                        else
                        {
                            bool rotate = Boolean.Parse(ReadValue(reader));

                            ReadArguments(reader);
                            int left = Int32.Parse(arguments[0]);
                            int top = Int32.Parse(arguments[1]);

                            ReadArguments(reader);
                            int width = Int32.Parse(arguments[0]);
                            int height = Int32.Parse(arguments[1]);

                            var region = new Region();
                            region.name = line;
                            region.rotate = rotate;
                            region.left = left;
                            region.top = top;
                            region.width = width;
                            region.height = height;

                            if (ReadArguments(reader) == 4)
                            {
                                region.splits = new int[] { Int32.Parse(arguments[0]), Int32.Parse(arguments[1]), Int32.Parse(arguments[2]), Int32.Parse(arguments[3]) };

                                if (ReadArguments(reader) == 4)
                                {
                                    region.pads = new int[] { Int32.Parse(arguments[0]), Int32.Parse(arguments[1]), Int32.Parse(arguments[2]), Int32.Parse(arguments[3]) };
                                    ReadArguments(reader);
                                }
                            }

                            region.originalWidth = Int32.Parse(arguments[0]);
                            region.originalHeight = Int32.Parse(arguments[1]);

                            ReadArguments(reader);
                            region.offsetX = Int32.Parse(arguments[0]);
                            region.offsetY = Int32.Parse(arguments[1]);

                            region.index = Int32.Parse(ReadValue(reader));

                            result._regions.Add(region);
                        }
                    }
                    reader.Close();
                }

                return result;

            }
            catch (Exception ex)
            {
                context.Logger.LogMessage("Error {0}", ex);
                throw;
            }
        }

        /// <summary>
        /// Holds line arguments;
        /// </summary>

        private String[] arguments = new String[4];

        /// <summary>
        /// Get arguments of line, libgdx code
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        private int ReadArguments(StreamReader reader)
        {
            String line = reader.ReadLine();
            int begin = line.IndexOf(':');
            if (begin == -1)
            {
                //exception
            }
            int i = 0, lastMatch = begin + 1;
            for(i = 0; i < 3; i++)
            {
                int comma = line.IndexOf(',', lastMatch);
                if (comma == -1)
                    break;
                arguments[i] = line.Substring(lastMatch, comma - lastMatch).Trim();
                lastMatch = comma + 1;
            }
            arguments[i] = line.Substring(lastMatch).Trim();
            return i + 1;
        }

        /// <summary>
        /// Get value from line
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        private String ReadValue(StreamReader reader)
        {
            String line = reader.ReadLine();
            int begin = line.IndexOf(':');
            if (begin == -1)
            {
                //exception
            }
            return line.Substring(begin + 1).Trim();
        }

    }
}
