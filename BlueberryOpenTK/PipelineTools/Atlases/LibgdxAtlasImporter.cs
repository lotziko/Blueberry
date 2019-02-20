using Blueberry;
using System.IO;

namespace BlueberryOpenTK.PipelineTools
{
    [ContentImporter(".atlas")]
    public class LibgdxAtlasImporter : ContentImporter<TextureAtlas>
    {
        public override TextureAtlas Import(string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            var result = new TextureAtlas();
            using (var reader = new StreamReader(filename))
            {
                Texture2D pageImage = null;
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    if (line.Trim().Length == 0)
                        pageImage = null;
                    else if (pageImage == null)
                    {
                        var imagePath = directory + "/" + line;

                        float width = 0, height = 0;
                        if (ReadArguments(reader) == 2)
                        { // size is only optional for an atlas packed with an old TexturePacker.
                            width = int.Parse(arguments[0]);
                            height = int.Parse(arguments[1]);
                            ReadArguments(reader);
                        }
                        var format = arguments[0];//Format.valueOf(tuple[0]);

                        ReadArguments(reader);
                        var min = arguments[0];//TextureFilter.valueOf(tuple[0]);
                        var max = arguments[1];//TextureFilter.valueOf(tuple[1]);

                        var direction = ReadValue(reader);
                        //var repeatX = ClampToEdge;
                        //var repeatY = ClampToEdge;
                        //if (direction.equals("x"))
                        //    repeatX = Repeat;
                        //else if (direction.equals("y"))
                        //    repeatY = Repeat;
                        //else if (direction.equals("xy"))
                        //{
                        //    repeatX = Repeat;
                        //    repeatY = Repeat;
                        //}

                        pageImage = Texture2D.LoadFromFile(imagePath);
                        result.texture.Add(pageImage);
                    }
                    else
                    {
                        bool rotate = bool.Parse(ReadValue(reader));

                        ReadArguments(reader);
                        int left = int.Parse(arguments[0]);
                        int top = int.Parse(arguments[1]);

                        ReadArguments(reader);
                        int width = int.Parse(arguments[0]);
                        int height = int.Parse(arguments[1]);

                        var region = new Region
                        {
                            page = result.texture.IndexOf(pageImage),
                            name = line,
                            rotate = rotate,
                            left = left,
                            top = top,
                            width = width,
                            height = height
                        };

                        if (ReadArguments(reader) == 4)
                        {
                            region.splits = new int[] { int.Parse(arguments[0]), int.Parse(arguments[1]), int.Parse(arguments[2]), int.Parse(arguments[3]) };

                            if (ReadArguments(reader) == 4)
                            {
                                region.pads = new int[] { int.Parse(arguments[0]), int.Parse(arguments[1]), int.Parse(arguments[2]), int.Parse(arguments[3]) };
                                ReadArguments(reader);
                            }
                        }

                        region.originalWidth = int.Parse(arguments[0]);
                        region.originalHeight = int.Parse(arguments[1]);

                        ReadArguments(reader);
                        region.offsetX = int.Parse(arguments[0]);
                        region.offsetY = int.Parse(arguments[1]);

                        region.index = int.Parse(ReadValue(reader));

                        result.regions.Add(region);
                    }
                }

                reader.Close();
            }
            return result;
        }

        /// <summary>
        /// Holds line arguments;
        /// </summary>

        private readonly string[] arguments = new string[4];

        /// <summary>
        /// Get arguments of line, libgdx code
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        private int ReadArguments(StreamReader reader)
        {
            var line = reader.ReadLine();
            int begin = line.IndexOf(':');
            if (begin == -1)
            {
                //exception
            }
            int i = 0, lastMatch = begin + 1;
            for (i = 0; i < 3; i++)
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

        private string ReadValue(StreamReader reader)
        {
            var line = reader.ReadLine();
            int begin = line.IndexOf(':');
            if (begin == -1)
            {
                //exception
            }
            return line.Substring(begin + 1).Trim();
        }
    }
}
