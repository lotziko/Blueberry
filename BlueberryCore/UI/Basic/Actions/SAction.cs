
namespace BlueberryCore.UI.Actions
{
    public class SAction
    {
        public static T Action<T>() where T : Action, new()
        {
            T action = Pool<T>.Obtain();
            //action.SetPool(pool);
            return action;
        }

        /** Sets the actor's alpha instantly. */
        static public AlphaAction Alpha(float a)
        {
            return Alpha(a, 0, null);
        }

        /** Transitions from the alpha at the time this action starts to the specified alpha. */
        static public AlphaAction Alpha(float a, float duration)
        {
            return Alpha(a, duration, null);
        }

        /** Transitions from the alpha at the time this action starts to the specified alpha. */
        static public AlphaAction Alpha(float a, float duration, Interpolation interpolation)
        {
            var action = Action<AlphaAction>();
            action.SetAlpha(a);
            action.SetDuration(duration);
            action.SetInterpolation(interpolation);
            return action;
        }
        /** Transitions from the alpha at the time this action starts to an alpha of 0. */
        public static AlphaAction FadeOut(float duration)
        {
            return Alpha(0, duration, null);
        }
        
        /** Transitions from the alpha at the time this action starts to an alpha of 0. */
        static public AlphaAction FadeOut(float duration, Interpolation interpolation)
        {
            var action = Action<AlphaAction>();
            action.SetAlpha(0);
            action.SetDuration(duration);
            action.SetInterpolation(interpolation);
            return action;
        }

        /** Transitions from the alpha at the time this action starts to an alpha of 1. */
        static public AlphaAction FadeIn(float duration)
        {
            return Alpha(255, duration, null);
        }

        /** Transitions from the alpha at the time this action starts to an alpha of 1. */
        static public AlphaAction FadeIn(float duration, Interpolation interpolation)
        {
            AlphaAction action = Action<AlphaAction>();
            action.SetAlpha(255);
            action.SetDuration(duration);
            action.SetInterpolation(interpolation);
            return action;
        }

        static public SequenceAction Sequence(Action action1)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            return action;
        }

        static public SequenceAction Sequence(Action action1, Action action2)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            return action;
        }

        static public SequenceAction Sequence(Action action1, Action action2, Action action3)
        {
            SequenceAction action = Action<SequenceAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            action.AddAction(action3);
            return action;
        }

        static public ParallelAction Parallel(Action action1)
        {
            ParallelAction action = Action<ParallelAction>();
            action.AddAction(action1);
		    return action;
        }

        static public ParallelAction Parallel(Action action1, Action action2)
        {
            ParallelAction action = Action<ParallelAction>();
            action.AddAction(action1);
            action.AddAction(action2);
            return action;
        }

        /** Scales the actor instantly. */
        static public ScaleToAction ScaleTo(float x, float y)
        {
            return ScaleTo(x, y, 0, null);
        }

        static public ScaleToAction ScaleTo(float x, float y, float duration)
        {
            return ScaleTo(x, y, duration, null);
        }

        static public ScaleToAction ScaleTo(float x, float y, float duration, Interpolation interpolation)
        {
            ScaleToAction action = Action<ScaleToAction>();
            action.SetScale(x, y);
            action.SetDuration(duration);
            action.SetInterpolation(interpolation);
            return action;
        }

        static public RemoveElementAction RemoveElement()
        {
            return Action<RemoveElementAction>();
        }

        static public RemoveElementAction RemoveElement(Element removeElement)
        {
            RemoveElementAction action = Action<RemoveElementAction>();
            action.SetTarget(removeElement);
            return action;
        }

        static public RemoveListenerAction RemoveListener(IEventListener listener, bool capture)
        {
            RemoveListenerAction action = Action<RemoveListenerAction>();
            action.SetListener(listener);
            action.SetCapture(capture);
            return action;
        }

        static public RemoveListenerAction RemoveListener(IEventListener listener, bool capture, Element targetElement)
        {
            RemoveListenerAction action = Action<RemoveListenerAction>();
            action.SetTarget(targetElement);
            action.SetListener(listener);
            action.SetCapture(capture);
            return action;
        }
    }
}
