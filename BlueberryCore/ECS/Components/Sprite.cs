
using BlueberryCore.TextureAtlases;

namespace BlueberryCore
{
    public class Sprite : RenderableComponent
    {
        protected TextureRegion _texture;

        public override void Render(Graphics graphics, Camera camera)
        {
            _texture.Draw(graphics, entity.transform._position);
        }

        public Sprite(string atlasName, string spriteName)
        {
            var atlas = Core.content.Load<TextureAtlas>(atlasName);
            _texture = atlas.FindRegion(spriteName);
        }
    }
}
