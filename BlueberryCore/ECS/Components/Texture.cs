using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore
{
    public class Texture : RenderableComponent
    {
        private Texture2D texture;

        public override void Render(Graphics graphics, Camera camera)
        {
            graphics.Draw(texture, entity.transform._position);   
        }

        public Texture(string atlasName)
        {
            texture = Core.content.Load<TextureAtlas>(atlasName).texture[0];
        }
    }
}
