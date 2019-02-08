using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BlueberryCore.UI
{
    public class Stage : InputAdapter
    {
        private Group root;
        private Camera _camera;
        private float _alpha = 1f;
        private bool _debug = false;
        private BitmapFont font;
        private int mouseScreenX, mouseScreenY;
        private Vector2 tmpCoords;

        internal Entity entity;

        private Element keyboardFocus, scrollFocus, mouseOverElement;
        //private List<(Element element, int screenX, int screenY, MouseButton button)> mouseTouchList = new List<(Element element, int screenX, int screenY, MouseButton button)>();

        public Stage(Camera camera)
        {
            _camera = camera;
            root = new Group();
            root.SetStage(this);
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

        public Rectangle CalculateScissors(Element element)
        {
            Element iterator = element.parent;
            Rectangle result = new Rectangle((int) element.x, (int) element.y, (int) element.width + 1, (int) element.height + 1);

            if (iterator == null)
            {
                return result;
            }

            while (iterator != null)
            {
                result.X += (int) iterator.x;
                result.Y += (int) iterator.y;
                iterator = iterator.parent;
            }

            return result;
        }

        public Vector2 ScreenToStageCoordinates(Vector2 screenCoords)
        {
            if (_camera == null)
                return screenCoords;
            return _camera.ScreenToWorldPoint(screenCoords);
        }

        public void ScreenToStageCoordinates(ref Vector2 inCoords, out Vector2 outCoords)
        {
            if (_camera == null)
            {
                outCoords = inCoords;
                return;
            }
            outCoords = _camera.ScreenToWorldPoint(inCoords);
        }

        public Group GetRoot()
        {
            return root;
        }

        public Camera GetCamera()
        {
            return _camera;
        }

        public float GetWidth()
        {
            return Screen.Width;
        }

        public float GetHeight()
        {
            return Screen.Height;
        }

        public void AddElement(Element element)
        {
            root.AddElement(element);
        }

        public List<Element> GetElements()
        {
            return root.elements;
        }

        public Element Hit(Vector2 point, bool touchable = true)
        {
            return root.Hit(point, touchable);
        }

        public void Update(float delta)
        {
            UpdateInput();
            root.Update(delta);
        }

        public void Draw(Graphics graphics)
        {
            root.Draw(graphics, _alpha);
            if (_debug)
                root.DrawDebug(graphics);
        }

        public void SetDebug(bool debug)
        {
            _debug = debug;
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
                    if(overLast != null)
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
            ScreenToStageCoordinates(ref tmpCoords, out tmpCoords);

            var over = Hit(tmpCoords, true);
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
            if (over != overLast)
            {
                Core.RequestRender();
            }
                
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
            if (handled)
                Core.RequestRender();
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
                Core.RequestRender();
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
            if (handled)
                Core.RequestRender();
            return handled;
        }

        public override bool TouchDown(int screenX, int screenY, int pointer, int button)
        {
            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords, out tmpCoords);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.touchDown);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);
            ev.SetPointer(pointer);
            ev.SetButton(button);

            var target = Hit(tmpCoords, true);

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
            if (handled)
                Core.RequestRender();
            return handled;
        }

        public override bool TouchUp(int screenX, int screenY, int pointer, int button)
        {
            pointerTouched[pointer] = false;
            pointerScreenX[pointer] = screenX;
            pointerScreenY[pointer] = screenY;

            if (touchFocuses.Count == 0) return false;
            
            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords, out tmpCoords);

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
            if (handled)
                Core.RequestRender();
            return handled;
        }

        public override bool MouseMoved(int screenX, int screenY)
        {
            mouseScreenX = screenX;
            mouseScreenY = screenY;

            if (!IsInsideViewport(screenX, screenY)) return false;

            tmpCoords.Set(screenX, screenY);
            ScreenToStageCoordinates(ref tmpCoords, out tmpCoords);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.mouseMoved);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);

            Element target = Hit(tmpCoords, true);
            if (target == null) target = root;

            target.Fire(ev);
            bool handled = ev.IsHandled();
            Pool<InputEvent>.Free(ev);
            if (handled)
                Core.RequestRender();
            return handled;
        }

        public override bool Scrolled(int amountX, int amountY)
        {
            Element target = scrollFocus ?? root;

            tmpCoords.Set(mouseScreenX, mouseScreenY);
            ScreenToStageCoordinates(ref tmpCoords, out tmpCoords);

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
            if (handled)
                Core.RequestRender();
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
            ScreenToStageCoordinates(ref tmpCoords, out tmpCoords);

            var ev = Pool<InputEvent>.Obtain();
            ev.Reset();
            ev.SetStage(this);
            ev.SetInputType(InputType.touchDragged);
            ev.SetStageX(tmpCoords.X);
            ev.SetStageY(tmpCoords.Y);
            ev.SetPointer(pointer);

            var focuses = touchFocuses.ToArray();
            for(int i = 0, n = touchFocuses.Count; i < n; i++)
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
            if (handled)
                Core.RequestRender();
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

        #region Focus

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

        public bool IsInsideViewport(Vector2 screenCoords)
        {
            return (screenCoords.X > 0 && screenCoords.Y > 0 && screenCoords.X < Screen.Width && screenCoords.Y < Screen.Height);
        }

        public bool IsInsideViewport(int screenX, int screenY)
        {
            return (screenX > 0 && screenY > 0 && screenX < Screen.Width && screenY < Screen.Height);
        }

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
