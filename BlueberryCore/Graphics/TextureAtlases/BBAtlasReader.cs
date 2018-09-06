using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BlueberryCore.TextureAtlases
{
    class BBAtlasReader : ContentTypeReader<TextureAtlas>
    {
        protected override TextureAtlas Read(ContentReader input, TextureAtlas existingInstance)
        {
            var atlas = new TextureAtlas();
            atlas._regions = new List<Region>();
            int count = input.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var region = new Region()
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

            int textureCount = input.ReadInt32();
            atlas.texture = new Texture2D[textureCount];
            for(int i = 0; i < textureCount; i++)
            {
                var texture = input.ReadObject<Texture2D>();
                Color[] data = new Color[texture.Width * texture.Height];
                texture.GetData(data);
                for (int j = 0; j != data.Length; ++j)
                    data[j] = Color.FromNonPremultiplied(data[j].ToVector4());
                texture.SetData(data);

                atlas.texture[i] = texture;
            }
            return atlas;
        }
    }
}
