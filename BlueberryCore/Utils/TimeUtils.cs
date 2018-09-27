using System;
using System.Diagnostics;

namespace BlueberryCore
{
    public static class TimeUtils
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static ulong frameCounter;

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public static void CountFrame()
        {
            ++frameCounter;
        }

        public static ulong CurrentFrame()
        {
            return frameCounter;
        }

        public static long NanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }
    }
}
