
namespace Blueberry.UI
{
    public class TemporalAction : Action
    {
        private float duration, time;
        private Interpolation interpolation;
        private bool reverse, began, complete;

        public TemporalAction()
        {
        }

        public TemporalAction(float duration)
        {
            this.duration = duration;
        }

        public TemporalAction(float duration, Interpolation interpolation)
        {
            this.duration = duration;
            this.interpolation = interpolation;
        }

        public override bool Update(float delta)
        {
            if (complete) return true;
            //Pool pool = getPool();
            //setPool(null); // Ensure this action can't be returned to the pool while executing.
            try
            {
                if (!began)
                {
                    Begin();
                    began = true;
                }
                time += delta;
                complete = time >= duration;
                float percent;
                if (complete)
                    percent = 1;
                else
                {
                    percent = time / duration;
                    if (interpolation != null) percent = interpolation.Apply(percent);
                }
                LocalUpdate(reverse ? 1 - percent : percent);
                if (complete)
                    End();
                return complete;
            }
            finally
            {
                //setPool(pool);
            }
        }

        /** Called the first time {@link #act(float)} is called. This is a good place to query the {@link #actor actor's} starting
	    * state. */
        protected virtual void Begin()
        {
        }

        /** Called the last time {@link #act(float)} is called. */
        protected virtual void End()
        {
        }

        /** Called each frame.
         * @param percent The percentage of completion for this action, growing from 0 to 1 over the duration. If
         *           {@link #setReverse(boolean) reversed}, this will shrink from 1 to 0. */
        protected virtual void LocalUpdate(float percent)
        {
        }

        /** Skips to the end of the transition. */
        public void Finish()
        {
            time = duration;
        }

        public override void Restart()
        {
            time = 0;
            began = false;
            complete = false;
        }

        public override void Reset()
        {
            base.Reset();
            reverse = false;
            interpolation = null;
        }

        /** Gets the transition time so far. */
        public float GetTime()
        {
            return time;
        }

        /** Sets the transition time so far. */
        public void SetTime(float time)
        {
            this.time = time;
        }

        public float GetDuration()
        {
            return duration;
        }

        /** Sets the length of the transition in seconds. */
        public void SetDuration(float duration)
        {
            this.duration = duration;
        }

        public Interpolation GetInterpolation()
        {
            return interpolation;
        }

        public void SetInterpolation(Interpolation interpolation)
        {
            this.interpolation = interpolation;
        }

        public bool IsReverse()
        {
            return reverse;
        }

        /** When true, the action's progress will go from 100% to 0%. */
        public void SetReverse(bool reverse)
        {
            this.reverse = reverse;
        }
    }
}
