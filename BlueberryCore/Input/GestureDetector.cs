using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore
{
    public class GestureDetector : InputAdapter
    {
        readonly IGestureListener listener;
        private float tapSquareSize;
        private long tapCountInterval;
        private float longPressSeconds;
        private long maxFlingDelay;

        private bool inTapSquare;
        private int tapCount;
        private long lastTapTime;
        private float lastTapX, lastTapY;
        private int lastTapButton, lastTapPointer;
        bool longPressFired;
        private bool pinching;
        private bool panning;

        private readonly VelocityTracker tracker = new VelocityTracker();
        private float tapSquareCenterX, tapSquareCenterY;
        private long gestureStartTime;
        Vector2 pointer1 = new Vector2();
        private Vector2 pointer2 = new Vector2();
        private Vector2 initialPointer1 = new Vector2();
        private Vector2 initialPointer2 = new Vector2();

        private readonly Task longPressTask;

        private class LongPressTask : Task
        {
            private readonly GestureDetector gd;

            public LongPressTask(GestureDetector gd)
            {
                this.gd = gd;
            }

            public override void Run()
            {
                if (!gd.longPressFired) gd.longPressFired = gd.listener.LongPress(gd.pointer1.X, gd.pointer1.Y);
            }
        }

        /// <summary>
        /// Creates a new GestureDetector with default values: halfTapSquareSize=20, tapCountInterval=0.4f, longPressDuration=1.1f, maxFlingDelay=0.15f.
        /// </summary>
        /// <param name="listener"></param>
        public GestureDetector(IGestureListener listener) : this(20, 0.4f, 1.1f, 0.15f, listener) { }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="halfTapSquareSize">halfTapSquareSize half width in pixels of the square around an initial touch event.</param>
        /// <param name="tapCountInterval">tapCountInterval time in seconds that must pass for two touch down/up sequences to be detected as consecutive taps.</param>
        /// <param name="longPressDuration">longPressDuration time in seconds that must pass for the detector to fire a GestureListener.LongPress(float, float) event.</param>
        /// <param name="maxFlingDelay">maxFlingDelay time in seconds the finger must have been dragged for a fling event to be fired.</param>
        /// <param name="listener">listener May be null if the listener will be set later.</param>
        public GestureDetector(float halfTapSquareSize, float tapCountInterval, float longPressDuration, float maxFlingDelay, IGestureListener listener)
        {
            tapSquareSize = halfTapSquareSize;
            this.tapCountInterval = (long)(tapCountInterval * 1000000000L);
            longPressSeconds = longPressDuration;
            this.maxFlingDelay = (long)(maxFlingDelay * 1000000000L);
            this.listener = listener;
            longPressTask = new LongPressTask(this);
        }

        public override bool TouchDown(int screenX, int screenY, int pointer, int button)
        {
            return TouchDown(screenX, screenY, pointer, button);
        }

        public bool TouchDown(float x, float y, int pointer, int button)
        {
            if (pointer > 1) return false;

            if (pointer == 0)
            {
                pointer1.Set(x, y);
                gestureStartTime = Input.GetCurrentEventTime();
                tracker.Start(x, y, gestureStartTime);
                if (Input.IsTouched(1))
                {
                    // Start pinch.
                    inTapSquare = false;
                    pinching = true;
                    initialPointer1.Set(pointer1);
                    initialPointer2.Set(pointer2);
                    Timer.Cancel(longPressTask);
                }
                else
                {
                    // Normal touch down.
                    inTapSquare = true;
                    pinching = false;
                    longPressFired = false;
                    tapSquareCenterX = x;
                    tapSquareCenterY = y;
                    if (!Timer.IsScheduled(longPressTask)) Timer.Schedule(longPressTask, TimeSpan.FromSeconds(longPressSeconds));
                }
            }
            else
            {
                // Start pinch.
                pointer2.Set(x, y);
                inTapSquare = false;
                pinching = true;
                initialPointer1.Set(pointer1);
                initialPointer2.Set(pointer2);
                Timer.Cancel(longPressTask);
            }
            return listener.TouchDown(x, y, pointer, button);
        }

        public override bool TouchDragged(int screenX, int screenY, int pointer)
        {
            return TouchDragged(screenX, screenY, pointer);
        }

        public bool TouchDragged(float x, float y, int pointer)
        {
            if (pointer > 1) return false;
            if (longPressFired) return false;

            if (pointer == 0)
                pointer1.Set(x, y);
            else
                pointer2.Set(x, y);

            // handle pinch zoom
            if (pinching)
            {
                if (listener != null)
                {
                    bool result = listener.Pinch(initialPointer1, initialPointer2, pointer1, pointer2);
                    return listener.Zoom(initialPointer1.Distance(initialPointer2), pointer1.Distance(pointer2)) || result;
                }
                return false;
            }

            // update tracker
            tracker.Update(x, y, Input.GetCurrentEventTime());

            // check if we are still tapping.
            if (inTapSquare && !IsWithinTapSquare(x, y, tapSquareCenterX, tapSquareCenterY))
            {
                Timer.Cancel(longPressTask);
                inTapSquare = false;
            }

            // if we have left the tap square, we are panning
            if (!inTapSquare)
            {
                panning = true;
                return listener.Pan(x, y, tracker.deltaX, tracker.deltaY);
            }

            return false;
        }

        public override bool TouchUp(int screenX, int screenY, int pointer, int button)
        {
            return TouchUp(screenX, screenY, pointer, button);
        }

        public bool TouchUp(float x, float y, int pointer, int button)
        {
            if (pointer > 1) return false;

            // check if we are still tapping.
            if (inTapSquare && !IsWithinTapSquare(x, y, tapSquareCenterX, tapSquareCenterY)) inTapSquare = false;

            bool wasPanning = panning;
            panning = false;

            Timer.Cancel(longPressTask);
            if (longPressFired) return false;

            if (inTapSquare)
            {
                // handle taps
                if (lastTapButton != button || lastTapPointer != pointer || TimeUtils.NanoTime() - lastTapTime > tapCountInterval
                    || !IsWithinTapSquare(x, y, lastTapX, lastTapY)) tapCount = 0;
                tapCount++;
                lastTapTime = TimeUtils.NanoTime();
                lastTapX = x;
                lastTapY = y;
                lastTapButton = button;
                lastTapPointer = pointer;
                gestureStartTime = 0;
                return listener.Tap(x, y, tapCount, button);
            }

            if (pinching)
            {
                // handle pinch end
                pinching = false;
                listener.PinchStop();
                panning = true;
                // we are in pan mode again, reset velocity tracker
                if (pointer == 0)
                {
                    // first pointer has lifted off, set up panning to use the second pointer...
                    tracker.Start(pointer2.X, pointer2.Y, Input.GetCurrentEventTime());
                }
                else
                {
                    // second pointer has lifted off, set up panning to use the first pointer...
                    tracker.Start(pointer1.X, pointer1.Y, Input.GetCurrentEventTime());
                }
                return false;
            }

            // handle no longer panning
            bool handled = false;
            if (wasPanning && !panning) handled = listener.PanStop(x, y, pointer, button);

            // handle fling
            gestureStartTime = 0;
            long time = Input.GetCurrentEventTime();
            if (time - tracker.lastTime < maxFlingDelay)
            {
                tracker.Update(x, y, time);
                handled = listener.Fling(tracker.GetVelocityX(), tracker.GetVelocityY(), button) || handled;
            }
            return handled;
        }

        public void Cancel()
        {
            Timer.Cancel(longPressTask);
            longPressFired = true;
        }

        public bool IsLongPressed()
        {
            return IsLongPressed(longPressSeconds);
        }


        public bool IsLongPressed(float duration)
        {
            if (gestureStartTime == 0) return false;
            return TimeUtils.NanoTime() - gestureStartTime > (long)(duration * 1000000000L);
        }

        public bool IsPanning()
        {
            return panning;
        }

        public void Reset()
        {
            gestureStartTime = 0;
            panning = false;
            inTapSquare = false;
            tracker.lastTime = 0;
        }

        private bool IsWithinTapSquare(float x, float y, float centerX, float centerY)
        {
            return Math.Abs(x - centerX) < tapSquareSize && Math.Abs(y - centerY) < tapSquareSize;
        }

        /** The tap square will not longer be used for the current touch. */
        public void InvalidateTapSquare()
        {
            inTapSquare = false;
        }

        public void SetTapSquareSize(float halfTapSquareSize)
        {
            tapSquareSize = halfTapSquareSize;
        }

        /** @param tapCountInterval time in seconds that must pass for two touch down/up sequences to be detected as consecutive taps. */
        public void SetTapCountInterval(float tapCountInterval)
        {
            this.tapCountInterval = (long)(tapCountInterval * 1000000000L);
        }

        public void SetLongPressSeconds(float longPressSeconds)
        {
            this.longPressSeconds = longPressSeconds;
        }

        public void SetMaxFlingDelay(long maxFlingDelay)
        {
            this.maxFlingDelay = maxFlingDelay;
        }


    }

    public interface IGestureListener
    {
        bool TouchDown(float x, float y, int pointer, int button);

        bool Tap(float x, float y, int count, int button);

        bool LongPress(float x, float y);

        bool Fling(float velocityX, float velocityY, int button);

        bool Pan(float x, float y, float deltaX, float deltaY);

        bool PanStop(float x, float y, int pointer, int button);

        bool Zoom(float initialDistance, float distance);

        bool Pinch(Vector2 initialPointer1, Vector2 initialPointer2, Vector2 pointer1, Vector2 pointer2);

        void PinchStop();
    }

    public class GestureAdapter : IGestureListener
    {
        public virtual bool Fling(float velocityX, float velocityY, int button)
        {
            return false;
        }

        public virtual bool LongPress(float x, float y)
        {
            return false;
        }

        public virtual bool Pan(float x, float y, float deltaX, float deltaY)
        {
            return false;
        }

        public virtual bool PanStop(float x, float y, int pointer, int button)
        {
            return false;
        }

        public virtual bool Pinch(Vector2 initialPointer1, Vector2 initialPointer2, Vector2 pointer1, Vector2 pointer2)
        {
            return false;
        }

        public virtual void PinchStop()
        {
            return;
        }

        public virtual bool Tap(float x, float y, int count, int button)
        {
            return false;
        }

        public virtual bool TouchDown(float x, float y, int pointer, int button)
        {
            return false;
        }

        public virtual bool Zoom(float initialDistance, float distance)
        {
            return false;
        }
    }

    internal class VelocityTracker
    {
        internal int sampleSize = 10;
        internal float lastX, lastY;
        internal float deltaX, deltaY;
        internal long lastTime;
        internal int numSamples;
        internal float[] meanX;
        internal float[] meanY;
        internal long[] meanTime;

        public VelocityTracker()
        {
            meanX = new float[sampleSize];
            meanY = new float[sampleSize];
            meanTime = new long[sampleSize];
        }

        public void Start(float x, float y, long timeStamp)
        {
            lastX = x;
            lastY = y;
            deltaX = 0;
            deltaY = 0;
            numSamples = 0;
            for (int i = 0; i < sampleSize; i++)
            {
                meanX[i] = 0;
                meanY[i] = 0;
                meanTime[i] = 0;
            }
            lastTime = timeStamp;
        }

        public void Update(float x, float y, long currTime)
        {
            deltaX = x - lastX;
            deltaY = y - lastY;
            lastX = x;
            lastY = y;
            long deltaTime = currTime - lastTime;
            lastTime = currTime;
            int index = numSamples % sampleSize;
            meanX[index] = deltaX;
            meanY[index] = deltaY;
            meanTime[index] = deltaTime;
            numSamples++;
        }

        public float GetVelocityX()
        {
            float meanX = GetAverage(this.meanX, numSamples);
            float meanTime = GetAverage(this.meanTime, numSamples) / 1000000000.0f;
            if (meanTime == 0) return 0;
            return meanX / meanTime;
        }

        public float GetVelocityY()
        {
            float meanY = GetAverage(this.meanY, numSamples);
            float meanTime = GetAverage(this.meanTime, numSamples) / 1000000000.0f;
            if (meanTime == 0) return 0;
            return meanY / meanTime;
        }

        private float GetAverage(float[] values, int numSamples)
        {
            numSamples = Math.Min(sampleSize, numSamples);
            float sum = 0;
            for (int i = 0; i < numSamples; i++)
            {
                sum += values[i];
            }
            return sum / numSamples;
        }

        private long GetAverage(long[] values, int numSamples)
        {
            numSamples = Math.Min(sampleSize, numSamples);
            long sum = 0;
            for (int i = 0; i < numSamples; i++)
            {
                sum += values[i];
            }
            if (numSamples == 0) return 0;
            return sum / numSamples;
        }

        private float GetSum(float[] values, int numSamples)
        {
            numSamples = Math.Min(sampleSize, numSamples);
            float sum = 0;
            for (int i = 0; i < numSamples; i++)
            {
                sum += values[i];
            }
            if (numSamples == 0) return 0;
            return sum;
        }
    }
}
