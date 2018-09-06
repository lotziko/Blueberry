

using Microsoft.Xna.Framework;

namespace BlueberryCore
{
    public class Mover : Component
    {
        public virtual void Move(Vector2 motion)
        {
            entity.transform._position += motion;
        }
    }
}
