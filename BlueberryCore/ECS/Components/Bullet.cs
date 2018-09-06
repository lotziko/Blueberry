using Microsoft.Xna.Framework;

namespace BlueberryCore
{
    public class Bullet : UpdatableComponent
    {
        private Vector2 _motion;

        public override void Update()
        {
            entity.transform._position += _motion;
        }

        public Bullet(Vector2 motion)
        {
            _motion = motion;
        }
    }
}
