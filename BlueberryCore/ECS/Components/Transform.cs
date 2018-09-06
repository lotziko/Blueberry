using Microsoft.Xna.Framework;

namespace BlueberryCore
{
    public class Transform
    {
        internal Vector2 _position;

        public Transform SetPosition(Vector2 position)
        {
            if (position == _position)
                return this;
            position = _position;
            //implement
            return this;
        }

        public Vector2 GetPosition() => _position;
    }
}
