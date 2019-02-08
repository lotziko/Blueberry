using System.Collections.Generic;

namespace Blueberry.UI
{
    public partial class Stage : InputAdapter
    {
        private Viewport viewport;
        private Group root;
        private float _alpha = 1f;
        private bool _debug = false;
        private int mouseScreenX, mouseScreenY;
        private Vec2 tmpCoords;

        private Element keyboardFocus, scrollFocus, mouseOverElement;

        public Stage() : this(new ScalingViewport(Scaling.stretch, Screen.Width, Screen.Height, new Camera()))
        {

        }

        public Stage(Viewport viewport)
        {
            this.viewport = viewport;
            root = new Group();
            root.SetStage(this);

            viewport.Update(Screen.Width, Screen.Height, true);
        }

        #region TouchFocus

        private readonly List<TouchFocus> touchFocuses = new List<TouchFocus>();

        private readonly Element[] pointerOverElements = new Element[20];
        private readonly bool[] pointerTouched = new bool[20];
        private readonly int[] pointerScreenX = new int[20];
        private readonly int[] pointerScreenY = new int[20];

        public void AddTouchFocus(IEventListener listener, Element listenerElement, Element target, int pointer, int button)
        {
            TouchFocus focus = Pool<TouchFocus>.Obtain();
            focus.listenerElement = listenerElement;
            focus.target = target;
            focus.listener = listener;
            focus.pointer = pointer;
            focus.button = button;
            touchFocuses.Add(focus);
        }

        public void RemoveTouchFocus(IEventListener listener, Element listenerElement, Element target, int pointer, int button)
        {
            var touchFocuses = this.touchFocuses;
            for (int i = touchFocuses.Count - 1; i >= 0; i--)
            {
                var focus = touchFocuses[i];
                if (focus.listener == listener && focus.listenerElement == listenerElement && focus.target == target && focus.pointer == pointer && focus.button == button)
                {
                    touchFocuses.RemoveAt(i);
                    Pool<TouchFocus>.Free(focus);
                }
            }
        }

        #endregion

        #region ScrollFocus

        public bool SetScrollFocus(Element element)
        {
            if (scrollFocus == element) return true;
            FocusEvent ev = Pool<FocusEvent>.Obtain();
            ev.SetStage(this);

            ev.SetFocusType(FocusType.scroll);
            var oldScrollFocus = scrollFocus;
            if (oldScrollFocus != null)
            {
                ev.SetFocused(false);
                ev.SetRelatedElement(element);
                oldScrollFocus.Fire(ev);
            }
            bool success = !ev.IsCancelled();
            if (success)
            {
                scrollFocus = element;
                if (element != null)
                {
                    ev.SetFocused(true);
                    ev.SetRelatedElement(oldScrollFocus);
                    element.Fire(ev);
                    success = !ev.IsCancelled();
                    if (!success) scrollFocus = oldScrollFocus;
                }
            }
            Pool<FocusEvent>.Free(ev);
            return success;
        }

        public Element GetScrollFocus() => scrollFocus;


        #endregion

        #region KeyboardFocus

        public Element GetKeyboardFocus()
        {
            return keyboardFocus;
        }

        public bool SetKeyboardFocus(Element element)
        {
            if (keyboardFocus == element) return true;
            var ev = Pool<FocusEvent>.Obtain();
            ev.SetStage(this);

            ev.SetFocusType(FocusType.keyboard);
            var oldKeyboardFocus = keyboardFocus;
            if (oldKeyboardFocus != null)
            {
                ev.SetFocused(false);
                ev.SetRelatedElement(element);
                oldKeyboardFocus.Fire(ev);
            }
            bool success = !ev.IsCancelled();
            if (success)
            {
                keyboardFocus = element;
                if (element != null)
                {
                    ev.SetFocused(true);
                    ev.SetRelatedElement(oldKeyboardFocus);
                    element.Fire(ev);
                    success = !ev.IsCancelled();
                    if (!success) keyboardFocus = oldKeyboardFocus;
                }
            }
            Pool<FocusEvent>.Free(ev);
            return success;
        }

        #endregion

        #region Listeners

        public bool AddListener(IEventListener listener)
        {
            return root.AddListener(listener);
        }

        public bool RemoveListener(IEventListener listener)
        {
            return root.RemoveListener(listener);
        }

        public bool AddCaptureListener(IEventListener listener)
        {
            return root.AddCaptureListener(listener);
        }

        public bool RemoveCaptureListener(IEventListener listener)
        {
            return root.RemoveCaptureListener(listener);
        }

        #endregion

        #region Scissors

        public Rect CalculateScissors(Element element)
        {
            Element iterator = element.parent;
            var result = new Rect(element.x, element.y, element.width + 1, element.height + 1);

            if (iterator == null)
            {
                return result;
            }

            while (iterator != null)
            {
                result.X += (int)iterator.x;
                result.Y += (int)iterator.y;
                iterator = iterator.parent;
            }

            return result;
        }

        public Rect CalculateScissors(Rect rectangle, Element element = null)
        {
            if (element != null)
            {
                rectangle.X += element.x;
                rectangle.Y += element.y;

                Element iterator = element.parent;

                while (iterator != null)
                {
                    rectangle.X += iterator.x;
                    rectangle.Y += iterator.y;
                    iterator = iterator.parent;
                }
            }
            return viewport.CalculateScissors(Mat.Identity, rectangle);
        }

        #endregion

        #region Getters

        public Group Root => root;

        public Camera Camera => viewport.Camera;

        public Viewport Viewport => viewport;

        public float Width => viewport.WorldWidth;

        public float Height => viewport.WorldHeight;

        #endregion

        #region Elements

        public void AddElement(Element element)
        {
            root.AddElement(element);
        }

        public bool RemoveElement(Element element)
        {
            return root.RemoveElement(element);
        }

        public List<Element> GetElements()
        {
            return root.elements;
        }

        public Element Hit(float x, float y, bool touchable = true)
        {
            return root.Hit(x, y, touchable);
        }

        #endregion

        public void Update(float delta)
        {
            UpdateInput();
            root.Update(delta);
        }

        public void SetDebug(bool enable)
        {
            _debug = enable;
            root.SetDebug(enable);
        }

        public Stage DebugAll()
        {
            _debug = true;
            root.DebugAll();
            return this;
        }

        #region Input

        public void UpdateInput()
        {
            for (int pointer = 0, n = pointerOverElements.Length; pointer < n; pointer++)
            {
                var overLast = pointerOverElements[pointer];
                // Check if pointer is gone.
                if (!pointerTouched[pointer])
                {
                    if (overLast != null)
                    {
                        pointerOverElements[pointer] = null;
                        tmpCoords.X = pointerScreenX[pointer];
                        tmpCoords.Y = pointerScreenY[pointer];
                        tmpCoords = ScreenToStageCoordinates(tmpCoords);

                        var ev = Pool<InputEvent>.Obtain();
                        ev.SetInputType(InputType.exit);
                        ev.SetStage(this);

                        ev.SetStageX(tmpCoords.X);
                        ev.SetStageY(tmpCoords.Y);
                        ev.SetRelatedElement(overLast);
                        ev.SetPointer(pointer);
                        overLast.Fire(ev);
                        Pool<InputEvent>.Free(ev);
                    }
                    continue;
                }
                pointerOverElements[pointer] = FireEnterAndExit(overLast, pointerScreenX[pointer], pointerScreenY[pointer], pointer);
            }
            //#if DESKTOP
            mouseOverElement = FireEnterAndExit(mouseOverElement, mouseScreenX, mouseScreenY, -1);
            //#endif
        }

        private Element FireEnterAndExit(Element overLast, int screenX, int screenY, int pointer)
        {
            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords.X, ref tmpCoords.Y);

            var over = Hit(tmpCoords.X, tmpCoords.Y, true);
            if (over == overLast) return overLast;

            if (overLast != null)
            {
                var ev = Pool<InputEvent>.Obtain();
                ev.SetStage(this);
                ev.SetInputType(InputType.exit);
                ev.SetStageX(tmpCoords.X);
                ev.SetStageY(tmpCoords.Y);
                ev.SetRelatedElement(over);
                ev.SetPointer(pointer);
                overLast.Fire(ev);
                Pool<InputEvent>.Free(ev);
            }

            if (over != null)
            {
                var ev = Pool<InputEvent>.Obtain();
                ev.SetStage(this);
                ev.SetInputType(InputType.enter);
                ev.SetStageX(tmpCoords.X);
                ev.SetStageY(tmpCoords.Y);
                ev.SetRelatedElement(overLast);
                ev.SetPointer(pointer);
                over.Fire(ev);
                Pool<InputEvent>.Free(ev);
            }
            //if (over != overLast)
            //{
            //    Render.Request();
            //}

            return over;
        }

        public override bool KeyDown(int keycode)
        {
            var target = keyboardFocus ?? root;
            var ev = Pool<InputEvent>.Obtain();
            ev.SetStage(this);
            ev.SetInputType(InputType.keyDown);
            ev.SetKeyCode(keycode);
            target.Fire(ev);
            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            //if (handled)
            //    Render.Request();
            return handled;
        }

        public override bool KeyUp(int keycode)
        {
            var target = keyboardFocus ?? root;
            var ev = Pool<InputEvent>.Obtain();
            ev.SetStage(this);
            ev.SetInputType(InputType.keyUp);
            ev.SetKeyCode(keycode);
            target.Fire(ev);
            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            if (handled)
                Render.Request();
            return handled;
        }

        public override bool KeyTyped(int keycode, char character)
        {
            var target = keyboardFocus ?? root;
            var ev = Pool<InputEvent>.Obtain();
            ev.SetStage(this);
            ev.SetInputType(InputType.keyTyped);
            ev.SetKeyCode(keycode);
            ev.SetCharacter(character);
            target.Fire(ev);
            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            //if (handled)
            //    Render.Request();
            return handled;
        }

        public override bool TouchDown(int screenX, int screenY, int pointer, int button)
        {
            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords.X, ref tmpCoords.Y);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.touchDown);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);
            ev.SetPointer(pointer);
            ev.SetButton(button);

            var target = Hit(tmpCoords.X, tmpCoords.Y, true);

            if (target == null)
            {
                if (root.GetTouchable() == Touchable.Enabled) root.Fire(ev);
            }
            else
            {
                target.Fire(ev);
            }
            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            //if (handled)
            //    Render.Request();
            return handled;
        }

        public override bool TouchUp(int screenX, int screenY, int pointer, int button)
        {
            pointerTouched[pointer] = false;
            pointerScreenX[pointer] = screenX;
            pointerScreenY[pointer] = screenY;

            if (touchFocuses.Count == 0) return false;

            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords.X, ref tmpCoords.Y);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.touchUp);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);
            ev.SetPointer(pointer);
            ev.SetButton(button);

            var focuses = touchFocuses.ToArray();

            for (int i = 0, n = touchFocuses.Count; i < n; i++)
            {
                var focus = focuses[i];
                if (focus.pointer != pointer || focus.button != button) continue;
                if (!touchFocuses.Remove(focus)) continue; // Touch focus already gone.

                ev.SetTarget(focus.target);
                ev.SetListenerElement(focus.listenerElement);
                if (focus.listener.Handle(ev)) ev.Handle();
                Pool<TouchFocus>.Free(focus);
            }

            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            //if (handled)
            //    Render.Request();
            return handled;
        }

        public override bool MouseMoved(int screenX, int screenY)
        {
            mouseScreenX = screenX;
            mouseScreenY = screenY;

            if (!IsInsideViewport(screenX, screenY)) return false;

            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords.X, ref tmpCoords.Y);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.mouseMoved);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);

            Element target = Hit(tmpCoords.X, tmpCoords.Y, true);
            if (target == null) target = root;

            target.Fire(ev);
            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            //if (handled)
            //    Render.Request();
            return handled;
        }

        public override bool Scrolled(float amountX, float amountY)
        {
            Element target = scrollFocus ?? root;

            tmpCoords.Set(mouseScreenX, mouseScreenY);
            ScreenToStageCoordinates(ref tmpCoords.X, ref tmpCoords.Y);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.scrolled);
            ev.SetScrollAmountX(amountX);
            ev.SetScrollAmountY(amountY);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);
            target.Fire(ev);
            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            //if (handled)
            //    Render.Request();
            return handled;
        }

        public override bool TouchDragged(int screenX, int screenY, int pointer)
        {
            pointerScreenX[pointer] = screenX;
            pointerScreenY[pointer] = screenY;
            mouseScreenX = screenX;
            mouseScreenY = screenY;

            if (touchFocuses.Count == 0) return false;

            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords.X, ref tmpCoords.Y);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.touchDragged);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);
            ev.SetPointer(pointer);

            var focuses = touchFocuses.ToArray();
            for (int i = 0, n = touchFocuses.Count; i < n; i++)
            {
                var focus = focuses[i];
                if (focus.pointer != pointer) continue;
                if (!touchFocuses.Contains(focus)) continue;
                ev.SetTarget(focus.target);
                ev.SetListenerElement(focus.listenerElement);
                if (focus.listener.Handle(ev)) ev.Handle();
            }

            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            //if (handled)
            //    Render.Request();
            return handled;
        }

        public void CancelTouchFocus(Element element)
        {
            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.touchUp);
            ev.SetStageX(int.MinValue);
            ev.SetStageY(int.MinValue);

            var focuses = touchFocuses.ToArray();
            for (int i = 0, n = touchFocuses.Count; i < n; i++)
            {
                var focus = focuses[i];
                if (focus.listenerElement != element) continue;
                if (!touchFocuses.Remove(focus)) continue; // Touch focus already gone.
                ev.SetTarget(focus.target);
                ev.SetListenerElement(focus.listenerElement);
                ev.SetPointer(focus.pointer);
                ev.SetButton(focus.button);
                focus.listener.Handle(ev);
            }
            Pool<InputEvent>.Free(ev);
        }

        public void CancelTouchFocusExcept(IEventListener exceptListener, Element exceptElement)
        {
            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetInputType(InputType.touchUp);
            ev.SetStageX(int.MinValue);
            ev.SetStageY(int.MinValue);

            var focuses = touchFocuses.ToArray();
            for (int i = 0, n = touchFocuses.Count; i < n; i++)
            {
                var focus = focuses[i];
                if (focus.listener == exceptListener && focus.listenerElement == exceptElement) continue;
                if (!touchFocuses.Remove(focus)) continue; // Touch focus already gone.
                ev.SetTarget(focus.target);
                ev.SetListenerElement(focus.listenerElement);
                ev.SetPointer(focus.pointer);
                ev.SetButton(focus.button);
                focus.listener.Handle(ev);
            }
            Pool<InputEvent>.Free(ev);
        }

        #endregion

        #region Coordinates convert

        public void ScreenToStageCoordinates(ref float screenCoordsX, ref float screenCoordsY)
        {
            if (Camera == null)
                return;
            viewport.Unproject(ref screenCoordsX, ref screenCoordsY);
        }

        public Vec2 ScreenToStageCoordinates(Vec2 coords)
        {
            if (Camera == null)
                return coords;
            return viewport.Unproject(coords);
        }

        #endregion

        #region Viewport

        public bool IsInsideViewport((float x, float y) screenCoords)
        {
            return (screenCoords.x > 0 && screenCoords.y > 0 && screenCoords.x < Screen.Width && screenCoords.y < Screen.Height);
        }

        public bool IsInsideViewport(int screenX, int screenY)
        {
            return (screenX > 0 && screenY > 0 && screenX < Screen.Width && screenY < Screen.Height);
        }

        #endregion
    }

    internal class TouchFocus : IPoolable
    {
        internal IEventListener listener;
        internal Element listenerElement, target;
        internal int pointer, button;

        public void Reset()
        {
            listenerElement = null;
            listener = null;
            target = null;
        }
    }
}
