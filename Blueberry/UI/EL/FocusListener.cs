
namespace Blueberry.UI
{
    public abstract class FocusListener : IEventListener
    {
        public bool Handle(Event ev)
        {
            if (!(ev is FocusEvent)) return false;
            FocusEvent focusEvent = (FocusEvent)ev;
            switch (focusEvent.GetFocusType())
            {
                case FocusType.keyboard:
                    KeyboardFocusChanged(focusEvent, ev.GetTarget(), focusEvent.IsFocused());
                    break;
                case FocusType.scroll:
                    ScrollFocusChanged(focusEvent, ev.GetTarget(), focusEvent.IsFocused());
                    break;
            }
            return false;
        }

        /** @param element The ev target, which is the element that emitted the focus ev. */
        public virtual void KeyboardFocusChanged(FocusEvent ev, Element element, bool focused)
        {
        }

        /** @param element The ev target, which is the element that emitted the focus ev. */
        public virtual void ScrollFocusChanged(FocusEvent ev, Element element, bool focused)
        {
        }
    }

    public abstract class FocusListener<T> : FocusListener
    {
        protected readonly T par;

        public FocusListener(T par)
        {
            this.par = par;
        }
    }

    /** Fired when an element gains or loses keyboard or scroll focus. Can be cancelled to prevent losing or gaining focus.
        * @author Nathan Sweet */
    public class FocusEvent : Event
    {

        private bool focused;
        private FocusType type;
        private Element relatedElement;

        public override void Reset()
        {
            base.Reset();
            relatedElement = null;
        }

        public bool IsFocused()
        {
            return focused;
        }

        public void SetFocused(bool focused)
        {
            this.focused = focused;
        }

        public FocusType GetFocusType()
        {
            return type;
        }

        public void SetFocusType(FocusType focusType)
        {
            this.type = focusType;
        }

        /** The element related to the ev. When focus is lost, this is the new element being focused, or null. When focus is gained,
         * this is the previous element that was focused, or null. */
        public Element GetRelatedElement()
        {
            return relatedElement;
        }

        /** @param relatedElement May be null. */
        public void SetRelatedElement(Element relatedElement)
        {
            this.relatedElement = relatedElement;
        }
    }

    public enum FocusType
    {
        keyboard, scroll
    }
}