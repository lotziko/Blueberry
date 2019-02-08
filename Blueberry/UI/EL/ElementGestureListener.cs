
namespace Blueberry.UI
{
    public class ElementGestureListener : IEventListener
    {
        static Vec2 tmpCoords = new Vec2(), tmpCoords2 = new Vec2();

        private readonly GestureDetector detector;
        private readonly Adapter adapter;
	    InputEvent ev;
        Element element, touchDownTarget;

        public ElementGestureListener() : this(20, 0.4f, 1.1f, 0.15f) { }

        public ElementGestureListener(float halfTapSquareSize, float tapCountInterval, float longPressDuration, float maxFlingDelay)
        {
            detector = new GestureDetector(halfTapSquareSize, tapCountInterval, longPressDuration, maxFlingDelay, adapter = new Adapter(this));
        }

        public GestureDetector GetGestureDetector() => detector;

        private class Adapter : GestureAdapter
        {
            public Adapter(ElementGestureListener egl)
            {
                _egl = egl;
                _e = egl.element;
            }

            private readonly Vec2 initialPointer1 = new Vec2(), initialPointer2 = new Vec2();
            private readonly Vec2 pointer1 = new Vec2(), pointer2 = new Vec2();
            private ElementGestureListener _egl;
            private Element _e;

            public void SetElement(Element e) => _e = e;

            public override bool Tap(float x, float y, int count, int button)
            {
                tmpCoords.Set(x, y);
                _e.StageToLocalCoordinates(ref tmpCoords.X, ref tmpCoords.Y);
                _egl.Tap(_egl.ev, x, y, count, button);
                return true;
            }

            public override bool Pan(float x, float y, float deltaX, float deltaY)
            {
                tmpCoords.Set(deltaX, deltaY);
                tmpCoords = StageToLocalAmount(tmpCoords);
                deltaX = tmpCoords.X;
                deltaY = tmpCoords.Y;
                tmpCoords.Set(x, y);
                _e.StageToLocalCoordinates(ref tmpCoords.X, ref tmpCoords.Y);
                _egl.Pan(_egl.ev, tmpCoords.X, tmpCoords.Y, deltaX, deltaY);
                return true;
            }

            public override bool Fling(float velocityX, float velocityY, int button)
            {
                tmpCoords.Set(velocityX, velocityY);
                tmpCoords = StageToLocalAmount(tmpCoords);
                _egl.Fling(_egl.ev, tmpCoords.X, tmpCoords.Y, button);
                return true;
            }


            private Vec2 StageToLocalAmount(Vec2 amount)
            {
                _e.StageToLocalCoordinates(ref amount.X, ref amount.Y);
                tmpCoords2.Set(0, 0);
                _e.StageToLocalCoordinates(ref tmpCoords2.X, ref tmpCoords2.Y);
                amount -= tmpCoords2;
                return amount;
            }

        }

        public virtual bool Handle(Event e)
        {
            if (!(e is InputEvent)) return false;
            var ev = e as InputEvent;
            
            switch (ev.GetInputType())
            {
                case InputType.touchDown:
                    element = ev.GetListenerElement();
                    adapter.SetElement(element);
                    touchDownTarget = ev.GetTarget();
                    detector.TouchDown(ev.GetStageX(), ev.GetStageY(), ev.GetPointer(), ev.GetButton());
                    tmpCoords.Set(ev.GetStageX(), ev.GetStageY());
                    element.StageToLocalCoordinates(ref tmpCoords.X, ref tmpCoords.Y);
                    TouchDown(ev, tmpCoords.X, tmpCoords.Y, ev.GetPointer(), ev.GetButton());
                    return true;
                case InputType.touchUp:
                    if (ev.IsTouchFocusCancel())
                    {
                        detector.Reset();
                        return false;
                    }

                    this.ev = ev;
                    element = ev.GetListenerElement();
                    adapter.SetElement(element);

                    detector.TouchUp(ev.GetStageX(), ev.GetStageY(), ev.GetPointer(), ev.GetButton());
                    tmpCoords.Set(ev.GetStageX(), ev.GetStageY());
                    element.StageToLocalCoordinates(ref tmpCoords.X, ref tmpCoords.Y);
                    TouchUp(ev, tmpCoords.X, tmpCoords.Y, ev.GetPointer(), ev.GetButton());
                    return true;
                case InputType.touchDragged:
                    this.ev = ev;
                    element = ev.GetListenerElement();
                    adapter.SetElement(element);
                    
                    detector.TouchDragged(ev.GetStageX(), ev.GetStageY(), ev.GetPointer());
                    return true;
            }
            return false;
        }

        public void TouchDown(InputEvent ev, float x, float y, int pointer, int button)
        {
        }

        public void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
        {
        }

        public virtual void Tap(InputEvent ev, float x, float y, int count, int button)
        {

        }

        public virtual void Pan(InputEvent ev, float x, float y, float deltaX, float deltaY)
        {

        }

        public virtual void Fling(InputEvent ev, float velocityX, float velocityY, int button)
        {

        }
    }
}
