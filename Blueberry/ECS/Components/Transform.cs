
namespace Blueberry
{
    public class Transform
    {
        internal Vec2 _position;

        public Transform SetPosition(Vec2 position)
        {
            if (position == _position)
                return this;
            position = _position;
            //implement
            return this;
        }

        public Vec2 GetPosition() => _position;
    }
}
