using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore.TextureAtlases
{
    class AtlasReader : ContentTypeReader<TextureAtlas>
    {
        protected override TextureAtlas Read(ContentReader input, TextureAtlas existingInstance)
        {
            var textureName = input.ReadString();
            textureName = textureName.Substring(0, textureName.IndexOf('.'));
            var texture = input.ContentManager.Load<Texture2D>(textureName + ".png");
            var atlas = new TextureAtlas(texture)
            {
                assetName = textureName
            };

            int regionCount = input.ReadInt32();

            for (var i = 0; i < regionCount; i++)
            {
                Region region = new Region()
                {
                    name = input.ReadString(),
                    rotate = input.ReadBoolean(),
                    left = input.ReadInt32(),
                    top = input.ReadInt32(),
                    width = input.ReadInt32(),
                    height = input.ReadInt32()
                };

                if (input.ReadBoolean() == true)
                {
                    region.splits = new int[] { input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32() };
                }

                if (input.ReadBoolean() == true)
                {
                    region.pads = new int[] { input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32() };
                }

                region.originalWidth = input.ReadInt32();
                region.originalHeight = input.ReadInt32();

                region.offsetX = input.ReadInt32();
                region.offsetY = input.ReadInt32();

                region.index = input.ReadInt32();

                atlas._regions.Add(region);
            }

            return atlas;
        }
    }
}
