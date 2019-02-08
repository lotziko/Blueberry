
namespace Blueberry.UI
{
    public class RemoveElementAction : Action
    {
        private bool removed;

        public override bool Update(float delta)
        {
            if (!removed)
            {
                removed = true;
                target.Remove();
            }
            return true;
        }

        public override void Restart()
        {
            removed = false;
        }
    }
}
