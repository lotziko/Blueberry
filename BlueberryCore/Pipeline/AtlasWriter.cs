using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace BlueberryCore.Pipeline
{
    [ContentTypeWriter]
    class AtlasWriter : ContentTypeWriter<TextureAtlas>
    {
        protected override void Write(ContentWriter output, TextureAtlas value)
        {
            output.Write(value.assetName);
            output.Write(value.Count);

            for(int i = 0; i < value.Count; i++)
            {
                var region = value._regions[i];
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
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(TextureAtlas).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AtlasReader).AssemblyQualifiedName;
        }
    }
}
