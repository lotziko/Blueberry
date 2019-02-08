
namespace Blueberry.UI
{
    public class Event : IPoolable
    {
        private Stage _stage;
        private Element _targetElement;
        private Element _listenerElement;
        private bool _capture; // true means event occurred during the capture phase
        private bool _bubbles = true; // true means propagate to target's parents
        private bool _handled; // true means the event was handled (the stage will eat the input)
        private bool _stopped; // true means event propagation was stopped
        private bool _cancelled; // true means propagation was stopped and any action that this event would cause should not happen

        /** Marks this event as handled. This does not affect event propagation inside scene2d, but causes the {@link Stage} event
         * methods to return true, which will eat the event so it is not passed on to the application under the stage. */
        public virtual void Handle()
        {
            _handled = true;
        }

        /** Marks this event cancelled. This {@link #handle() handles} the event and {@link #stop() stops} the event propagation. It
         * also cancels any default action that would have been taken by the code that fired the event. Eg, if the event is for a
         * checkbox being checked, cancelling the event could uncheck the checkbox. */
        public void Cancel()
        {
            _cancelled = true;
            _stopped = true;
            _handled = true;
        }

        /** Marks this event has being stopped. This halts event propagation. Any other listeners on the {@link #getListenerElement()
         * listener Element} are notified, but after that no other listeners are notified. */
        public void Stop()
        {
            _stopped = true;
        }

        public virtual void Reset()
        {
            _stage = null;
            _targetElement = null;
            _listenerElement = null;
            _capture = false;
            _bubbles = true;
            _handled = false;
            _stopped = false;
            _cancelled = false;
        }

        /** Returns the Element that the event originated from. */
        public Element GetTarget()
        {
            return _targetElement;
        }

        public void SetTarget(Element targetElement)
        {
            _targetElement = targetElement;
        }

        /** Returns the Element that this listener is attached to. */
        public Element GetListenerElement()
        {
            return _listenerElement;
        }

        public void SetListenerElement(Element listenerElement)
        {
            _listenerElement = listenerElement;
        }

        public bool GetBubbles()
        {
            return _bubbles;
        }

        /** If true, after the event is fired on the target Element, it will also be fired on each of the parent Elements, all the way to
         * the root. */
        public void SetBubbles(bool bubbles)
        {
            _bubbles = bubbles;
        }

        /** {@link #handle()} */
        public bool IsHandled()
        {
            return _handled;
        }

        /** @see #stop() */
        public bool IsStopped()
        {
            return _stopped;
        }

        /** @see #cancel() */
        public bool IsCancelled()
        {
            return _cancelled;
        }

        public void SetCapture(bool capture)
        {
            _capture = capture;
        }

        /** If true, the event was fired during the capture phase.
         * @see Element#fire(Event) */
        public bool IsCapture()
        {
            return _capture;
        }

        public void SetStage(Stage stage)
        {
            _stage = stage;
        }

        /** The stage for the Element the event was fired on. */
        public Stage GetStage()
        {
            return _stage;
        }
    }
}
