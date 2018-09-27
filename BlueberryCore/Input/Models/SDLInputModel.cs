using BlueberryCore.SDL;
using Microsoft.Xna.Framework;

namespace BlueberryCore.InputModels
{
    public class SDLInputModel : IInputModel
    {
        private void OnKeyDown(object sender, KeyDownEventArgs args)
        {
            Input.CheckEventTime();
            if (char.IsControl(args.character))
                OnTextInput(null, new TextInputEventArgs(args.character, args.key));

            if (!Input.keyStates.ContainsKey(args.key))
            {
                var state = Pool<InputState>.Obtain();
                state.eventFrame = TimeUtils.CurrentFrame();
                Input.keyStates.Add(args.key, state);
            }

            if (Input.Processor != null)
            {
                Input.Processor.KeyDown((int)args.key);
            }
        }

        private void OnKeyUp(object sender, KeyUpEventArgs args)
        {
            Input.CheckEventTime();

            Input.keyStates.TryGetValue(args.key, out InputState state);
            if (state != null)
            {
                Input.keyStates.Remove(args.key);
                Pool<InputState>.Free(state);
            }

            if (Input.Processor != null)
            {
                Input.Processor.KeyUp((int)args.key);
            }
        }

        private void OnMouseMotion(object sender, MouseMotionEventArgs args)
        {
            Input.CheckEventTime();
            Input.previousMousePos = Input.mousePos;
            Input.mousePos.X = args.x;
            Input.mousePos.Y = args.y;

            if (Input.Processor != null)
            {
                if (Input.mouseStates.Count > 0)
                    Input.Processor.TouchDragged(args.x, args.y, 0);
                else
                    Input.Processor.MouseMoved(args.x, args.y);

            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs args)
        {
            Input.CheckEventTime();
            Input.mouseWheelX = args.x;
            Input.mouseWheelY = args.y;

            if (Input.Processor != null && Input.mouseWheelY != 0)
                Input.Processor.Scrolled(Input.mouseWheelY);
        }

        private void OnMouseButtonDown(object sender, MouseButtonDownEventArgs args)
        {
            Input.CheckEventTime();

            var state = Pool<InputState>.Obtain();
            state.eventFrame = TimeUtils.CurrentFrame();
            Input.mouseStates.Add(args.button, state);

            if (Input.Processor != null)
                Input.Processor.TouchDown(args.x, args.y, 0, (int)args.button);
        }

        private void OnMouseButtonUp(object sender, MouseButtonUpEventArgs args)
        {
            Input.CheckEventTime();

            Input.mouseStates.TryGetValue(args.button, out InputState state);
            if (state != null)
            {
                Input.mouseStates.Remove(args.button);
                Pool<InputState>.Free(state);
            }

            if (Input.Processor != null)
                Input.Processor.TouchUp(args.x, args.y, 0, (int)args.button);
        }

        private void OnTextInput(object sender, TextInputEventArgs args)
        {
            if (Input.Processor != null)
            {
                Input.Processor.KeyTyped((int)args.Character, args.Character);
                Input.CheckEventTime();
            }
        }

        public void Initialize()
        {
            EDSManager.KeyDown += OnKeyDown;
            EDSManager.KeyUp += OnKeyUp;
            EDSManager.MouseMotion += OnMouseMotion;
            EDSManager.MouseWheel += OnMouseWheel;
            EDSManager.MouseButtonDown += OnMouseButtonDown;
            EDSManager.MouseButtonUp += OnMouseButtonUp;
            EDSManager.TextInput += OnTextInput;
        }

        public void Update()
        {
            var currFrame = TimeUtils.CurrentFrame();
            foreach (var statePair in Input.mouseStates)
            {
                var state = statePair.Value;
                if (state.eventFrame != currFrame)
                    state.outdated = true;
            }
            foreach (var statePair in Input.keyStates)
            {
                var state = statePair.Value;
                if (state.eventFrame != currFrame)
                    state.outdated = true;
            }
            Input.mouseWheelX = 0;
            Input.mouseWheelY = 0;

            return;
        }
    }
}
