
namespace BlueberryCore.UI.Actions
{
    public class RemoveListenerAction : Action
    {
        private IEventListener listener;
        private bool capture;

        public override bool Update(float delta)
        {
            if (capture)
                target.RemoveCaptureListener(listener);
            else
                target.RemoveListener(listener);
            return true;
        }

        public IEventListener GetListener()
        {
            return listener;
        }

        public void SetListener(IEventListener listener)
        {
            this.listener = listener;
        }

        public bool GetCapture()
        {
            return capture;
        }

        public void SetCapture(bool capture)
        {
            this.capture = capture;
        }

        public override void Reset()
        {
            base.Reset();
            listener = null;
        }
    }
}
